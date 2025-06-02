using System;
using System.Text;
using System.Text.Json;
using SaaS.Model;
using StackExchange.Redis;

namespace SaaS.Service;


public class RedisService
{
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly Env _env;

    public RedisService(IConfiguration config, Env env)
    {
        _env = env;

        ConnectionModel redisConnectionModel = _env.RedisConnectionModel;

        var options = new ConfigurationOptions
        {
            EndPoints = {
                { redisConnectionModel.Server, Convert.ToInt32(redisConnectionModel.Port) }
            },
            Ssl = false, 
            AbortOnConnectFail = false
        };
        if (!string.IsNullOrEmpty(redisConnectionModel.Username)) options.User = redisConnectionModel.Username;
        if (!string.IsNullOrEmpty(redisConnectionModel.Password)) options.Password = redisConnectionModel.Password;

        _redis = ConnectionMultiplexer.Connect(options);

        if (!_redis.IsConnected) throw new Exception("Redis connection failed.");
        
        _db = _redis.GetDatabase();
    }

    public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
    {
        await _db.StringSetAsync(key, value, expiry);
    }

    public async Task<bool> SetIfNotExistsAsync(string key, string value, TimeSpan? expiry)
    {
        return await _db.StringSetAsync(key, value, expiry, when: When.NotExists);
    }

    public async Task<string?> GetAsync(string key)
    {
        return await _db.StringGetAsync(key);
    }

    public async Task<bool> DeleteKeyAsync(string key)
    {
        return await _db.KeyDeleteAsync(key);
    }
}
