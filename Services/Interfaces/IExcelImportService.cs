using GiaPha.BusinessObjects.DTOs.Imports;

namespace GiaPha.Services.Interfaces;

public interface IExcelImportService
{
    /// <summary>
    /// Đọc và kiểm tra từng dòng từ stream tệp Excel.
    /// Nếu có lỗi cú pháp/ràng buộc, trả về danh sách lỗi kèm số dòng.
    /// Nếu hợp lệ, trả về danh sách các đối tượng dữ liệu dòng.
    /// </summary>
    (IReadOnlyList<ExcelRowData> Rows, IReadOnlyList<ImportRowValidationError> Errors) ParseExcel(Stream stream);

    /// <summary>
    /// Tạo file Excel template mẫu 10 cột để người dùng tải về.
    /// </summary>
    byte[] GenerateTemplate();
}
