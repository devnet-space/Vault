using Devnet.Vault.Domain.Constants.Messages;
using FluentValidation;
using System.Text.Json;

namespace Devnet.Vault.Api.Middlewares;

/// <summary>
/// All unhandled exception are logged and generic response message created and send for the request
/// </summary>
/// <param name="next"></param>
/// <param name="logger"></param>
public class ExceptionHandler(RequestDelegate next, ILogger<ExceptionHandler> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogInformation(ex, ExceptionMessages.GENERIC_VALIDATION_ERROR);

            await WriteResponse(context, StatusCodes.Status400BadRequest,
                ex.Message, ex.Message);
        }
        catch (ValidationException ex)
        {
            logger.LogWarning(ex, ExceptionMessages.GENERIC_VALIDATION_ERROR);

            await WriteResponse(context, StatusCodes.Status400BadRequest, ExceptionMessages.GENERIC_VALIDATION_ERROR,
                ex.Errors.Select(x => new
                {
                    field = x.PropertyName,
                    error = x.ErrorMessage
                }));
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex, ExceptionMessages.OPERATION_CANCELLED);

            await WriteResponse(context, StatusCodes.Status499ClientClosedRequest,
                ExceptionMessages.OPERATION_CANCELLED, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ExceptionMessages.GENERIC_ERROR);

            await WriteResponse(context, StatusCodes.Status500InternalServerError,
                ExceptionMessages.GENERIC_ERROR, ex.Message);
        }
    }

    private static async Task WriteResponse(HttpContext context, int statusCode, string message, object error)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.Clear();

        context.Response.StatusCode = statusCode;

        context.Response.ContentType = "application/json";

        var response = new
        {
            message,
            error
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }
}