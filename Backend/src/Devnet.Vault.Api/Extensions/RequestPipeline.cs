using Devnet.Vault.Api.Middlewares;
using Devnet.Vault.Domain.Constants.AppKeys;

namespace Devnet.Vault.Api.Extensions;

public static class RequestPipeline
{
    public static void AddMiddlewares(this WebApplication _app)
    {
        if (_app.Environment.IsDevelopment())
            _app.MapOpenApi();

        _app.UseCors(ConfigKeys.CORS_POLICY_NAME);
        _app.UseHttpsRedirection();
        _app.UseMiddleware<CorrelationIdMiddleware>();
        _app.UseRouting();
        _app.UseMiddleware<ExceptionHandler>();
        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapControllers();
    }
}
