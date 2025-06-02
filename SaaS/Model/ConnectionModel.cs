using System;
using System.Text.Json;
using SaaS.Enum;
using SaaS.Helper;

namespace SaaS.Model;

public class ConnectionModel
{
    public string Server { get; set; } = string.Empty;
    public string Port { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public ProviderEnum Provider { get; set; } = ProviderEnum.Npg;
    public string ConnectionName { get; set; } = string.Empty;
    public ConnectionModel? ConfigConnection { get; set; } = null;
    public ConnectionModel? TenantServerConnection { get; set; } = null;
    public string ConnectionString
    {
        get
        {
            string connectionString;
            switch (this.Provider)
            {
                case ProviderEnum.Npg:
                    connectionString = $"Host={this.Server};Port={this.Port};Database={this.Database};Username={this.Username};Password={this.Password}";
                    break;
                case ProviderEnum.SqlServer:
                    connectionString = $"Server={this.Server};Database={this.Database};User Id={this.Username};Password={this.Password}";
                    break;
                case ProviderEnum.Redis:
                    connectionString = $"{this.Server}:{this.Port}";
                    if (!string.IsNullOrWhiteSpace(this.Password)) connectionString += $",password={this.Password}";
                    break;
                default:
                    throw new Exception("Unsupported provider");
            }
            return connectionString;
        }
    }
}