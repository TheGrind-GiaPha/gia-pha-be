namespace GiaPha.BusinessObjects.Exceptions;

/// <summary>Không đủ quyền thực hiện hành động (ví dụ tự duyệt đề xuất của mình).</summary>
public sealed class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}
