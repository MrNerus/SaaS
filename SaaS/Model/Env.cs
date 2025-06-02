using System;
using System.Text.Json;

namespace SaaS.Model;

public class Env
{
    public string EncryptionKey { get; set; } = string.Empty;
    public string ConfigConnection { get; set; } = string.Empty;
    public string RedisConnection { get; set; } = string.Empty;

    private ConnectionModel? _configConnectionModel;
    private ConnectionModel? _redisConnectionModel;

    public ConnectionModel ConfigConnectionModel
    {
        get
        {
            if (this._configConnectionModel == null)
                _configConnectionModel = JsonSerializer.Deserialize<ConnectionModel>(this.ConfigConnection) ?? throw new Exception("Invalid config connection data.");
            return _configConnectionModel;
        }
    }

    public ConnectionModel RedisConnectionModel
    {
        get
        {
            if (this._redisConnectionModel == null)
                _redisConnectionModel = JsonSerializer.Deserialize<ConnectionModel>(this.RedisConnection) ?? throw new Exception("Invalid redis connection data.");
            return _redisConnectionModel;
        }
    }

    public string ConfigConnectionString => ConfigConnectionModel.ConnectionString;
    public string RedisConnectionString => RedisConnectionModel.ConnectionString;

}