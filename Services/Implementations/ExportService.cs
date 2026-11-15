using GiaPha.BusinessObjects.DTOs.Exports;
using GiaPha.BusinessObjects.Entities;
using GiaPha.BusinessObjects.Exceptions;
using GiaPha.Repositories.Interfaces;
using GiaPha.Services.Interfaces;
using GiaPha.Services.Logging;
using GiaPha.Services.Pdf;
using Hangfire;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace GiaPha.Services.Implementations;

public sealed class ExportService : IExportService
{
    private readonly IRepositoryManager _repository;
    private readonly ILoggerManager _logger;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private static bool _fontsRegistered;

    public ExportService(
        IRepositoryManager repository,
        ILoggerManager logger,
        IBackgroundJobClient backgroundJobClient)
    {
        _repository = repository;
        _logger = logger;
        _backgroundJobClient = backgroundJobClient;
        EnsureQuestPdfConfigured();
    }

    private static void EnsureQuestPdfConfigured()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        QuestPDF.Settings.ThrowOnMissingFontFamilies = false;

        if (_fontsRegistered) return;

        try
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var candidateDirs = new[]
            {
                Path.Combine(baseDir, "Resources", "Fonts"),
                Path.Combine(baseDir, "wwwroot", "fonts"),
                Path.Combine(baseDir, "..", "..", "..", "..", "Services", "Resources", "Fonts"),
                Path.Combine(baseDir, "..", "..", "..", "..", "APIs", "wwwroot", "fonts"),
                "d:\\Download\\gia-pha-be\\Services\\Resources\\Fonts",
                "d:\\Download\\gia-pha-be\\APIs\\wwwroot\\fonts"
            };

