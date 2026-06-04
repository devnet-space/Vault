using Devnet.Vault.Domain.Constants.AppSettings;

namespace Devnet.Vault.Api.Middlewares;

/// <summary>
/// Extract request specific corrrelation id and add it to request context and serilog context
/// </summary>
/// <param name="next"></param>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        var correlationId =
            context.Request.Headers[AppConstants.CORRELATION_ID_HEADER].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.Items[AppConstants.CORRELATION_ID_NAME] = correlationId;

        context.Response.Headers[AppConstants.CORRELATION_ID_HEADER] = correlationId;

        using (Serilog.Context.LogContext.PushProperty(AppConstants.CORRELATION_ID_NAME, correlationId))
        {
            await next(context);
        }
    }
}
