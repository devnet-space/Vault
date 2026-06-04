using Devnet.Vault.Application.Configurations;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace Devnet.Vault.Infrastructure.Persistence.Context;

internal sealed class DbConnectionFactory(IOptions<ConnectionStringSettings> _options)
{
    private readonly string mySqlconnectionString = _options.Value.MySqlConnection;

    public MySqlConnection CreateMySqlConnection()
    {
        return new MySqlConnection(mySqlconnectionString);
    }
}
