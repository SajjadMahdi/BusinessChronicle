using BusinessChronicle.Abstractions.Enums;
using BusinessChronicle.Abstractions.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BusinessChronicle.AspNetCore.ProblemDetails;

/// <summary>
/// Maps chronicle failures to RFC 7807 problem details.
/// </summary>
public static class ChronicleProblemDetailsMapper
{
    /// <summary>
    /// Writes a problem details response for a chronicle failure.
    /// </summary>
    public static IResult FromFailure<T>(ChronicleResult<T> result)
    {
        ChronicleError error = result.Error!.Value;
        int statusCode = error.Code switch
        {
            ChronicleErrorCode.NotFound => StatusCodes.Status404NotFound,
            ChronicleErrorCode.ValidationFailed => StatusCodes.Status400BadRequest,
            ChronicleErrorCode.ConcurrencyConflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError,
        };

        return Results.Problem(
            title: error.Code.ToString(),
            detail: error.Message,
            statusCode: statusCode,
            extensions: error.Detail is null ? null : new Dictionary<string, object?> { ["detail"] = error.Detail });
    }

    /// <summary>
    /// Writes a validation problem response.
    /// </summary>
    public static IResult Validation(string message) =>
        Results.Problem(title: nameof(ChronicleErrorCode.ValidationFailed), detail: message, statusCode: StatusCodes.Status400BadRequest);
}
