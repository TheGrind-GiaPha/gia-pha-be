using GiaPha.BusinessObjects.DTOs.Imports;
using GiaPha.BusinessObjects.Entities;

namespace GiaPha.Services.Interfaces;

public interface IImportService
{
    Task<ImportBatchDto> UploadExcelAsync(
        Guid clanId,
        Stream stream,
        string fileName,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<ImportBatchDto> UploadWordAsync(
        Guid clanId,
        Stream stream,
        string fileName,
        bool consentGiven,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<ImportBatchDto> GetBatchAsync(
        Guid clanId,
        Guid batchId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ImportBatchDto>> GetBatchesByClanAsync(
        Guid clanId,
        CancellationToken cancellationToken = default);

    Task<ImportRowDto> UpdateRowAsync(
        Guid clanId,
        Guid batchId,
        Guid rowId,
        string updatedExtractedJson,
        ImportRowStatus status,
        CancellationToken cancellationToken = default);

    Task<CommitImportResultDto> CommitBatchAsync(
        Guid clanId,
        Guid batchId,
        IReadOnlyList<Guid> acceptedRowIds,
        Guid userId,
        CancellationToken cancellationToken = default);
}
