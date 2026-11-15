using System.Text.Json;
using GiaPha.BusinessObjects.DTOs.Imports;
using GiaPha.Repositories.Interfaces;
using GiaPha.Services.Interfaces;
using GiaPha.Services.Logging;

namespace GiaPha.Services.Handlers;

public sealed class ImportBatchChangeHandler : IChangeHandler
{
    private readonly IImportService _importService;
    private readonly IRepositoryManager _repository;
    private readonly ILoggerManager _logger;

    public ImportBatchChangeHandler(
        IImportService importService,
        IRepositoryManager repository,
        ILoggerManager logger)
    {
        _importService = importService;
        _repository = repository;
        _logger = logger;
    }

    public string Type => "ImportBatch";

    public Task<string> SummarizeAsync(string payload, CancellationToken cancellationToken = default)
    {
        try
        {
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;
            var count = 0;
            if (root.TryGetProperty("AcceptedRowIds", out var idsElement))
            {
                count = idsElement.GetArrayLength();
            }
            return Task.FromResult($"Nhập hàng loạt {count} nhân khẩu từ tệp đã duyệt vào cây gia phả.");
        }
        catch
        {
            return Task.FromResult("Nhập hàng loạt nhân khẩu từ tệp vào cây gia phả.");
        }
    }

    public async Task ApplyAsync(string payload, CancellationToken cancellationToken = default)
    {
        using var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;

        var batchId = root.GetProperty("BatchId").GetGuid();
        var acceptedRowIds = JsonSerializer.Deserialize<List<Guid>>(root.GetProperty("AcceptedRowIds").GetRawText())
            ?? new List<Guid>();

        var batch = await _repository.ImportBatch.GetByIdAsync(batchId, trackChanges: false, cancellationToken);
        if (batch is null)
        {
            throw new InvalidOperationException($"Không tìm thấy lô import {batchId} để phê duyệt.");
        }

        // Tạm chuyển trạng thái Clan sang Draft để commit trực tiếp, sau đó phục hồi Published
        var clan = await _repository.Clan.GetByIdAsync(batch.ClanId, trackChanges: true, cancellationToken);
        if (clan is not null)
        {
            var originalStatus = clan.Status;
            clan.Status = BusinessObjects.Entities.ClanStatus.Draft;
            await _repository.SaveAsync(cancellationToken);

            try
            {
                await _importService.CommitBatchAsync(batch.ClanId, batchId, acceptedRowIds, Guid.Empty, cancellationToken);
            }
            finally
            {
                clan.Status = originalStatus;
                await _repository.SaveAsync(cancellationToken);
            }
        }

        _logger.LogInfo($"Đã áp dụng phê duyệt ChangeRequest ImportBatch cho lô {batchId}.");
    }
}
