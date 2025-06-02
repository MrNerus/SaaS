using System;
using Dapper;
using Npgsql;
using SaaS.DTO;
using SaaS.Enum;
using SaaS.Helper;
using SaaS.Model;

namespace SaaS.Service;

public class ServerAdminService(Env env)
{
    private readonly Env _env = env;
    public async Task RegisterTenant(ConnectionModel connectionModel)
    {
        using NpgsqlConnection configCon = new NpgsqlConnection(_env.ConfigConnectionString);
        await configCon.OpenAsync();

        using NpgsqlTransaction configTran = await configCon.BeginTransactionAsync() as NpgsqlTransaction ?? throw new Exception("Failed to begin validation process.");

        ConnectionModel tenantServer = await GetTenantServer(configCon, configTran);
        connectionModel.ConnectionName = connectionModel.ConnectionName.ToLower(); 
        connectionModel.ConfigConnection = _env.ConfigConnectionModel;
        connectionModel.Database = $"tenant_{connectionModel.ConnectionName}_db";
        connectionModel.Username = $"tenant_{connectionModel.ConnectionName}";
        connectionModel.Password = CryptoHelper.GeneratePassword(12);
        connectionModel.Server = tenantServer.Server;
        connectionModel.Port = tenantServer.Port;
        connectionModel.TenantServerConnection = tenantServer;

        string encryptedConnectionModel = EncryptionHelper.Encrypt(connectionModel);


        try
        {
            int rowsAffected = await configCon.ExecuteAsync("insert into connectionInfo (connectionName) values (@connectionName)", new { connectionName = connectionModel.ConnectionName, connectionString = encryptedConnectionModel }, transaction: configTran);
        }
        catch (PostgresException ex) when (ex.SqlState == PGExceptionState.UniqueViolation.StateCode())
        {
            throw new Exception($"Connection Name: {connectionModel.ConnectionName} is already registered. Please choose another name.");
        }

        using NpgsqlConnection tenantServerCon = new NpgsqlConnection(tenantServer.ConnectionString);
        await tenantServerCon.OpenAsync();

        try
        {
            await ValidateNewTenantDeployability(connectionModel.ConnectionName, configCon, tenantServerCon);
            await DeployTenant(connectionModel, tenantServerCon);
        }
        catch (Exception ex)
        {
            _ = ex;
            await configTran.RollbackAsync();
            throw;
        }

        await configTran.CommitAsync();
    }

    public async Task<string> RegisterTanentServer(TenantServerRegistrationDTO serverRegistrationDTO)
    {
        using NpgsqlConnection configCon = new NpgsqlConnection(_env.ConfigConnectionString);
        await configCon.OpenAsync();


        ConnectionModel connectionModel = new ConnectionModel
        {
            Provider = serverRegistrationDTO.Provider,
            Server = serverRegistrationDTO.ServerName,
            Port = serverRegistrationDTO.Port
        };

        connectionModel.Username = serverRegistrationDTO.Username.ToLower();
        connectionModel.Password = serverRegistrationDTO.Password;

        if (string.IsNullOrEmpty(connectionModel.Username)) connectionModel.Username = CryptoHelper.GenerateRandomCharacterSet(8).ToLower();
        if (string.IsNullOrEmpty(connectionModel.Password)) connectionModel.Password = CryptoHelper.GeneratePassword(12);

        connectionModel.Database = connectionModel.Provider.BaseDbName();
        string encryptedConnectionModel = EncryptionHelper.Encrypt(connectionModel);

        if (!serverRegistrationDTO.AlreadyExists)
        {
            // TODO: Find a way to connect to server and create a role there.
            string createUserSql = $"CREATE ROLE {connectionModel.Username} LOGIN PASSWORD '{connectionModel.Password.Replace("'", "''")}' SUPERUSER CREATEDB CREATEROLE;";
            await configCon.ExecuteAsync(createUserSql);

            using NpgsqlTransaction configTran = await configCon.BeginTransactionAsync() as NpgsqlTransaction ?? throw new Exception("Failed to register tanent server.");
            await configCon.ExecuteAsync("insert into tenant_server (server_name, port, connection_string) values (@server_name, @port, @connection_string)", new { server_name = connectionModel.Server, port = connectionModel.Port, connection_string = encryptedConnectionModel }, transaction: configTran);
            await configTran.CommitAsync();
        }

        return connectionModel.ConnectionString;
    }

