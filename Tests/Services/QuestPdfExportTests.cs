using System.Diagnostics;
using GiaPha.BusinessObjects.Entities;
using GiaPha.Services.Implementations;
using GiaPha.Services.Logging;
using Moq;
using Xunit;

namespace GiaPha.UnitTests.Services;

public sealed class QuestPdfExportTests
{
    [Fact]
    public void GenerateGenealogyBookPdf_WithComplexVietnameseDiacritics_ShouldSucceed()
    {
        var clan = new Clan
        {
            Id = Guid.NewGuid(),
            Name = "Họ Nguyễn Thị Ánh Tuyết",
            OriginStory = "Tổ tiên từ ngàn xưa lập ấp, con cháu sum vầy ễ ự ợ muôn đời hiển vinh.",
            Description = "Gia phả mẫu thử nghiệm ký tự tiếng Việt đặc biệt."
        };

        var father = new Person
        {
            Id = Guid.NewGuid(),
            ClanId = clan.Id,
            FullName = "Nguyễn Thị Ánh Tuyết ễ ự ợ",
            Gender = Gender.Female,
            Generation = 1,
            BirthDate = new FlexibleDate { Year = 1940 }
        };

        var child = new Person
        {
            Id = Guid.NewGuid(),
            ClanId = clan.Id,
            FullName = "Nguyễn Văn Đạt",
            Gender = Gender.Male,
            Generation = 2,
            BirthDate = new FlexibleDate { Year = 1970 }
        };

        var persons = new List<Person> { father, child };
        var childToFather = new Dictionary<Guid, Guid> { { child.Id, father.Id } };
        var fatherToChildren = new Dictionary<Guid, List<Guid>> { { father.Id, new List<Guid> { child.Id } } };
        var husbandToWives = new Dictionary<Guid, List<(Person Wife, int Rank)>>();

        var exportService = new ExportService(
            Mock.Of<Repositories.Interfaces.IRepositoryManager>(),
            Mock.Of<ILoggerManager>(),
            Mock.Of<Hangfire.IBackgroundJobClient>());

        var pdfBytes = exportService.GenerateGenealogyBookPdf(
            clan,
            persons,
            childToFather,
            fatherToChildren,
            husbandToWives);

        Assert.NotNull(pdfBytes);
        Assert.True(pdfBytes.Length > 1000); // File PDF hợp lệ
    }

    [Fact]
    public void GenerateLargeChartPdf_WithSyntheticTree_ShouldCompleteQuicklyUnder20MB()
    {
        var clan = new Clan
        {
            Id = Guid.NewGuid(),
            Name = "Họ Lê Bá",
            Description = "Kiểm thử hiệu năng phả đồ lớn"
        };

        var persons = new List<Person>();
        var childToFather = new Dictionary<Guid, Guid>();
        var fatherToChildren = new Dictionary<Guid, List<Guid>>();

        // Sinh cây 300 người mô phỏng
        var root = new Person
        {
            Id = Guid.NewGuid(),
            ClanId = clan.Id,
            FullName = "Lê Bá Tổ",
            Gender = Gender.Male,
            Generation = 1,
            BirthDate = new FlexibleDate { Year = 1850 }
        };
        persons.Add(root);

        var currentGen = new List<Person> { root };
        for (var gen = 2; gen <= 5; gen++)
        {
            var nextGen = new List<Person>();
            foreach (var p in currentGen)
            {
                var childrenCount = 3;
                var childIds = new List<Guid>();
                for (var c = 1; c <= childrenCount; c++)
                {
                    var child = new Person
                    {
                        Id = Guid.NewGuid(),
                        ClanId = clan.Id,
                        FullName = $"Lê Bá {gen}_{c}_{p.FullName[..Math.Min(6, p.FullName.Length)]}",
                        Gender = c % 2 == 0 ? Gender.Female : Gender.Male,
                        Generation = gen,
                        BirthDate = new FlexibleDate { Year = 1850 + gen * 25 }
                    };
                    persons.Add(child);
                    nextGen.Add(child);
                    childIds.Add(child.Id);
                    childToFather[child.Id] = p.Id;
                }
                fatherToChildren[p.Id] = childIds;
            }
            currentGen = nextGen;
        }

        var exportService = new ExportService(
            Mock.Of<Repositories.Interfaces.IRepositoryManager>(),
            Mock.Of<ILoggerManager>(),
            Mock.Of<Hangfire.IBackgroundJobClient>());

        var sw = Stopwatch.StartNew();
        var pdfBytes = exportService.GenerateLargeChartPdf(
            clan,
            persons,
            childToFather,
            fatherToChildren,
            pageSize: "A0");
        sw.Stop();

        Assert.NotNull(pdfBytes);
        Assert.True(pdfBytes.Length > 0);
        Assert.True(sw.ElapsedMilliseconds < 15000, $"Thời gian xuất {sw.ElapsedMilliseconds}ms vượt quá 15s");
        Assert.True(pdfBytes.Length < 20 * 1024 * 1024, $"Kích thước {pdfBytes.Length} vượt quá 20MB");
    }
}
