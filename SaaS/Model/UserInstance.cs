using System;

namespace SaaS.Model;

public class UserProfile
{
    public string Username { get; set; } = string.Empty;
    public string ConnectionName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string LoginId { get; set; } = string.Empty;
    public TimeSpan? TokenLifeSpanInSecond { get; set; } = null;
    public DateTime? TokenSetDateTime { get; set; } = null;
    public string Role { get; set; } = string.Empty;

}
public class UserInstance: UserProfile
{
    public string ConnectionString { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}