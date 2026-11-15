using GiaPha.BusinessObjects.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GiaPha.Services.Pdf;

public sealed class GenealogyBookDocument : IDocument
{
    private readonly Clan _clan;
    private readonly IReadOnlyList<Person> _persons;
    private readonly IReadOnlyDictionary<Guid, Guid> _childToFatherMap;
    private readonly IReadOnlyDictionary<Guid, List<Guid>> _fatherToChildrenMap;
    private readonly IReadOnlyDictionary<Guid, List<(Person Wife, int Rank)>> _husbandToWivesMap;
    private readonly Dictionary<Guid, Person> _personMap;

    public GenealogyBookDocument(
        Clan clan,
        IReadOnlyList<Person> persons,
        IReadOnlyDictionary<Guid, Guid> childToFatherMap,
        IReadOnlyDictionary<Guid, List<Guid>> fatherToChildrenMap,
        IReadOnlyDictionary<Guid, List<(Person Wife, int Rank)>> husbandToWivesMap)
    {
        _clan = clan;
        _persons = persons;
        _childToFatherMap = childToFatherMap;
        _fatherToChildrenMap = fatherToChildrenMap;
        _husbandToWivesMap = husbandToWivesMap;
        _personMap = persons.ToDictionary(p => p.Id);
    }

    public DocumentMetadata GetMetadata() => new()
    {
        Title = $"Sách Phả Ký - {_clan.Name}",
        Author = "Hệ thống Quản lý Gia phả (VGMS)",
        Subject = "Phả ký và phả hệ dòng họ",
        Keywords = "gia phả, phả ký, dòng họ, VGMS"
    };

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(2.5f, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontFamily("Be Vietnam Pro").FontSize(11).FontColor("#2d2a26"));

            // 1. TRANG BÌA
            page.Content().Column(col =>
            {
                col.Item().PaddingTop(120).AlignCenter().Text("GIA PHẢ").FontSize(24).Bold().FontColor("#7c2d12");
                col.Item().PaddingTop(15).AlignCenter().Text(_clan.Name.ToUpperInvariant()).FontSize(20).Bold().FontColor("#1c1917");
                col.Item().PaddingTop(10).AlignCenter().Text(_clan.Description ?? "Tập lưu truyền muôn đời con cháu").FontSize(13).Italic();

                col.Item().PaddingTop(160).AlignCenter().Text("BẢN GIA PHẢ CHÍNH THỨC").FontSize(11).Bold();
                col.Item().PaddingTop(5).AlignCenter().Text($"Xuất bản năm: {DateTime.UtcNow.Year}").FontSize(11);
                col.Item().AlignCenter().Text("Nguyễn Thị Ánh Tuyết ễ ự ợ").FontSize(9).FontColor("#78716c"); // Test kiểm tra font tiếng Việt

                col.Item().PageBreak();

                // 2. PHẢ KÝ & LỜI TỰA
                col.Item().PaddingTop(10).AlignCenter().Text("PHẢ KÝ DÒNG HỌ").FontSize(18).Bold().FontColor("#7c2d12");
                col.Item().PaddingTop(5).LineHorizontal(1).LineColor("#e7e5e4");

                col.Item().PaddingTop(20).Text(text =>
                {
                    var story = !string.IsNullOrWhiteSpace(_clan.OriginStory)
                        ? _clan.OriginStory
                        : "Dòng họ ta từ ngàn xưa khai cơ lập nghiệp, trải qua bao thăng trầm dâu bể, các bậc tiền nhân đã dày công vun đắp, để lại cho hậu bối muôn đời một cơ đồ rạng rỡ và truyền thống hiếu học, cần cù, nhân hậu.";

                    text.Span(story).LineHeight(1.6f);
                });

                if (!string.IsNullOrWhiteSpace(_clan.Rules))
                {
                    col.Item().PaddingTop(25).Text("GIA HUẤN & QUY ƯỚC DÒNG HỌ").FontSize(14).Bold().FontColor("#7c2d12");
                    col.Item().PaddingTop(10).Text(_clan.Rules).LineHeight(1.6f);
                }

                col.Item().PageBreak();

                // 3. NỘI DUNG TỪNG ĐỜI
                var generations = _persons
                    .GroupBy(p => p.Generation <= 0 ? 1 : p.Generation)
                    .OrderBy(g => g.Key);

                foreach (var gen in generations)
                {
                    col.Item().PaddingTop(20).Text($"ĐỜI THỨ {RomanNumeral(gen.Key)}").FontSize(16).Bold().FontColor("#7c2d12");
                    col.Item().PaddingTop(5).LineHorizontal(1).LineColor("#e7e5e4");

                    col.Item().PaddingTop(12).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3); // Họ tên
                            columns.RelativeColumn(2); // Húy/Tự
                            columns.RelativeColumn(3); // Sinh-mất
                            columns.RelativeColumn(3); // Vợ/Chồng
                            columns.RelativeColumn(3); // Con
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background("#f5f5f4").Padding(5).Text("Họ tên").Bold().FontSize(10);
                            header.Cell().Background("#f5f5f4").Padding(5).Text("Tên húy/tự").Bold().FontSize(10);
                            header.Cell().Background("#f5f5f4").Padding(5).Text("Sinh – Mất").Bold().FontSize(10);
                            header.Cell().Background("#f5f5f4").Padding(5).Text("Vợ / Chồng").Bold().FontSize(10);
                            header.Cell().Background("#f5f5f4").Padding(5).Text("Hậu duệ (Con)").Bold().FontSize(10);
                        });

