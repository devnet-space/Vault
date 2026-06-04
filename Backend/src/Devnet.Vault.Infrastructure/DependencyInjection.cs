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

namespace Devnet.Vault.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection _services, IConfiguration _config)
    {
        var mySqlConnectionString = _config[ConfigKeys.MYSQL_CONNECTION_STRINGS_KEY];
        var redisConnectionString = _config[ConfigKeys.REDIS_CONNECTION_STRINGS_KEY];

        _services.AddScoped<DbConnectionFactory>();

        _services.AddDbContext<AppDbContext>(options =>
        {
            options.UseMySql(
                mySqlConnectionString,
                ServerVersion.AutoDetect(mySqlConnectionString)
            );
        });
        _services.AddSingleton<IEmailQueue, InMemoryEmailQueue>();
        _services.AddScoped<IEmailSender, SmtpEmailSender>();

        _services.AddHostedService<EmailWorker>();
        _services.AddHostedService<FileUploadWorker>();
        _services.AddSingleton(typeof(IAppLogger<>), typeof(SerilogAppLogger<>));

        _services.AddScoped<IEncryptionService, EncryptionService>();
        _services.AddScoped<IJwtService, JwtService>();

        _services.AddSingleton<AmazonS3Client>(provider =>
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
        _services.AddSingleton<IFileUploadQueue, InMemoryFileUploadQueue>();
        _services.AddScoped<IR2FileUploadService, R2FileUploadService>();

        _services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            if (string.IsNullOrWhiteSpace(redisConnectionString))
                throw new InvalidOperationException(ExceptionMessages.REDIS_CONNECTION_KEY_NOT_FOUND);

            var options = ConfigurationOptions.Parse(redisConnectionString);

            options.AbortOnConnectFail = false;
            options.ConnectRetry = 3;
            options.ReconnectRetryPolicy = new ExponentialRetry(5000);

            return ConnectionMultiplexer.Connect(options);
        });
        _services.AddScoped<ICacheService, RedisCacheService>();
        _services.AddScoped<IOtpGenerator, OtpGenerator>();
        _services.AddScoped<IOtpValidationService, OtpValidationService>();

        // Register Auth and Account Repositories
        _services.AddScoped<IAuthRepository, AuthRepository>();
        _services.AddScoped<IUserRepository, UserRepository>();
        _services.AddScoped<IGroupRepository, GroupRepository>();
        _services.AddScoped<IVaultItemsRepository, VaultItemsRepository>();

        return _services;
    }
}