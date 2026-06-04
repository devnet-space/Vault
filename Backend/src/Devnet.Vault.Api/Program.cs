
using Devnet.Vault.Api.Extensions;
using Devnet.Vault.Application;
using Devnet.Vault.Infrastructure;

namespace Devnet.Vault.Api;

public sealed class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddPresentation(builder.Configuration);
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);

        var app = builder.Build();

        app.AddMiddlewares();

        app.Run();
    }
}
