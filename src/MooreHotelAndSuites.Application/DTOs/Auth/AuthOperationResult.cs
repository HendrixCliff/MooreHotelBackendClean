
namespace MooreHotelAndSuites.Application.DTOs.Auth;

public sealed record AuthOperationResult(
    bool Succeeded,
    string Message,
    IReadOnlyList<string> Errors)
{
    public static AuthOperationResult Success(string message) =>
        new(true, message, Array.Empty<string>());

    public static AuthOperationResult Failure(
        string message,
        IEnumerable<string>? errors = null) =>
        new(false, message, errors?.ToArray() ?? Array.Empty<string>());
}
