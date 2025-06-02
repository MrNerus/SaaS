using System;
using SaaS.Enum;
using SaaS.Model;

namespace SaaS.DTO;

public class TenantServerRegistrationDTO
{
    public string ServerName { get; set; } = string.Empty;
    public string Port { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public ProviderEnum Provider { get; set; } = ProviderEnum.Npg;
    public bool AlreadyExists { get; set; } = false;
}
