namespace GiaPha.BusinessObjects.Exceptions;

/// <summary>
/// Dòng họ đã Published nên không được ghi trực tiếp; thay đổi phải đi qua ChangeRequest.
/// </summary>
public sealed class RequiresChangeRequestException : Exception
{
    public RequiresChangeRequestException(string message) : base(message)
    {
    }

    public string Code => ApiErrorCodes.RequiresChangeRequest;
}
