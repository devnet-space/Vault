using Devnet.Vault.Application.Features.Shared.Otp.Commands;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Devnet.Vault.Api.Extensions;

/// <summary>
/// Add all dependency of presentation layer
/// </summary>
public static class DependencyInjection
{
    public static WebApplicationBuilder AddPresentation(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddControllers()
           .AddJsonOptions(options =>
           {
               // convert all enums to json and vice versa for user friendly request and response
               options.JsonSerializerOptions.Converters.Add(
                   new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
           });


        builder.Services.BindAppsettings(configuration);

        // Register MediatR for CQRS pattern
        builder.Services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblies(
                typeof(RequestOtpCommand).Assembly);
        });

        builder.Services.AddAuthPolicy(configuration);
        builder.Services.AddCORSPolicy(configuration);
        builder.ConfigureLogger();


        // Added support of open api for getting api documentaion and importing it for postman collection
        builder.Services.AddOpenApi();

        return builder;
    }
}
