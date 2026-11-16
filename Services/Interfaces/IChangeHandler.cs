namespace GiaPha.Services.Interfaces;

/// <summary>
/// Hợp đồng handler cho từng loại thay đổi (ChangeRequest.Type).
/// Interface do Luồng 1 chủ trì; Luồng 2 chỉ cài các handler cho thao tác cây.
/// Cần đối chiếu lại với interface chính thức của Luồng 1 khi Luồng 1 code.
/// </summary>
public interface IChangeHandler
{
    string Type { get; }

    /// <summary>Sinh câu tóm tắt tiếng Việt cho người duyệt lớn tuổi đọc.</summary>
    Task<string> SummarizeAsync(string payload, CancellationToken cancellationToken = default);

    /// <summary>Áp dụng thay đổi vào dữ liệu cây.</summary>
    Task ApplyAsync(string payload, CancellationToken cancellationToken = default);
}
