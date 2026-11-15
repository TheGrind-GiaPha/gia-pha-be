using System.Text.Json;
using GiaPha.BusinessObjects.DTOs.Imports;
using GiaPha.BusinessObjects.DTOs.Persons;
using GiaPha.BusinessObjects.Entities;
using GiaPha.BusinessObjects.Exceptions;
using GiaPha.Repositories.Interfaces;
using GiaPha.Services.Interfaces;
using GiaPha.Services.Logging;

namespace GiaPha.Services.Implementations;

public sealed class ImportService : IImportService
{
    private readonly IRepositoryManager _repository;
    private readonly IExcelImportService _excelImportService;
    private readonly IWordImportService _wordImportService;
    private readonly ILlmExtractionService _llmExtractionService;
    private readonly IDuplicateDetectionService _duplicateDetectionService;
    private readonly IPersonService _personService;
    private readonly ILoggerManager _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ImportService(
        IRepositoryManager repository,
        IExcelImportService excelImportService,
        IWordImportService wordImportService,
        ILlmExtractionService llmExtractionService,
        IDuplicateDetectionService duplicateDetectionService,
        IPersonService personService,
        ILoggerManager logger)
    {
        _repository = repository;
        _excelImportService = excelImportService;
        _wordImportService = wordImportService;
        _llmExtractionService = llmExtractionService;
        _duplicateDetectionService = duplicateDetectionService;
        _personService = personService;
        _logger = logger;
    }

    public async Task<ImportBatchDto> UploadExcelAsync(
        Guid clanId,
        Stream stream,
        string fileName,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var clan = await _repository.Clan.GetByIdAsync(clanId, trackChanges: false, cancellationToken);
        if (clan is null)
        {
            throw new NotFoundException("Không tìm thấy dòng họ.");
        }

        var (rows, errors) = _excelImportService.ParseExcel(stream);
        if (errors.Count > 0)
        {
            var firstErrors = string.Join("; ", errors.Take(5).Select(e => $"Dòng {e.RowIndex} [{e.ColumnName}]: {e.ErrorMessage}"));
            throw new ArgumentException($"Tệp Excel chứa lỗi không hợp lệ: {firstErrors}");
        }

        var batch = new ImportBatch
        {
            ClanId = clanId,
            Source = ImportSource.Excel,
            Status = ImportBatchStatus.Parsed,
            FileName = fileName,
            TotalRows = rows.Count,
            CreatedByUserId = userId,
            ConsentGiven = true
        };
        _repository.ImportBatch.Create(batch);

        // Chuẩn bị dữ liệu so khớp trùng lặp
        var existingPersons = await _repository.Person.GetByClanAsync(clanId, trackChanges: false, cancellationToken);
        var existingParentChildMap = await GetParentChildMapAsync(existingPersons.Select(p => p.Id).ToList(), cancellationToken);

        var rowIndex = 1;
        foreach (var r in rows)
        {
            var birthYear = ExtractYear(r.NgaySinh);
            var (isDup, matchedId, warn) = _duplicateDetectionService.CheckDuplicate(
                r.HoTen,
                birthYear,
                r.MaCha,
                Guid.TryParse(r.MaCha, out var fGuid) ? fGuid : null,
                existingPersons,
                existingParentChildMap);

            var rowEntity = new ImportRow
            {
                BatchId = batch.Id,
                RowIndex = rowIndex++,
                ExtractedJson = JsonSerializer.Serialize(r, JsonOptions),
                Status = isDup ? ImportRowStatus.PossibleDuplicate : ImportRowStatus.New,
                MatchedPersonId = matchedId,
                Warnings = warn
            };
            _repository.ImportRow.Create(rowEntity);
            batch.Rows.Add(rowEntity);
        }

        await _repository.SaveAsync(cancellationToken);
        _logger.LogInfo($"Đã lưu lô nhập Excel {batch.Id} cho dòng họ {clanId} với {rows.Count} dòng.");

        return Map(batch);
    }