    public async Task ValidateNewTenantDeployability(string connectionName, NpgsqlConnection configCon, NpgsqlConnection tenantServerCon)
    {
        // make sure there in no existing databaseName in configCon server where databaseName = connectionName
        int dbExists = await tenantServerCon.QueryFirstOrDefaultAsync<int>("SELECT COUNT(*) FROM pg_database WHERE datname = @dbName", new { dbName = connectionName });
        if (dbExists > 0) throw new Exception($"A database named '{connectionName}' already exists on the server.");

        // make sure there is no existing server login user in configCon server where loginName = tenant_connectionName
        string expectedLogin = $"tenant_{connectionName}";
        int loginExists = await tenantServerCon.QueryFirstOrDefaultAsync<int>("SELECT COUNT(*) FROM pg_roles WHERE rolname = @loginName", new { loginName = expectedLogin });
        if (loginExists > 0) throw new Exception($"A server login/user named '{expectedLogin}' already exists.");
    }

    public async Task DeployTenant(ConnectionModel connectionModel, NpgsqlConnection tenantServerCon)
    {
        string dbName = $"tenant_{connectionModel.ConnectionName}_db";
        string loginName = $"tenant_{connectionModel.ConnectionName}";

        string createUserSql = $"CREATE ROLE {loginName} LOGIN PASSWORD '{connectionModel.Password.Replace("'", "''")}';";
        string createDbSql = $@"CREATE DATABASE {dbName} WITH TEMPLATE template_db OWNER {loginName};";
        string grantSql = $"GRANT ALL PRIVILEGES ON DATABASE {dbName} TO postgres;";

        try
        {
            await tenantServerCon.ExecuteAsync(createUserSql);
            await tenantServerCon.ExecuteAsync(createDbSql);
            await tenantServerCon.ExecuteAsync(grantSql);
        }
        catch (PostgresException ex) when (ex.SqlState == PGExceptionState.DuplicateObject.StateCode())
        {
            throw new Exception("Tenant already exists.");
        }
        catch (Exception ex)
        {
            _ = ex;
            throw;
        }
    }

    public async Task RegisterUser(LoginDTO loginDTO)
    {
        using NpgsqlConnection configCon = new NpgsqlConnection(_env.ConfigConnectionString);
        await configCon.OpenAsync();

        using NpgsqlTransaction configTran = await configCon.BeginTransactionAsync() as NpgsqlTransaction ?? throw new Exception("Failed to begin validation process.");

        try
        {
            int rowsAffected = await configCon.ExecuteAsync("insert into users (username, password, role, connection_name) values (@username, @password, @role, @connection_name)", new { username = loginDTO.UserName, password = EncryptionHelper.Hash(loginDTO.Password), role = loginDTO.Role, connection_name = loginDTO.ConnectionName }, transaction: configTran);
        }
        catch (PostgresException ex) when (ex.SqlState == PGExceptionState.UniqueViolation.StateCode())
        {
            throw new Exception($"Username: {loginDTO.UserName} is already registered. Please choose another name.");
        }

        await configTran.CommitAsync();
    }

    public async Task<ConnectionModel> GetTenantServer(NpgsqlConnection configCon, NpgsqlTransaction configTran)
    {
        // TODO: Fine tune which connectionstring to get once multiple tenant server is available.
        string encryptedConString = (await configCon.QueryFirstOrDefaultAsync<string>("select connection_string from tenant_server fetch next 1 rows only")) ?? throw new Exception($"No tenant server is available at the moment.");
        ConnectionModel connectionModel = EncryptionHelper.Decrypt<ConnectionModel>(encryptedConString);

        return connectionModel;
    }
}
