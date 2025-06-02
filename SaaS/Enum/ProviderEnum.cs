namespace SaaS.Enum;

public enum ProviderEnum
{
    Npg,
    SqlServer,
    Redis
}

public static class ProviderEnumExtensions
{
    public static string BaseDbName(this ProviderEnum provider)
    {
        return provider switch
        {
            ProviderEnum.Npg => "postgres",
            ProviderEnum.SqlServer => "master",
            _ => throw new Exception("Unsupported provider")
        };
    }
}