using System;

namespace SaaS.DTO;

public class LoginDTO
{
    private string _userName = string.Empty;
    private string _password = string.Empty;
    private string _connectionName = string.Empty;
    private string _role = string.Empty;
    public string UserName
    {
        get => _userName;
        set => _userName = value?.Trim() ?? string.Empty;
    }
    public string Password
    {
        get => _password;
        set => _password = value?.Trim() ?? string.Empty;
    }
    public string ConnectionName
    {
        get => _connectionName;
        set => _connectionName = value?.Trim() ?? string.Empty;
    }
    public string Role
    {
        get => _role;
        set => _role = value?.Trim() ?? string.Empty;
    }
}

public static class LoginDTOExtensions
{
    public static bool hasValidData(this LoginDTO dto)
    {
        if (string.IsNullOrEmpty(dto.UserName) || string.IsNullOrEmpty(dto.Password))
        {
            return false;
        }

        return true;
    }
}