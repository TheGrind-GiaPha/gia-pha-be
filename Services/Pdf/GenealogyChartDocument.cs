using System.Security;
using System.Text;
using GiaPha.BusinessObjects.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GiaPha.Services.Pdf;

public sealed class GenealogyChartDocument : IDocument
{
    private readonly Clan _clan;
    private readonly List<List<TreeNode>> _pagesNodes;
    private readonly List<string> _pageTitles;
    private readonly PageSize _pageSize;

    public GenealogyChartDocument(
        Clan clan,
        List<List<TreeNode>> pagesNodes,
        List<string> pageTitles,
        string pageSizeName = "A0")
    {
        _clan = clan;
        _pagesNodes = pagesNodes;
        _pageTitles = pageTitles;
        _pageSize = pageSizeName.ToUpperInvariant() switch
        {
            "A1" => PageSizes.A1.Landscape(),
            _ => PageSizes.A0.Landscape()
        };
    }

    public DocumentMetadata GetMetadata() => new()
    {
        Title = $"Phả Đồ Khổ Lớn - {_clan.Name}",
        Author = "Hệ thống Quản lý Gia phả (VGMS)",
        Subject = "Phả đồ cây gia phả vector khổ lớn",
        Keywords = "phả đồ, vector, A0, A1, VGMS"
    };

    public void Compose(IDocumentContainer container)
    {
        for (var pageIdx = 0; pageIdx < _pagesNodes.Count; pageIdx++)
        {
            var nodes = _pagesNodes[pageIdx];
            var title = pageIdx < _pageTitles.Count ? _pageTitles[pageIdx] : $"Tờ số {pageIdx + 1}";
            var svgContent = GenerateTreeSvg(nodes);

            container.Page(page =>
            {
                page.Size(_pageSize);
                page.Margin(40, Unit.Point);
                page.DefaultTextStyle(x => x.FontFamily("Be Vietnam Pro").FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text($"PHẢ ĐỒ DÒNG HỌ {_clan.Name.ToUpperInvariant()}").FontSize(26).Bold().FontColor("#7c2d12");
                    col.Item().AlignCenter().Text(title).FontSize(16).Italic().FontColor("#44403c");
                    col.Item().PaddingTop(10).LineHorizontal(1.5f).LineColor("#7c2d12");
                });

                page.Content().PaddingTop(20).Svg(svgContent);

                page.Footer().AlignCenter().Text($"Hệ thống Quản lý Gia phả VGMS — Bản đồ chuẩn in ấn khổ {_pageSize.Width:F0}x{_pageSize.Height:F0}pt — Xuất bản năm {DateTime.UtcNow.Year}");
            });
        }
    }

    private static string GenerateTreeSvg(List<TreeNode> nodes)
    {
        if (nodes.Count == 0) return "<svg xmlns='http://www.w3.org/2000/svg'></svg>";

        var minX = nodes.Min(n => n.X);
        var minY = nodes.Min(n => n.Y);
        var maxX = nodes.Max(n => n.X + n.Width);
        var maxY = nodes.Max(n => n.Y + n.Height);

        var svgWidth = Math.Max(maxX + 60f, 1000f);
        var svgHeight = Math.Max(maxY + 60f, 600f);

        var sb = new StringBuilder();
        sb.AppendLine($"<svg width='{svgWidth:F0}' height='{svgHeight:F0}' viewBox='0 0 {svgWidth:F0} {svgHeight:F0}' xmlns='http://www.w3.org/2000/svg'>");

        // 1. Vẽ các đường kẻ vuông góc (Orthogonal vector lines) nối cha - con
        foreach (var node in nodes)
        {
            if (node.Children.Count == 0) continue;

            var fatherBottomX = node.X + node.Width / 2f;
            var fatherBottomY = node.Y + node.Height;
            var midY = fatherBottomY + 22f;

            // Kẻ dọc xuống từ đáy cha
            sb.AppendLine($"<line x1='{fatherBottomX:F1}' y1='{fatherBottomY:F1}' x2='{fatherBottomX:F1}' y2='{midY:F1}' stroke='#64748b' stroke-width='1.8' />");

            var firstChildMidX = node.Children.First().X + node.Children.First().Width / 2f;
            var lastChildMidX = node.Children.Last().X + node.Children.Last().Width / 2f;

            // Kẻ thanh ngang nối các con
            sb.AppendLine($"<line x1='{firstChildMidX:F1}' y1='{midY:F1}' x2='{lastChildMidX:F1}' y2='{midY:F1}' stroke='#64748b' stroke-width='1.8' />");

            // Kẻ nhánh dọc xuống từng con
            foreach (var child in node.Children)
            {
                var childTopX = child.X + child.Width / 2f;
                sb.AppendLine($"<line x1='{childTopX:F1}' y1='{midY:F1}' x2='{childTopX:F1}' y2='{child.Y:F1}' stroke='#64748b' stroke-width='1.8' />");
            }
        }

        // 2. Vẽ từng node cá nhân
        foreach (var node in nodes)
        {
            var borderColor = node.Person.Gender == Gender.Female ? "#be185d" : "#b45309";
            var strokeWidth = 1.5f;

            // Hộp chữ nhật bo góc
            sb.AppendLine($"<rect x='{node.X:F1}' y='{node.Y:F1}' width='{node.Width:F1}' height='{node.Height:F1}' rx='6' ry='6' fill='#fdfcf8' stroke='{borderColor}' stroke-width='{strokeWidth}' />");

            // Text thông tin
            var safeName = SecurityElement.Escape(TruncateText(node.Person.FullName, 22));
            var genText = SecurityElement.Escape($"Đời thứ {node.Person.Generation}");
            var birth = node.Person.BirthDate?.Year?.ToString() ?? "?";
            var death = node.Person.DeathDate?.Year?.ToString() ?? (node.Person.IsLiving ? "nay" : "?");
            var dateText = SecurityElement.Escape($"{birth} – {death}");

            sb.AppendLine($"<text x='{(node.X + 10):F1}' y='{(node.Y + 20):F1}' font-family='Be Vietnam Pro, sans-serif' font-size='10.5' font-weight='bold' fill='#1c1917'>{safeName}</text>");
            sb.AppendLine($"<text x='{(node.X + 10):F1}' y='{(node.Y + 36):F1}' font-family='Be Vietnam Pro, sans-serif' font-size='8.5' fill='#57534e'>{genText}</text>");
            sb.AppendLine($"<text x='{(node.X + 10):F1}' y='{(node.Y + 52):F1}' font-family='Be Vietnam Pro, sans-serif' font-size='8.5' fill='#57534e'>{dateText}</text>");
        }

        sb.AppendLine("</svg>");
        return sb.ToString();
    }

    private static string TruncateText(string text, int maxChars)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        return text.Length <= maxChars ? text : text[..(maxChars - 2)] + "..";
    }
}
