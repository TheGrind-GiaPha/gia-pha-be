using GiaPha.BusinessObjects.DTOs.Imports;

namespace GiaPha.Services.Interfaces;

public interface ILlmExtractionService
{
    /// <summary>
    /// Gửi văn bản đoạn đời tới Google Gemini API để trích xuất danh sách nhân khẩu theo cấu trúc JSON.
    /// Tự động che số điện thoại và retry 1 lần nếu cú pháp JSON trả về bị sai.
    /// </summary>
    Task<IReadOnlyList<LlmPersonExtracted>> ExtractPersonsAsync(
        string segmentTitle,
        string segmentText,
        int generationNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Khử định dạng và che số điện thoại (masking) trước khi gửi dữ liệu ra bên ngoài.
    /// </summary>
    string MaskSensitiveData(string input);
}
