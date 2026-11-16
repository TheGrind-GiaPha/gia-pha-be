using System.Text.Json;
using GiaPha.BusinessObjects.DTOs.Imports;
using GiaPha.BusinessObjects.Entities;
using GiaPha.Repositories.Interfaces;
using GiaPha.Services.Implementations;
using GiaPha.Services.Interfaces;
using GiaPha.Services.Logging;
using Moq;
using Xunit;

namespace GiaPha.UnitTests.Services;

public sealed class ImportServiceTests
{
    private readonly Mock<IRepositoryManager> _repositoryMock = new();
    private readonly Mock<IExcelImportService> _excelMock = new();
    private readonly Mock<IWordImportService> _wordMock = new();
    private readonly Mock<ILlmExtractionService> _llmMock = new();
    private readonly Mock<IDuplicateDetectionService> _dupMock = new();
    private readonly Mock<IPersonService> _personServiceMock = new();
    private readonly Mock<ILoggerManager> _loggerMock = new();

    private readonly ImportService _importService;

    public ImportServiceTests()
    {
        _importService = new ImportService(
            _repositoryMock.Object,
            _excelMock.Object,
            _wordMock.Object,
            _llmMock.Object,
            _dupMock.Object,
            _personServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CommitBatch_WhenChildSelectedButFatherNotSelected_ShouldThrowArgumentException()
    {
        var clanId = Guid.NewGuid();
        var batchId = Guid.NewGuid();

        var clan = new Clan { Id = clanId, Status = ClanStatus.Draft };
        _repositoryMock.Setup(r => r.Clan.GetByIdAsync(clanId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clan);

        var fatherRowId = Guid.NewGuid();
        var childRowId = Guid.NewGuid();

        var fatherRow = new ImportRow
        {
            Id = fatherRowId,
            BatchId = batchId,
            RowIndex = 1,
            ExtractedJson = JsonSerializer.Serialize(new ExcelRowData { MaDong = "P01", HoTen = "Nguyễn Văn Cha" })
        };

        var childRow = new ImportRow
        {
            Id = childRowId,
            BatchId = batchId,
            RowIndex = 2,
            ExtractedJson = JsonSerializer.Serialize(new ExcelRowData { MaDong = "P02", HoTen = "Nguyễn Văn Con", MaCha = "P01" })
        };

        var batch = new ImportBatch
        {
            Id = batchId,
            ClanId = clanId,
            Source = ImportSource.Excel,
            Rows = new List<ImportRow> { fatherRow, childRow }
        };

        _repositoryMock.Setup(r => r.ImportBatch.GetWithRowsAsync(batchId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(batch);

        // Chỉ chọn con, KHÔNG chọn cha
        var acceptedRowIds = new List<Guid> { childRowId };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _importService.CommitBatchAsync(clanId, batchId, acceptedRowIds, Guid.NewGuid()));

        Assert.Contains("chưa được chọn để nhập", ex.Message);
        Assert.Contains("Nguyễn Văn Con", ex.Message);
    }

    [Fact]
    public async Task CommitBatch_WhenClanPublished_ShouldCreateChangeRequestAndReturnPending()
    {
        var clanId = Guid.NewGuid();
        var batchId = Guid.NewGuid();

        var clan = new Clan { Id = clanId, Status = ClanStatus.Published };
        _repositoryMock.Setup(r => r.Clan.GetByIdAsync(clanId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clan);

        var rowId = Guid.NewGuid();
        var row = new ImportRow
        {
            Id = rowId,
            BatchId = batchId,
            RowIndex = 1,
            ExtractedJson = JsonSerializer.Serialize(new ExcelRowData { MaDong = "P01", HoTen = "Nguyễn Văn Tổ" })
        };

        var batch = new ImportBatch
        {
            Id = batchId,
            ClanId = clanId,
            Source = ImportSource.Excel,
            Rows = new List<ImportRow> { row }
        };

        _repositoryMock.Setup(r => r.ImportBatch.GetWithRowsAsync(batchId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(batch);

        _repositoryMock.Setup(r => r.ChangeRequest.Create(It.IsAny<ChangeRequest>()));
        _repositoryMock.Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _importService.CommitBatchAsync(clanId, batchId, new List<Guid> { rowId }, Guid.NewGuid());

        Assert.True(result.IsPendingApproval);
        Assert.NotNull(result.ChangeRequestId);
        Assert.Contains("chờ phê duyệt", result.Message);
        _repositoryMock.Verify(r => r.ChangeRequest.Create(It.Is<ChangeRequest>(cr => cr.Type == "ImportBatch")), Times.Once);
    }
}
