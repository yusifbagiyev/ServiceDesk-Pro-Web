using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ServiceDesk.Kernel.Results;

namespace ServiceDesk.SharedInfrastructure.Web;

/// <summary>Maps the railway <see cref="Result"/> to HTTP responses (ProblemDetails on failure).</summary>
public static class ApiResults
{
    public static IActionResult ToActionResult(
        this Result result,
        ControllerBase controller,
        int successStatusCode = StatusCodes.Status204NoContent) =>
        result.IsSuccess ? controller.StatusCode(successStatusCode) : Problem(controller, result.Error);

    public static IActionResult ToActionResult<TValue>(this Result<TValue> result, ControllerBase controller) =>
        result.IsSuccess ? controller.Ok(result.Value) : Problem(controller, result.Error);

    private static IActionResult Problem(ControllerBase controller, Error error)
    {
        if (error is ValidationError validationError)
        {
            var modelState = new ModelStateDictionary();
            foreach (var fieldError in validationError.Errors)
            {
                modelState.AddModelError(fieldError.Code, fieldError.Message);
            }

            return controller.ValidationProblem(modelState);
        }

        return controller.Problem(detail: error.Message, statusCode: StatusFor(error.Type), title: error.Code);
    }

    private static int StatusFor(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        _ => StatusCodes.Status400BadRequest,
    };
}