    public async Task<ImportBatchDto> UploadWordAsync(
        Guid clanId,
        Stream stream,
        string fileName,
        bool consentGiven,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (!consentGiven)
        {
            throw new InvalidOperationException("Cần có sự xác nhận đồng ý của đại diện dòng họ trước khi gửi dữ liệu sang dịch vụ AI bên ngoài.");
        }

        var clan = await _repository.Clan.GetByIdAsync(clanId, trackChanges: false, cancellationToken);
        if (clan is null)
        {
            throw new NotFoundException("Không tìm thấy dòng họ.");
        }

        var segments = _wordImportService.ExtractSegments(stream);
        if (segments.Count == 0)
        {
            throw new ArgumentException("Tệp Word không chứa nội dung hoặc không nhận diện được các đoạn phân theo đời.");
        }

        var batch = new ImportBatch
        {
            ClanId = clanId,
            Source = ImportSource.Word,
            Status = ImportBatchStatus.Parsing,
            FileName = fileName,
            ConsentGiven = true,
            CreatedByUserId = userId
        };
        _repository.ImportBatch.Create(batch);

        var existingPersons = await _repository.Person.GetByClanAsync(clanId, trackChanges: false, cancellationToken);
        var existingParentChildMap = await GetParentChildMapAsync(existingPersons.Select(p => p.Id).ToList(), cancellationToken);

        var rowIndex = 1;
        var totalExtracted = 0;

        foreach (var seg in segments)
        {
            if (string.IsNullOrWhiteSpace(seg.Content)) continue;

            try
            {
                var extractedPersons = await _llmExtractionService.ExtractPersonsAsync(
                    seg.Title,
                    seg.Content,
                    seg.GenerationNumber,
                    cancellationToken);

                foreach (var p in extractedPersons)
                {
                    totalExtracted++;
                    var (isDup, matchedId, warn) = _duplicateDetectionService.CheckDuplicate(
                        p.FullName,
                        p.Birth?.Year,
                        p.FatherTempId,
                        null,
                        existingPersons,
                        existingParentChildMap);

                    var rowEntity = new ImportRow
                    {
                        BatchId = batch.Id,
                        RowIndex = rowIndex++,
                        ExtractedJson = JsonSerializer.Serialize(p, JsonOptions),
                        Status = isDup ? ImportRowStatus.PossibleDuplicate : ImportRowStatus.New,
                        MatchedPersonId = matchedId,
                        Warnings = warn
                    };
                    _repository.ImportRow.Create(rowEntity);
                    batch.Rows.Add(rowEntity);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi gọi LLM bóc tách đoạn '{seg.Title}': {ex.Message}");
                var errorRow = new ImportRow
                {
                    BatchId = batch.Id,
                    RowIndex = rowIndex++,
                    ExtractedJson = JsonSerializer.Serialize(new { Title = seg.Title, Error = ex.Message }, JsonOptions),
                    Status = ImportRowStatus.Rejected,
                    Warnings = $"Lỗi trích xuất AI: {ex.Message}"
                };
                _repository.ImportRow.Create(errorRow);
                batch.Rows.Add(errorRow);
            }
        }

        batch.TotalRows = batch.Rows.Count;
        batch.Status = ImportBatchStatus.Parsed;
        await _repository.SaveAsync(cancellationToken);

        _logger.LogInfo($"Đã lưu lô nhập Word {batch.Id} cho dòng họ {clanId} với {batch.TotalRows} đối tượng.");
        return Map(batch);
    }

    public async Task<ImportBatchDto> GetBatchAsync(Guid clanId, Guid batchId, CancellationToken cancellationToken = default)
    {
        var batch = await _repository.ImportBatch.GetWithRowsAsync(batchId, trackChanges: false, cancellationToken);
        if (batch is null || batch.ClanId != clanId)
        {
            throw new NotFoundException("Không tìm thấy lô nhập liệu.");
        }

        return Map(batch);
    }

    public async Task<IReadOnlyList<ImportBatchDto>> GetBatchesByClanAsync(Guid clanId, CancellationToken cancellationToken = default)
    {
        var batches = await _repository.ImportBatch.GetByClanAsync(clanId, trackChanges: false, cancellationToken);
        return batches.Select(Map).ToList();
    }

    public async Task<ImportRowDto> UpdateRowAsync(
        Guid clanId,
        Guid batchId,
        Guid rowId,
        string updatedExtractedJson,
        ImportRowStatus status,
        CancellationToken cancellationToken = default)
    {
        var batch = await _repository.ImportBatch.GetByIdAsync(batchId, trackChanges: false, cancellationToken);
        if (batch is null || batch.ClanId != clanId)
        {
            throw new NotFoundException("Không tìm thấy lô nhập liệu.");
        }

        var row = await _repository.ImportRow.GetByIdAsync(rowId, trackChanges: true, cancellationToken);
        if (row is null || row.BatchId != batchId)
        {
            throw new NotFoundException("Không tìm thấy dòng dữ liệu staging.");
        }

        row.ExtractedJson = updatedExtractedJson;
        row.Status = status;
        row.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveAsync(cancellationToken);
        return Map(row);
    }

    public async Task<CommitImportResultDto> CommitBatchAsync(
        Guid clanId,
        Guid batchId,
        IReadOnlyList<Guid> acceptedRowIds,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var clan = await _repository.Clan.GetByIdAsync(clanId, trackChanges: true, cancellationToken);
        if (clan is null)
        {
            throw new NotFoundException("Không tìm thấy dòng họ.");
        }

        var batch = await _repository.ImportBatch.GetWithRowsAsync(batchId, trackChanges: true, cancellationToken);
        if (batch is null || batch.ClanId != clanId)
        {
            throw new NotFoundException("Không tìm thấy lô nhập liệu.");
        }

        if (acceptedRowIds.Count == 0)
        {
            throw new ArgumentException("Chưa có dòng nào được chọn để commit.");
        }

        var acceptedSet = acceptedRowIds.ToHashSet();
        var acceptedRows = batch.Rows.Where(r => acceptedSet.Contains(r.Id)).ToList();
        if (acceptedRows.Count == 0)
        {
            throw new ArgumentException("Các dòng được chọn không thuộc lô nhập liệu này.");
        }

        // Bóc tách đối tượng để kiểm tra tính toàn vẹn phụ thuộc cha - con
        var parsedCandidates = new List<NormalizedCandidate>();
        foreach (var r in acceptedRows)
        {
            var c = ParseCandidate(r, batch.Source);
            parsedCandidates.Add(c);
        }

        var candidateMap = parsedCandidates.ToDictionary(c => c.LocalId, StringComparer.OrdinalIgnoreCase);

        // Kiểm tra ràng buộc bất biến: Dòng được chọn mà cha chưa được chọn thì báo lỗi, không tạo dở dang!
        foreach (var c in parsedCandidates)
        {
            if (!string.IsNullOrWhiteSpace(c.FatherLocalId))
            {
                // Nếu cha là mã nội bộ trong file: cha BẮT BUỘC phải nằm trong acceptedRows
                var isInternalCode = batch.Rows.Any(r =>
                {
                    var parsed = ParseCandidate(r, batch.Source);
                    return string.Equals(parsed.LocalId, c.FatherLocalId, StringComparison.OrdinalIgnoreCase);
                });

                if (isInternalCode && !candidateMap.ContainsKey(c.FatherLocalId))
                {
                    throw new ArgumentException(
                        $"Dòng '{c.FullName}' (dòng #{c.RowIndex}) có cha là mã '{c.FatherLocalId}' chưa được chọn để nhập. Vui lòng chọn cả người cha hoặc bỏ chọn người con.");
                }
            }
        }

        // Nếu Clan đã Published: Gói thành ChangeRequest loại ImportBatch
        if (clan.Status == ClanStatus.Published)
        {
            var changeRequest = new ChangeRequest
            {
                ClanId = clanId,
                Type = "ImportBatch",
                Status = ChangeRequestStatus.Pending,
                RequesterId = userId,
                Summary = $"Nhập hàng loạt {acceptedRowIds.Count} người từ file vào dòng họ.",
                DueAt = DateTime.UtcNow.AddDays(7),
                PayloadJson = JsonSerializer.Serialize(new
                {
                    BatchId = batchId,
                    AcceptedRowIds = acceptedRowIds,
                    CandidateCount = parsedCandidates.Count
                }, JsonOptions)
            };

            _repository.ChangeRequest.Create(changeRequest);
            await _repository.SaveAsync(cancellationToken);

            _logger.LogInfo($"Dòng họ {clanId} đã Published. Đã tạo ChangeRequest {changeRequest.Id} cho lô import {batchId}.");
            return new CommitImportResultDto
            {
                IsPendingApproval = true,
                ChangeRequestId = changeRequest.Id,
                CreatedPersonsCount = 0,
                Message = $"Dòng họ đã xuất bản. Yêu cầu nhập liệu đã được chuyển thành Đề xuất thay đổi số #{changeRequest.Id} và đang chờ phê duyệt."
            };
        }

        // Nếu Clan đang là Draft: Thực thi tạo người trực tiếp theo thứ tự cha trước con
        var createdCount = await ApplyCandidatesAsync(clanId, parsedCandidates, cancellationToken);

        // Cập nhật trạng thái các dòng trong batch
        foreach (var row in batch.Rows)
        {
            if (acceptedSet.Contains(row.Id))
            {
                row.Status = ImportRowStatus.Accepted;
            }
            else if (row.Status == ImportRowStatus.New || row.Status == ImportRowStatus.PossibleDuplicate)
            {
                row.Status = ImportRowStatus.Rejected;
            }
        }

        batch.AcceptedRowsCount = acceptedRows.Count;
        batch.Status = ImportBatchStatus.Committed;
        batch.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveAsync(cancellationToken);

        _logger.LogInfo($"Đã commit thành công lô {batchId} vào dòng họ {clanId} với {createdCount} người mới.");
        return new CommitImportResultDto
        {
            IsPendingApproval = false,
            CreatedPersonsCount = createdCount,
            Message = $"Đã nhập thành công {createdCount} người vào cây gia phả."
        };
    }

    private async Task<int> ApplyCandidatesAsync(
        Guid clanId,
        List<NormalizedCandidate> candidates,
        CancellationToken cancellationToken)
    {
        // Sắp xếp topo (Topological sort) để cha luôn đứng trước con
        var sorted = TopoSortCandidates(candidates);
        var localIdToRealGuid = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        var existingPersons = await _repository.Person.GetByClanAsync(clanId, trackChanges: false, cancellationToken);
        var existingMap = existingPersons.ToDictionary(p => p.Id);

        var createdCount = 0;

        foreach (var c in sorted)
        {
            Guid? realFatherId = null;

            if (!string.IsNullOrWhiteSpace(c.FatherLocalId))
            {
                if (localIdToRealGuid.TryGetValue(c.FatherLocalId, out var mappedId))
                {
                    realFatherId = mappedId;
                }
                else if (Guid.TryParse(c.FatherLocalId, out var directGuid) && existingMap.ContainsKey(directGuid))
                {
                    realFatherId = directGuid;
                }
            }

            if (realFatherId.HasValue)
            {
                // Thêm con thông qua IPersonService
                var addedList = await _personService.AddChildrenAsync(
                    clanId,
                    realFatherId.Value,
                    new AddChildrenRequest
                    {
                        Children =
                        {
                            new AddChildItem
                            {
                                FullName = c.FullName,
                                Gender = c.Gender,
                                BirthDate = c.BirthDate,
                                RelationType = c.RelationType
                            }
                        }
                    },
                    cancellationToken);

                var createdChild = addedList.First();
                localIdToRealGuid[c.LocalId] = createdChild.Id;
                createdCount++;

                // Thêm phối ngẫu nếu có
                await AddCandidateSpousesAsync(clanId, createdChild.Id, c.Spouses, cancellationToken);
            }
            else
            {
                // Cụ tổ / Root person
                var rootPerson = new Person
                {
                    ClanId = clanId,
                    FullName = c.FullName,
                    Gender = c.Gender,
                    BirthDate = MapDate(c.BirthDate),
                    DeathDate = MapDate(c.DeathDate),
                    IsLiving = c.DeathDate is null && (c.BirthDate?.Year ?? 0) >= 1930,
                    Generation = 1,
                    Status = PersonStatus.Active,
                    PrivacyLevel = PrivacyLevel.Public,
                    Version = 1
                };
                rootPerson.Path = $"/{rootPerson.Id}/";

                _repository.Person.Create(rootPerson);
                await _repository.SaveAsync(cancellationToken);

                localIdToRealGuid[c.LocalId] = rootPerson.Id;
                createdCount++;

                await AddCandidateSpousesAsync(clanId, rootPerson.Id, c.Spouses, cancellationToken);
            }
        }

        return createdCount;
    }

    private async Task AddCandidateSpousesAsync(
        Guid clanId,
        Guid husbandId,
        List<CandidateSpouse> spouses,
        CancellationToken cancellationToken)
    {
        foreach (var s in spouses)
        {
            if (string.IsNullOrWhiteSpace(s.FullName)) continue;

            // Tạo Person cho vợ
            var wife = new Person
            {
                ClanId = clanId,
                FullName = s.FullName,
                Gender = Gender.Female,
                IsLiving = false,
                Generation = 1,
                Status = PersonStatus.Active,
                PrivacyLevel = PrivacyLevel.Public,
                Version = 1
            };
            wife.Path = $"/{wife.Id}/";
            _repository.Person.Create(wife);
            await _repository.SaveAsync(cancellationToken);

            // Thêm Union
            await _personService.AddUnionAsync(clanId, husbandId, new AddUnionRequest
            {
                WifeId = wife.Id,
                Rank = s.Rank <= 0 ? 1 : s.Rank
            }, cancellationToken);
        }
    }

    private static List<NormalizedCandidate> TopoSortCandidates(List<NormalizedCandidate> items)
    {
        var result = new List<NormalizedCandidate>();
        var itemMap = items.ToDictionary(x => x.LocalId, StringComparer.OrdinalIgnoreCase);
        var inDegree = items.ToDictionary(x => x.LocalId, _ => 0, StringComparer.OrdinalIgnoreCase);

        foreach (var item in items)
        {
            if (!string.IsNullOrWhiteSpace(item.FatherLocalId) && itemMap.ContainsKey(item.FatherLocalId))
            {
                inDegree[item.LocalId]++;
            }
        }

        var queue = new Queue<NormalizedCandidate>(items.Where(x => inDegree[x.LocalId] == 0));

        while (queue.Count > 0)
        {
            var curr = queue.Dequeue();
            result.Add(curr);

            foreach (var child in items.Where(x => string.Equals(x.FatherLocalId, curr.LocalId, StringComparison.OrdinalIgnoreCase)))
            {
                inDegree[child.LocalId]--;
                if (inDegree[child.LocalId] == 0)
                {
                    queue.Enqueue(child);
                }
            }
        }

        // Nếu còn sót do chu trình (cycle), bổ sung nốt vào cuối
        if (result.Count < items.Count)
        {
            var remaining = items.Where(x => !result.Contains(x));
            result.AddRange(remaining);
        }

        return result;
    }

    private static NormalizedCandidate ParseCandidate(ImportRow row, ImportSource source)
    {
        if (source == ImportSource.Excel)
        {
            var data = JsonSerializer.Deserialize<ExcelRowData>(row.ExtractedJson, JsonOptions) ?? new();
            var candidate = new NormalizedCandidate
            {
                LocalId = string.IsNullOrWhiteSpace(data.MaDong) ? row.Id.ToString() : data.MaDong,
                RowIndex = row.RowIndex,
                FullName = data.HoTen,
                Gender = ParseGender(data.GioiTinh),
                BirthDate = ParseDateDto(data.NgaySinh, data.LoaiLich),
                DeathDate = ParseDateDto(data.NgayMat, data.LoaiLich),
                FatherLocalId = data.MaCha,
                RelationType = ParseRelationType(data.LoaiCon)
            };

            if (data.ThuTuVo.HasValue)
            {
                // Nếu bản ghi có ThuTuVo và MaCha/MaMe
            }

            return candidate;
        }
        else
        {
            var p = JsonSerializer.Deserialize<LlmPersonExtracted>(row.ExtractedJson, JsonOptions) ?? new();
            var candidate = new NormalizedCandidate
            {
                LocalId = string.IsNullOrWhiteSpace(p.TempId) ? row.Id.ToString() : p.TempId,
                RowIndex = row.RowIndex,
                FullName = p.FullName,
                Gender = ParseGender(p.Gender),
                BirthDate = MapLlmDate(p.Birth),
                DeathDate = MapLlmDate(p.Death),
                FatherLocalId = p.FatherTempId,
                RelationType = RelationType.Biological
            };

            if (p.Spouses is not null)
            {
                foreach (var sp in p.Spouses)
                {
                    candidate.Spouses.Add(new CandidateSpouse { FullName = sp.FullName, Rank = sp.Rank });
                }
            }

            return candidate;
        }
    }

    private async Task<Dictionary<Guid, Guid>> GetParentChildMapAsync(List<Guid> personIds, CancellationToken cancellationToken)
    {
        var map = new Dictionary<Guid, Guid>();
        if (personIds.Count == 0) return map;

        var relations = await _repository.ParentChild.GetByChildIdsAsync(personIds, trackChanges: false, cancellationToken);
        foreach (var rel in relations)
        {
            map[rel.ChildId] = rel.ParentId;
        }
        return map;
    }

    private static Gender ParseGender(string? g)
    {
        if (string.IsNullOrWhiteSpace(g)) return Gender.Male;
        var lower = g.Trim().ToLowerInvariant();
        if (lower is "nữ" or "nu" or "female") return Gender.Female;
        if (lower is "khác" or "khac" or "other") return Gender.Other;
        return Gender.Male;
    }

    private static RelationType ParseRelationType(string? r)
    {
        if (string.IsNullOrWhiteSpace(r)) return RelationType.Biological;
        var lower = r.Trim().ToLowerInvariant();
        if (lower.Contains("nuôi") || lower.Contains("adopted")) return RelationType.Adopted;
        if (lower.Contains("thừa tự") || lower.Contains("heir")) return RelationType.Heir;
        return RelationType.Biological;
    }

    private static FlexibleDateDto? ParseDateDto(string? dateStr, string? calendarStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr)) return null;