                        foreach (var p in gen.OrderBy(x => x.FullName))
                        {
                            // 1. Họ tên
                            table.Cell().BorderBottom(0.5f).BorderColor("#e7e5e4").Padding(5).Text(p.FullName).Bold().FontSize(10);

                            // 2. Húy/Tự
                            var huyTu = p.HuyName ?? p.TuName ?? "—";
                            table.Cell().BorderBottom(0.5f).BorderColor("#e7e5e4").Padding(5).Text(huyTu).FontSize(9);

                            // 3. Sinh - Mất
                            var birthStr = FormatDate(p.BirthDate);
                            var deathStr = FormatDate(p.DeathDate);
                            var dates = $"{birthStr} – {deathStr}";
                            table.Cell().BorderBottom(0.5f).BorderColor("#e7e5e4").Padding(5).Text(dates).FontSize(9);

                            // 4. Vợ
                            var wivesStr = "—";
                            if (_husbandToWivesMap.TryGetValue(p.Id, out var wives) && wives.Count > 0)
                            {
                                wivesStr = string.Join("\n", wives.Select(w => $"{w.Wife.FullName} (Bậc {w.Rank})"));
                            }
                            table.Cell().BorderBottom(0.5f).BorderColor("#e7e5e4").Padding(5).Text(wivesStr).FontSize(9);

                            // 5. Con
                            var childrenStr = "—";
                            if (_fatherToChildrenMap.TryGetValue(p.Id, out var childIds) && childIds.Count > 0)
                            {
                                var childNames = childIds
                                    .Where(_personMap.ContainsKey)
                                    .Select(id => _personMap[id].FullName);
                                childrenStr = string.Join(", ", childNames);
                            }
                            table.Cell().BorderBottom(0.5f).BorderColor("#e7e5e4").Padding(5).Text(childrenStr).FontSize(9);
                        }
                    });

                    col.Item().PaddingBottom(20);
                }
            });

            // Footer
            page.Footer().AlignCenter().Text(x =>
            {
                x.CurrentPageNumber();
                x.Span(" / ");
                x.TotalPages();
            });
        });
    }

    private static string FormatDate(FlexibleDate? d)
    {
        if (d is null) return "—";
        var calSuffix = d.Calendar == CalendarType.Lunar ? " (ÂL)" : "";
        if (d.Day.HasValue && d.Month.HasValue && d.Year.HasValue)
        {
            return $"{d.Day:D2}/{d.Month:D2}/{d.Year}{calSuffix}";
        }
        if (d.Year.HasValue)
        {
            return $"{d.Year}{calSuffix}";
        }
        return "—";
    }

    private static string RomanNumeral(int number) => number switch
    {
        1 => "I", 2 => "II", 3 => "III", 4 => "IV", 5 => "V",
        6 => "VI", 7 => "VII", 8 => "VIII", 9 => "IX", 10 => "X",
        11 => "XI", 12 => "XII", 13 => "XIII", 14 => "XIV", 15 => "XV",
        16 => "XVI", 17 => "XVII", 18 => "XVIII", 19 => "XIX", 20 => "XX",
        _ => number.ToString()
    };
}
