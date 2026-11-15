using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using GiaPha.BusinessObjects.DTOs.Imports;
using GiaPha.Services.Interfaces;

namespace GiaPha.Services.Implementations;

public sealed class ExcelImportService : IExcelImportService
{
    private static readonly string[] ExpectedHeaders =
    [
        "MaDong", "HoTen", "GioiTinh", "NgaySinh", "LoaiLich",
        "NgayMat", "MaCha", "MaMe", "LoaiCon", "ThuTuVo"
    ];

    private sealed class ColumnIndices
    {
        public int? MaDong { get; set; }
        public int HoTen { get; set; } = -1;
        public int? GioiTinh { get; set; }
        public int? NgaySinh { get; set; }
        public int? LoaiLich { get; set; }
        public int? NgayMat { get; set; }
        public int? MaCha { get; set; }
        public int? MaMe { get; set; }
        public int? LoaiCon { get; set; }
        public int? ThuTuVo { get; set; }
    }

    public (IReadOnlyList<ExcelRowData> Rows, IReadOnlyList<ImportRowValidationError> Errors) ParseExcel(Stream stream)
    {
        var rows = new List<ExcelRowData>();
        var errors = new List<ImportRowValidationError>();

        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.FirstOrDefault();
        if (worksheet is null)
        {
            errors.Add(new ImportRowValidationError
            {
                RowIndex = 0,
                ColumnName = "Worksheet",
                ErrorMessage = "Tệp Excel không chứa sheet dữ liệu nào."
            });
            return (rows, errors);
        }

        var (headerRowIndex, colMap) = DetectHeaderRowAndColumns(worksheet);
        if (headerRowIndex == -1 || colMap.HoTen == -1)
        {
            errors.Add(new ImportRowValidationError
            {
                RowIndex = 1,
                ColumnName = "Header",
                ErrorMessage = "Không tìm thấy dòng tiêu đề hợp lệ (cần ít nhất cột 'Họ và tên' và các cột nhân khẩu)."
            });
            return (rows, errors);
        }

        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? headerRowIndex;
        var seenCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var autoCodeIndex = 1;

        for (var rowNum = headerRowIndex + 1; rowNum <= lastRow; rowNum++)
        {
            var row = worksheet.Row(rowNum);
            if (row.IsEmpty()) continue;

            var hoTen = GetCellText(row.Cell(colMap.HoTen));
            // Nếu họ tên trống hoàn toàn thì bỏ qua dòng trống
            if (string.IsNullOrWhiteSpace(hoTen)) continue;

            var rawMaDong = colMap.MaDong.HasValue ? GetCellText(row.Cell(colMap.MaDong.Value)) : string.Empty;
            var rawGioiTinh = colMap.GioiTinh.HasValue ? GetCellText(row.Cell(colMap.GioiTinh.Value)) : "Nam";
            var rawNgaySinh = colMap.NgaySinh.HasValue ? GetCellText(row.Cell(colMap.NgaySinh.Value)) : null;
            var rawLoaiLich = colMap.LoaiLich.HasValue ? GetCellText(row.Cell(colMap.LoaiLich.Value)) : null;
            var rawNgayMat = colMap.NgayMat.HasValue ? GetCellText(row.Cell(colMap.NgayMat.Value)) : null;
            var rawMaCha = colMap.MaCha.HasValue ? GetCellText(row.Cell(colMap.MaCha.Value)) : null;
            var rawMaMe = colMap.MaMe.HasValue ? GetCellText(row.Cell(colMap.MaMe.Value)) : null;
            var rawLoaiCon = colMap.LoaiCon.HasValue ? GetCellText(row.Cell(colMap.LoaiCon.Value)) : null;
            var rawThuTuVo = colMap.ThuTuVo.HasValue ? GetCellText(row.Cell(colMap.ThuTuVo.Value)) : null;

            // Xử lý MaDong
            var maDong = string.IsNullOrWhiteSpace(rawMaDong) ? autoCodeIndex.ToString() : rawMaDong;
            if (!seenCodes.Add(maDong))
            {
                // Nếu bị trùng mã, sinh mã đuôi
                maDong = $"{maDong}_{rowNum}";
                seenCodes.Add(maDong);
            }
            autoCodeIndex++;

            // Xử lý Giới tính
            var gioiTinh = NormalizeGender(rawGioiTinh);

            // Xử lý Loại lịch
            var loaiLich = NormalizeCalendar(rawLoaiLich);

            // Xử lý Loại con
            var loaiCon = string.IsNullOrWhiteSpace(rawLoaiCon) ? "Con ruột" : rawLoaiCon;

            // Xử lý Mã cha / Mẹ
            var maCha = CleanReferenceCode(rawMaCha);
            var maMe = CleanReferenceCode(rawMaMe);

            // Không được tự làm cha của chính mình
            if (!string.IsNullOrWhiteSpace(maCha) && string.Equals(maCha, maDong, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(new ImportRowValidationError
                {
                    RowIndex = rowNum,
                    ColumnName = "MaCha",
                    ErrorMessage = "Mã cha (MaCha) không được trùng với mã dòng của chính người này."
                });
                maCha = null;
            }

            // Xử lý Thứ tự vợ
            int? thuTuVo = null;
            if (!string.IsNullOrWhiteSpace(rawThuTuVo))
            {
                if (int.TryParse(rawThuTuVo, out var parsedRank) && parsedRank >= 1)
                {
                    thuTuVo = parsedRank;
                }
                else if (Regex.IsMatch(rawThuTuVo, @"\d+"))
                {
                    var match = Regex.Match(rawThuTuVo, @"\d+");
                    if (int.TryParse(match.Value, out var num) && num >= 1)
                    {
                        thuTuVo = num;
                    }
                }
                else
                {
                    thuTuVo = 1;
                }
            }

            rows.Add(new ExcelRowData
            {
                MaDong = maDong,
                HoTen = hoTen,
                GioiTinh = gioiTinh,
                NgaySinh = string.IsNullOrWhiteSpace(rawNgaySinh) ? null : rawNgaySinh,
                LoaiLich = loaiLich,
                NgayMat = string.IsNullOrWhiteSpace(rawNgayMat) ? null : rawNgayMat,
                MaCha = maCha,
                MaMe = maMe,
                LoaiCon = loaiCon,
                ThuTuVo = thuTuVo
            });
        }

        return (rows, errors);
    }

    private static (int HeaderRow, ColumnIndices Map) DetectHeaderRowAndColumns(IXLWorksheet worksheet)
    {
        var maxScanRow = Math.Min(15, worksheet.LastRowUsed()?.RowNumber() ?? 1);

        for (var r = 1; r <= maxScanRow; r++)
        {
            var row = worksheet.Row(r);
            if (row.IsEmpty()) continue;

            var map = new ColumnIndices();
            var maxCol = Math.Min(30, row.LastCellUsed()?.Address.ColumnNumber ?? 1);
            var matchCount = 0;

            for (var c = 1; c <= maxCol; c++)
            {
                var text = NormalizeHeader(GetCellText(row.Cell(c)));
                if (string.IsNullOrWhiteSpace(text)) continue;

                if (IsHoTenHeader(text))
                {
                    map.HoTen = c;
                    matchCount++;
                }
                else if (IsGioiTinhHeader(text))
                {
                    map.GioiTinh = c;
                    matchCount++;
                }
                else if (IsNgaySinhHeader(text))
                {
                    map.NgaySinh = c;
                    matchCount++;
                }
                else if (IsNgayMatHeader(text))
                {
                    map.NgayMat = c;
                    matchCount++;
                }
                else if (IsMaChaHeader(text))
                {
                    map.MaCha = c;
                    matchCount++;
                }
                else if (IsMaMeHeader(text))
                {
                    map.MaMe = c;
                    matchCount++;
                }
                else if (IsMaDongHeader(text))
                {
                    map.MaDong = c;
                    matchCount++;
                }
                else if (IsLoaiLichHeader(text))
                {
                    map.LoaiLich = c;
                    matchCount++;
                }
                else if (IsLoaiConHeader(text))
                {
                    map.LoaiCon = c;
                    matchCount++;
                }
                else if (IsThuTuVoHeader(text))
                {
                    map.ThuTuVo = c;
                    matchCount++;
                }
            }

            // Nếu tìm thấy cột Họ tên và ít nhất 1 cột quan trọng khác -> Đây là dòng tiêu đề
            if (map.HoTen != -1 && matchCount >= 2)
            {
                return (r, map);
            }
        }

        return (-1, new ColumnIndices());
    }

    private static string NormalizeHeader(string header)
    {
        if (string.IsNullOrWhiteSpace(header)) return string.Empty;
        var text = header.Trim().ToLowerInvariant();
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != UnicodeCategory.NonSpacingMark && char.IsLetterOrDigit(c))
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    private static bool IsHoTenHeader(string h) =>
        h is "hoten" or "hovaten" or "ten" or "fullname" or "hovatenguoi" or "tennguoi" ||
        h.Contains("hoten") || h.Contains("hovaten");

    private static bool IsGioiTinhHeader(string h) =>
        h is "gioitinh" or "gender" or "sex" or "namnu" || h.Contains("gioitinh");

    private static bool IsNgaySinhHeader(string h) =>
        h is "ngaysinh" or "namsinh" or "sinh" or "namsinhngaysinh" or "birth" or "dob" or "ngaythangnamsinh" ||
        h.Contains("namsinh") || h.Contains("ngaysinh");

    private static bool IsNgayMatHeader(string h) =>
        h is "ngaymat" or "nammat" or "mat" or "death" or "ngaythangnammat" ||
        h.Contains("nammat") || h.Contains("ngaymat");

    private static bool IsMaChaHeader(string h) =>
        h is "macha" or "idcha" or "idchame" or "cha" or "bo" or "father" or "fatherid" or "machame" ||
        h.StartsWith("macha") || h.StartsWith("idcha") || h.Contains("chame");

    private static bool IsMaMeHeader(string h) =>
        h is "mame" or "idme" or "me" or "mother" or "motherid" ||
        h.StartsWith("mame") || h.StartsWith("idme");

    private static bool IsMaDongHeader(string h) =>
        h is "madong" or "id" or "stt" or "dong" or "maphahe" or "maso" or "code";

    private static bool IsLoaiLichHeader(string h) =>
        h is "loailich" or "lich" or "calendar";

    private static bool IsLoaiConHeader(string h) =>
        h is "loaicon" or "conruotconnuoi" or "relation";

    private static bool IsThuTuVoHeader(string h) =>
        h is "thutuvo" or "vochong" or "vothu" or "quanhevochong" or "vo" or "chong" or "spouse" ||
        h.Contains("vochong") || h.Contains("thutuvo");

    private static string NormalizeGender(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return "Nam";
        var norm = NormalizeHeader(val);
        if (norm.StartsWith("nu") || norm.StartsWith("female") || norm.Contains("gai") || norm.Contains("ba") || norm == "2" || norm == "0")
        {
            return "Nữ";
        }
        return "Nam";
    }

    private static string NormalizeCalendar(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return "Dương lịch";
        var norm = NormalizeHeader(val);
        if (norm.Contains("am") || norm.Contains("lunar") || norm == "al")
        {
            return "Âm lịch";
        }
        return "Dương lịch";
    }

    private static string? CleanReferenceCode(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;
        var trimmed = val.Trim();
        if (trimmed is "-" or "0" or "none" or "null" or "ko" or "khong") return null;
        return trimmed;
    }

    private static string GetCellText(IXLCell cell)
    {
        if (cell.IsEmpty()) return string.Empty;

        // Nếu ô chứa DateTime
        if (cell.DataType == XLDataType.DateTime)
        {
            try
            {
                var dt = cell.GetDateTime();
                return dt.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            }
            catch
            {
                // Fallback nếu parse date lỗi
            }
        }

        // Nếu ô chứa số nguyên (ví dụ năm 1920)
        if (cell.DataType == XLDataType.Number)
        {
            var val = cell.GetDouble();
            if (val == Math.Floor(val))
            {
                return ((long)val).ToString();
            }
        }

        return cell.GetString().Trim();
    }

    public byte[] GenerateTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("GiaPhaTemplate");

        // Header
        for (var i = 0; i < ExpectedHeaders.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = ExpectedHeaders[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#EFEFEF");
        }

        // Sample Rows
        worksheet.Cell(2, 1).Value = "P01";
        worksheet.Cell(2, 2).Value = "Nguyễn Văn Khởi";
        worksheet.Cell(2, 3).Value = "Nam";
        worksheet.Cell(2, 4).Value = "1920";
        worksheet.Cell(2, 5).Value = "Dương lịch";
        worksheet.Cell(2, 6).Value = "1995";
        worksheet.Cell(2, 7).Value = "";
        worksheet.Cell(2, 8).Value = "";
        worksheet.Cell(2, 9).Value = "";
        worksheet.Cell(2, 10).Value = "";

        worksheet.Cell(3, 1).Value = "P02";
        worksheet.Cell(3, 2).Value = "Trần Thị Mai";
        worksheet.Cell(3, 3).Value = "Nữ";
        worksheet.Cell(3, 4).Value = "1925";
        worksheet.Cell(3, 5).Value = "Dương lịch";
        worksheet.Cell(3, 6).Value = "2000";
        worksheet.Cell(3, 7).Value = "";
        worksheet.Cell(3, 8).Value = "";
        worksheet.Cell(3, 9).Value = "";
        worksheet.Cell(3, 10).Value = "1";

        worksheet.Cell(4, 1).Value = "P03";
        worksheet.Cell(4, 2).Value = "Nguyễn Văn Đạt";
        worksheet.Cell(4, 3).Value = "Nam";
        worksheet.Cell(4, 4).Value = "15/08/1950";
        worksheet.Cell(4, 5).Value = "Dương lịch";
        worksheet.Cell(4, 6).Value = "";
        worksheet.Cell(4, 7).Value = "P01";
        worksheet.Cell(4, 8).Value = "P02";
        worksheet.Cell(4, 9).Value = "Con ruột";
        worksheet.Cell(4, 10).Value = "";

        worksheet.Columns().AdjustToContents();

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        return memoryStream.ToArray();
    }
}
