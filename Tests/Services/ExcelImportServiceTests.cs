using ClosedXML.Excel;
using GiaPha.Services.Implementations;
using Xunit;

namespace GiaPha.UnitTests.Services;

public sealed class ExcelImportServiceTests
{
    private readonly ExcelImportService _service = new();

    [Fact]
    public void GenerateTemplate_ShouldReturnValidExcelBytesWithHeaders()
    {
        var bytes = _service.GenerateTemplate();
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);

        using var stream = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(stream);
        var ws = workbook.Worksheets.First();

        Assert.Equal("MaDong", ws.Cell(1, 1).GetString());
        Assert.Equal("HoTen", ws.Cell(1, 2).GetString());
        Assert.Equal("GioiTinh", ws.Cell(1, 3).GetString());
    }

    [Fact]
    public void ParseExcel_WithValidTemplateBytes_ShouldReturnRowsWithoutErrors()
    {
        var bytes = _service.GenerateTemplate();
        using var stream = new MemoryStream(bytes);

        var (rows, errors) = _service.ParseExcel(stream);

        Assert.Empty(errors);
        Assert.True(rows.Count >= 3);
        Assert.Contains(rows, r => r.HoTen == "Nguyễn Văn Khởi");
    }

    [Fact]
    public void ParseExcel_WithUserSampleV2_ShouldParseSuccessfully()
    {
        var path = @"D:\Download\Gia_Pha_Ho_Nguyen_Huu_Mau-v2.xlsx";
        if (!File.Exists(path)) return;

        using var stream = File.OpenRead(path);
        var (rows, errors) = _service.ParseExcel(stream);

        Assert.Empty(errors);
        Assert.True(rows.Count >= 13);
        Assert.Contains(rows, r => r.HoTen == "Nguyễn Hữu Phúc" && r.GioiTinh == "Nam");
        Assert.Contains(rows, r => r.HoTen == "Nguyễn Hữu An" && r.MaCha == "1");
    }

    [Fact]
    public void ParseExcel_WithUserSampleV1_ShouldParseSuccessfully()
    {
        var path = @"D:\Download\Gia_Pha_Ho_Nguyen_Huu_Mau.xlsx";
        if (!File.Exists(path)) return;

        using var stream = File.OpenRead(path);
        var (rows, errors) = _service.ParseExcel(stream);

        Assert.Empty(errors);
        Assert.True(rows.Count >= 13);
        Assert.Contains(rows, r => r.HoTen == "Nguyễn Hữu Phúc");
        Assert.Contains(rows, r => r.HoTen == "Nguyễn Hữu An" && r.MaCha == "1");
    }
}
