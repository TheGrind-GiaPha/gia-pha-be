using GiaPha.BusinessObjects.Entities;

namespace GiaPha.Services.Pdf;

public sealed class TreeNode
{
    public Person Person { get; set; } = null!;
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; } = 140f;
    public float Height { get; set; } = 65f;
    public float Prelim { get; set; }
    public float Mod { get; set; }
    public TreeNode? Parent { get; set; }
    public List<TreeNode> Children { get; set; } = new();
}

public static class TidyTreeLayout
{
    private const float SiblingGap = 20f;
    private const float SubtreeGap = 40f;
    private const float LevelHeight = 110f;

    public static List<TreeNode> ComputeLayout(
        TreeNode root,
        float startX = 50f,
        float startY = 80f)
    {
        // Lượt 1: Tính toán sơ bộ prelim X
        FirstWalk(root);

        // Lượt 2: Gán tọa độ X, Y tuyệt đối và loại bỏ tọa độ âm
        SecondWalk(root, -root.Prelim + startX, startY);

        // Gom toàn bộ các node đã tính layout
        var allNodes = new List<TreeNode>();
        CollectNodes(root, allNodes);

        // Chuẩn hóa để node có X nhỏ nhất cách lề startX
        var minX = allNodes.Min(n => n.X);
        if (minX < startX)
        {
            var shift = startX - minX;
            foreach (var n in allNodes)
            {
                n.X += shift;
            }
        }

        return allNodes;
    }

    private static void FirstWalk(TreeNode node)
    {
        if (node.Children.Count == 0)
        {
            var prevSibling = GetPreviousSibling(node);
            node.Prelim = prevSibling is not null
                ? prevSibling.Prelim + prevSibling.Width + SiblingGap
                : 0f;
            return;
        }

        // Đệ quy cho các con
        foreach (var child in node.Children)
        {
            FirstWalk(child);
        }

        var midPoint = (node.Children.First().Prelim + node.Children.Last().Prelim) / 2f;
        var leftSibling = GetPreviousSibling(node);

        if (leftSibling is not null)
        {
            node.Prelim = leftSibling.Prelim + leftSibling.Width + SubtreeGap;
            node.Mod = node.Prelim - midPoint;
        }
        else
        {
            node.Prelim = midPoint;
        }
    }

    private static void SecondWalk(TreeNode node, float modSum, float currentY)
    {
        node.X = node.Prelim + modSum;
        node.Y = currentY;

        foreach (var child in node.Children)
        {
            SecondWalk(child, modSum + node.Mod, currentY + LevelHeight);
        }
    }

    private static TreeNode? GetPreviousSibling(TreeNode node)
    {
        if (node.Parent is null) return null;
        var siblings = node.Parent.Children;
        var idx = siblings.IndexOf(node);
        return idx > 0 ? siblings[idx - 1] : null;
    }

    private static void CollectNodes(TreeNode node, List<TreeNode> result)
    {
        result.Add(node);
        foreach (var c in node.Children)
        {
            CollectNodes(c, result);
        }
    }
}