            foreach (var dir in candidateDirs)
            {
                if (Directory.Exists(dir))
                {
                    foreach (var fontFile in Directory.GetFiles(dir, "*.ttf"))
                    {
                        try
                        {
                            using var stream = File.OpenRead(fontFile);
                            FontManager.RegisterFontFromStream(stream);
                        }
                        catch
                        {
                            // Ignore duplicates
                        }
                    }
                }
            }
            _fontsRegistered = true;
        }
        catch
        {
            // Fallback nếu hệ thống đã có font hoặc không mở được stream
        }
    }

    public async Task<ExportJobDto> CreateExportJobAsync(
        Guid clanId,
        CreateExportRequest request,
        CancellationToken cancellationToken = default)
    {
        var clan = await _repository.Clan.GetByIdAsync(clanId, trackChanges: false, cancellationToken);
        if (clan is null)
        {
            throw new NotFoundException("Không tìm thấy dòng họ.");
        }

        var job = new ExportJob
        {
            ClanId = clanId,
            Type = request.Type,
            BranchId = request.BranchId,
            Status = ExportJobStatus.Queued,
            ProgressPercentage = 0,
            FileName = $"{clan.Name.Replace(" ", "_")}_{(request.Type == ExportType.Book ? "PhaKy" : "PhaDo")}_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf"
        };

        _repository.ExportJob.Create(job);
        await _repository.SaveAsync(cancellationToken);

        // Đẩy vào hàng đợi Hangfire
        _backgroundJobClient.Enqueue<IExportService>(x => x.ExecuteExportJobAsync(job.Id, CancellationToken.None));

        _logger.LogInfo($"Đã tạo ExportJob {job.Id} loại {job.Type} cho dòng họ {clanId}.");
        return Map(job);
    }

    public async Task<ExportJobDto> GetExportJobAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        var job = await _repository.ExportJob.GetByIdAsync(jobId, trackChanges: false, cancellationToken);
        if (job is null)
        {
            throw new NotFoundException("Không tìm thấy tác vụ xuất file.");
        }

        return Map(job);
    }

    public async Task ExecuteExportJobAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        var job = await _repository.ExportJob.GetByIdAsync(jobId, trackChanges: true, cancellationToken);
        if (job is null) return;

        try
        {
            job.Status = ExportJobStatus.Processing;
            job.ProgressPercentage = 20;
            await _repository.SaveAsync(cancellationToken);

            var clan = await _repository.Clan.GetByIdAsync(job.ClanId, trackChanges: false, cancellationToken)
                ?? throw new InvalidOperationException("Không tìm thấy thông tin dòng họ.");

            var persons = await _repository.Person.GetByClanAsync(job.ClanId, trackChanges: false, cancellationToken);
            var personIds = persons.Select(p => p.Id).ToList();

            job.ProgressPercentage = 40;
            await _repository.SaveAsync(cancellationToken);

            var parentChildList = await _repository.ParentChild.GetByChildIdsAsync(personIds, trackChanges: false, cancellationToken);
            var childToFatherMap = parentChildList.ToDictionary(r => r.ChildId, r => r.ParentId);

            var fatherToChildrenMap = new Dictionary<Guid, List<Guid>>();
            foreach (var rel in parentChildList)
            {
                if (!fatherToChildrenMap.TryGetValue(rel.ParentId, out var list))
                {
                    list = new List<Guid>();
                    fatherToChildrenMap[rel.ParentId] = list;
                }
                list.Add(rel.ChildId);
            }

            var husbandToWivesMap = new Dictionary<Guid, List<(Person Wife, int Rank)>>();
            var personMap = persons.ToDictionary(p => p.Id);

            foreach (var p in persons.Where(p => p.Gender == Gender.Male))
            {
                var unions = await _repository.Union.GetByHusbandAsync(p.Id, trackChanges: false, cancellationToken);
                var wives = unions
                    .Where(u => personMap.ContainsKey(u.WifeId))
                    .Select(u => (Wife: personMap[u.WifeId], Rank: u.Rank))
                    .OrderBy(w => w.Rank)
                    .ToList();

                if (wives.Count > 0)
                {
                    husbandToWivesMap[p.Id] = wives;
                }
            }

            job.ProgressPercentage = 70;
            await _repository.SaveAsync(cancellationToken);

            byte[] pdfBytes;
            if (job.Type == ExportType.Book)
            {
                pdfBytes = GenerateGenealogyBookPdf(
                    clan,
                    persons,
                    childToFatherMap,
                    fatherToChildrenMap,
                    husbandToWivesMap);
            }
            else
            {
                pdfBytes = GenerateLargeChartPdf(
                    clan,
                    persons,
                    childToFatherMap,
                    fatherToChildrenMap,
                    "A0",
                    job.BranchId);
            }

            // Lưu file ra ổ đĩa
            var exportDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "exports");
            if (!Directory.Exists(exportDir))
            {
                Directory.CreateDirectory(exportDir);
            }

            var filePath = Path.Combine(exportDir, job.FileName!);
            await File.WriteAllBytesAsync(filePath, pdfBytes, cancellationToken);

            job.FilePath = filePath;
            job.FileSizeBytes = pdfBytes.Length;
            job.Status = ExportJobStatus.Completed;
            job.ProgressPercentage = 100;
            job.FinishedAt = DateTime.UtcNow;

            await _repository.SaveAsync(cancellationToken);
            _logger.LogInfo($"Hoàn thành xuất PDF cho ExportJob {job.Id} (Kích thước: {pdfBytes.Length / 1024} KB).");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Thất bại khi thực hiện ExportJob {job.Id}: {ex.Message}");
            job.Status = ExportJobStatus.Failed;
            job.ErrorMessage = ex.Message;
            job.FinishedAt = DateTime.UtcNow;
            await _repository.SaveAsync(CancellationToken.None);
        }
    }

    public byte[] GenerateGenealogyBookPdf(
        Clan clan,
        IReadOnlyList<Person> persons,
        IReadOnlyDictionary<Guid, Guid> childToFatherMap,
        IReadOnlyDictionary<Guid, List<Guid>> fatherToChildrenMap,
        IReadOnlyDictionary<Guid, List<(Person Wife, int Rank)>> husbandToWivesMap)
    {
        EnsureQuestPdfConfigured();
        var document = new GenealogyBookDocument(
            clan,
            persons,
            childToFatherMap,
            fatherToChildrenMap,
            husbandToWivesMap);

        return document.GeneratePdf();
    }

    public byte[] GenerateLargeChartPdf(
        Clan clan,
        IReadOnlyList<Person> persons,
        IReadOnlyDictionary<Guid, Guid> childToFatherMap,
        IReadOnlyDictionary<Guid, List<Guid>> fatherToChildrenMap,
        string pageSize = "A0",
        Guid? branchId = null)
    {
        EnsureQuestPdfConfigured();

        var personMap = persons.ToDictionary(p => p.Id);
        var roots = persons.Where(p => !childToFatherMap.ContainsKey(p.Id)).ToList();
        if (roots.Count == 0 && persons.Count > 0)
        {
            roots.Add(persons.OrderBy(p => p.Generation).First());
        }

        var pagesNodes = new List<List<TreeNode>>();
        var pageTitles = new List<string>();

        // Xây dựng cây phân nhánh
        foreach (var rootPerson in roots)
        {
            var rootNode = BuildTreeNode(rootPerson, fatherToChildrenMap, personMap);
            var computedNodes = TidyTreeLayout.ComputeLayout(rootNode, startX: 60f, startY: 80f);

            // Kiểm tra xem cây có quá rộng để tách chi không (chiều rộng tối đa A0 landscape ~ 3300pt)
            var maxNodeX = computedNodes.Max(n => n.X + n.Width);
            var needBranchSplitting = maxNodeX > 3200f && rootNode.Children.Count > 1;

            if (needBranchSplitting)
            {
                // Tờ 1: Tổng quan Đời 1 đến Đời 3
                var masterRoot = ShallowCloneTree(rootNode, maxDepth: 3);
                var masterNodes = TidyTreeLayout.ComputeLayout(masterRoot, startX: 60f, startY: 80f);
                pagesNodes.Add(masterNodes);
                pageTitles.Add("Tờ 1: Cây Tổng quan Đời 1 đến Đời 3");

                // Tờ 2..N: Từng Chi họ (từ các con ở đời 2 hoặc đời 3)
                var branchIdx = 1;
                foreach (var branchChild in rootNode.Children)
                {
                    branchChild.Parent = null; // Tách cành
                    var branchNodes = TidyTreeLayout.ComputeLayout(branchChild, startX: 60f, startY: 80f);
                    pagesNodes.Add(branchNodes);
                    pageTitles.Add($"Tờ {++branchIdx}: Chi họ {branchChild.Person.FullName}");
                }
            }
            else
            {
                // Toàn bộ trên 1 tờ
                pagesNodes.Add(computedNodes);
                pageTitles.Add($"Phả đồ Chi phái {rootPerson.FullName}");
            }
        }

        var document = new GenealogyChartDocument(clan, pagesNodes, pageTitles, pageSize);
        return document.GeneratePdf();
    }

    private static TreeNode BuildTreeNode(
        Person person,
        IReadOnlyDictionary<Guid, List<Guid>> fatherToChildrenMap,
        Dictionary<Guid, Person> personMap)
    {
        var node = new TreeNode { Person = person };

        if (fatherToChildrenMap.TryGetValue(person.Id, out var childIds))
        {
            foreach (var cId in childIds)
            {
                if (personMap.TryGetValue(cId, out var childPerson))
                {
                    var childNode = BuildTreeNode(childPerson, fatherToChildrenMap, personMap);
                    childNode.Parent = node;
                    node.Children.Add(childNode);
                }
            }
        }

        return node;
    }

    private static TreeNode ShallowCloneTree(TreeNode original, int maxDepth, int currentDepth = 1)
    {
        var copy = new TreeNode
        {
            Person = original.Person,
            Width = original.Width,
            Height = original.Height
        };

        if (currentDepth < maxDepth)
        {
            foreach (var c in original.Children)
            {
                var childCopy = ShallowCloneTree(c, maxDepth, currentDepth + 1);
                childCopy.Parent = copy;
                copy.Children.Add(childCopy);
            }
        }

        return copy;
    }

    private static ExportJobDto Map(ExportJob j) => new()
    {
        Id = j.Id,
        ClanId = j.ClanId,
        Type = j.Type,
        BranchId = j.BranchId,
        Status = j.Status,
        ProgressPercentage = j.ProgressPercentage,
        FileName = j.FileName,
        FileSizeBytes = j.FileSizeBytes,
        DownloadUrl = j.Status == ExportJobStatus.Completed ? $"/api/exports/{j.Id}/download" : null,
        ErrorMessage = j.ErrorMessage,
        CreatedAt = j.CreatedAt,
        FinishedAt = j.FinishedAt
    };
}
