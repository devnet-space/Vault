using Devnet.Vault.Application.Behaviour;
using Devnet.Vault.Application.Features.Auth.Commands;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
namespace Devnet.Vault.Application;

/// <summary>
/// Added all dependecy of application layer
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // registered all request validator while taking one command for getting assemply location
        services.AddValidatorsFromAssembly(typeof(RegisterOrLoginCommand).Assembly);

        // registered validator in pipeline behaviour
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>)
        );

        // added support mediatR for quick CQRS implementations.
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(RegisterOrLoginCommand).Assembly);
        });

        return services;
    }
}