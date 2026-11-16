using GiaPha.BusinessObjects.DTOs.Exports;
using GiaPha.BusinessObjects.Entities;

namespace GiaPha.Services.Interfaces;

public interface IExportService
{
    /// <summary>
    /// Tạo yêu cầu xuất file và đưa vào hàng đợi Hangfire.
    /// </summary>
    Task<ExportJobDto> CreateExportJobAsync(
        Guid clanId,
        CreateExportRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy thông tin và tiến trình của một ExportJob.
    /// </summary>
    Task<ExportJobDto> GetExportJobAsync(
        Guid jobId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Worker thực thi tác vụ xuất ngầm (chạy qua Hangfire).
    /// </summary>
    Task ExecuteExportJobAsync(
        Guid jobId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sinh tệp PDF sách phả ký.
    /// </summary>
    byte[] GenerateGenealogyBookPdf(
        Clan clan,
        IReadOnlyList<Person> persons,
        IReadOnlyDictionary<Guid, Guid> childToFatherMap,
        IReadOnlyDictionary<Guid, List<Guid>> fatherToChildrenMap,
        IReadOnlyDictionary<Guid, List<(Person Wife, int Rank)>> husbandToWivesMap);

    /// <summary>
    /// Sinh tệp PDF phả đồ khổ lớn A1/A0 theo thuật toán Tidy Tree.
    /// </summary>
    byte[] GenerateLargeChartPdf(
        Clan clan,
        IReadOnlyList<Person> persons,
        IReadOnlyDictionary<Guid, Guid> childToFatherMap,
        IReadOnlyDictionary<Guid, List<Guid>> fatherToChildrenMap,
        string pageSize = "A0",
        Guid? branchId = null);
}