        var cal = CalendarType.Solar;
        if (!string.IsNullOrWhiteSpace(calendarStr) && calendarStr.Contains("âm", StringComparison.OrdinalIgnoreCase))
        {
            cal = CalendarType.Lunar;
        }

        if (int.TryParse(dateStr.Trim(), out var yearOnly) && yearOnly is >= 1000 and <= 2100)
        {
            return new FlexibleDateDto { Year = yearOnly, Calendar = cal };
        }

        if (DateTime.TryParseExact(dateStr.Trim(), ["d/M/yyyy", "dd/MM/yyyy", "yyyy-MM-dd"],
            System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var dt))
        {
            return new FlexibleDateDto
            {
                Year = dt.Year,
                Month = dt.Month,
                Day = dt.Day,
                Calendar = cal
            };
        }

        return null;
    }

    private static FlexibleDateDto? MapLlmDate(LlmDateExtracted? d)
    {
        if (d is null) return null;
        return new FlexibleDateDto
        {
            Year = d.Year,
            Month = d.Month,
            Day = d.Day,
            Calendar = string.Equals(d.Calendar, "Lunar", StringComparison.OrdinalIgnoreCase) ? CalendarType.Lunar : CalendarType.Solar
        };
    }

    private static FlexibleDate? MapDate(FlexibleDateDto? dto)
    {
        if (dto is null) return null;
        return new FlexibleDate
        {
            Year = dto.Year,
            Month = dto.Month,
            Day = dto.Day,
            Calendar = dto.Calendar,
            IsLeapMonth = dto.IsLeapMonth
        };
    }

    private static int? ExtractYear(string? dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr)) return null;
        if (int.TryParse(dateStr.Trim(), out var yr) && yr is >= 1000 and <= 2100) return yr;
        if (DateTime.TryParseExact(dateStr.Trim(), ["d/M/yyyy", "dd/MM/yyyy", "yyyy-MM-dd"],
            System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var dt))
        {
            return dt.Year;
        }
        return null;
    }

    private static ImportBatchDto Map(ImportBatch b) => new()
    {
        Id = b.Id,
        ClanId = b.ClanId,
        Source = b.Source,
        Status = b.Status,
        FileName = b.FileName,
        TotalRows = b.TotalRows,
        AcceptedRowsCount = b.AcceptedRowsCount,
        ConsentGiven = b.ConsentGiven,
        CreatedAt = b.CreatedAt,
        Rows = b.Rows?.Select(Map).ToList() ?? new List<ImportRowDto>()
    };

    private static ImportRowDto Map(ImportRow r) => new()
    {
        Id = r.Id,
        BatchId = r.BatchId,
        RowIndex = r.RowIndex,
        ExtractedJson = r.ExtractedJson,
        Status = r.Status,
        MatchedPersonId = r.MatchedPersonId,
        Warnings = r.Warnings,
        CreatedAt = r.CreatedAt
    };

    private sealed class NormalizedCandidate
    {
        public string LocalId { get; set; } = string.Empty;
        public int RowIndex { get; set; }
        public string FullName { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public FlexibleDateDto? BirthDate { get; set; }
        public FlexibleDateDto? DeathDate { get; set; }
        public string? FatherLocalId { get; set; }
        public RelationType RelationType { get; set; } = RelationType.Biological;
        public List<CandidateSpouse> Spouses { get; set; } = new();
    }

    private sealed class CandidateSpouse
    {
        public string FullName { get; set; } = string.Empty;
        public int Rank { get; set; } = 1;
    }
}
