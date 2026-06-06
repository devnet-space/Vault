#region imports
using Amazon.S3;
using Devnet.Vault.Application.Configurations;
using Devnet.Vault.Application.Features.Account.Interfaces.Repositories;
using Devnet.Vault.Application.Features.Auth.Interfaces.Repositories;
using Devnet.Vault.Application.Features.Auth.Interfaces.Services;
using Devnet.Vault.Application.Features.Groups.Interfaces.Repositories;
using Devnet.Vault.Application.Features.Shared.Cache.Interfaces.Services;
using Devnet.Vault.Application.Features.Shared.FileUpload.Interfaces;
using Devnet.Vault.Application.Features.Shared.Logging.Interfaces;
using Devnet.Vault.Application.Features.Shared.Otp.Interfaces.Services;
using Devnet.Vault.Application.Features.VaultItems.Interfaces;
using Devnet.Vault.Application.Notifications.Email.Interfaces;
using Devnet.Vault.Application.Security.Encryption.Interfaces;
using Devnet.Vault.Domain.Constants.AppKeys;
using Devnet.Vault.Domain.Constants.Messages;
using Devnet.Vault.Infrastructure.Cache.Services;
using Devnet.Vault.Infrastructure.Logging.Services;
using Devnet.Vault.Infrastructure.Notifications.Email.Queue;
using Devnet.Vault.Infrastructure.Notifications.Email.Services;
using Devnet.Vault.Infrastructure.Notifications.Email.Workers;
using Devnet.Vault.Infrastructure.Otp.Services;
using Devnet.Vault.Infrastructure.Persistence.Context;
using Devnet.Vault.Infrastructure.Persistence.Repositories.Implementations.Account;
using Devnet.Vault.Infrastructure.Persistence.Repositories.Implementations.Authentication;
using Devnet.Vault.Infrastructure.Persistence.Repositories.Implementations.Groups;
using Devnet.Vault.Infrastructure.Persistence.Repositories.Implementations.VaultItems;
using Devnet.Vault.Infrastructure.Security;
using Devnet.Vault.Infrastructure.Storage.CloudFareR2.Queue;
using Devnet.Vault.Infrastructure.Storage.CloudFareR2.Services;
using Devnet.Vault.Infrastructure.Storage.CloudFareR2.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
#endregion

namespace Devnet.Vault.Infrastructure;

/// <summary>
/// Added dependecy related to infrastructure layer
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection _services, IConfiguration _config)
    {
        var mySqlConnectionString = _config[ConfigKeys.MYSQL_CONNECTION_STRINGS_KEY];
        var redisConnectionString = _config[ConfigKeys.REDIS_CONNECTION_STRINGS_KEY]
            ?? throw new KeyNotFoundException(ExceptionMessages.REDIS_CONNECTION_KEY_NOT_FOUND);

        // register mysql connecton provider with entity and dapper support
        _services.AddScoped<DbConnectionFactory>();

        _services.AddDbContext<AppDbContext>(options =>
        {
            options.UseMySql(
                mySqlConnectionString,
                ServerVersion.AutoDetect(mySqlConnectionString)
            );
        });

        _services.RegisterRedis(redisConnectionString);
        _services.RegisterStorageSupport();

        _services.RegisterRepositories();
        _services.RegisterServices();
        _services.RegisterBackgroundServiceAndQueues();


        return _services;
    }

    /// <summary>
    /// Register all background worker , queues and services relaed to queues
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    private static IServiceCollection RegisterBackgroundServiceAndQueues(this IServiceCollection services)
    {
        services.AddSingleton<IEmailQueue, InMemoryEmailQueue>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        services.AddHostedService<EmailWorker>();
        services.AddHostedService<FileUploadWorker>();
        services.AddSingleton(typeof(IAppLogger<>), typeof(SerilogAppLogger<>));

        services.AddSingleton<IFileUploadQueue, InMemoryFileUploadQueue>();
        services.AddScoped<IR2FileUploadService, R2FileUploadService>();

        return services;
    }

    /// <summary>
    /// Register infrastructure services 
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    private static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<IOtpGenerator, OtpGenerator>();
        services.AddScoped<IOtpValidationService, OtpValidationService>();
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<IJwtService, JwtService>();

        return services;
    }

    /// <summary>
    /// Register repositories implemenatation whose constract defined in application layer
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    private static IServiceCollection RegisterRepositories(this IServiceCollection services)
    {
        // Register Auth and Account Repositories
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IVaultItemsRepository, VaultItemsRepository>();

        return services;
    }

    /// <summary>
    /// Added redis connection support suing connection multiplexer
    /// </summary>
    /// <param name="services"></param>
    /// <param name="redisConnectionString"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static IServiceCollection RegisterRedis(this IServiceCollection services, string redisConnectionString)
    {

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            if (string.IsNullOrWhiteSpace(redisConnectionString))
                throw new InvalidOperationException(ExceptionMessages.REDIS_CONNECTION_KEY_NOT_FOUND);

            var options = ConfigurationOptions.Parse(redisConnectionString);

            options.AbortOnConnectFail = false;
            options.ConnectRetry = 3;
            options.ReconnectRetryPolicy = new ExponentialRetry(5000);

            return ConnectionMultiplexer.Connect(options);
        });
        return services;
    }

    /// <summary>
    /// Register cloudfarer2 support for storing files and folders
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    private static IServiceCollection RegisterStorageSupport(this IServiceCollection services)
    {
        services.AddSingleton<AmazonS3Client>(provider =>
        {
            var settings = provider
                .GetRequiredService<IOptions<CloudFareR2Settings>>()
                .Value;

            var config = new AmazonS3Config
            {
                ServiceURL = settings.ServiceUrl,

                ForcePathStyle = true
            };

            return new AmazonS3Client(
                settings.AccessKeyId,
                settings.SecretAccessKey,
                config);
        });

        return services;
    }
}