using System;
using SaaS.DTO;
using SaaS.Model;
using Npgsql;
using Dapper;
using SaaS.Helper;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Text.Json;


namespace SaaS.Service;

public class LoginService(Env env, RedisService redis)
{
    private readonly Env _env = env;
    private readonly RedisService _redis = redis;

    public async Task<UserProfile> Login(LoginDTO loginDTO)
    {
        using NpgsqlConnection configCon = new NpgsqlConnection(_env.ConfigConnectionString);
        await configCon.OpenAsync();

        UserInstance userInstance = await configCon.QueryFirstOrDefaultAsync<UserInstance>("select username, password, role from users where username = @username", new { username = loginDTO.UserName }) ?? throw new Exception($"'{loginDTO.UserName}' is not a registered user.");
        string encryptedConString = (await configCon.QueryFirstOrDefaultAsync<string>("select connection_string from connection_info where connection_name = @connectionName and username = @username", new { connectionName = loginDTO.ConnectionName, username = loginDTO.UserName })) ?? throw new Exception($"User: '{loginDTO.UserName}' doesnot has access to Connection Name: '{loginDTO.ConnectionName}'.");
        ConnectionModel userConnectionModel = EncryptionHelper.Decrypt<ConnectionModel>(encryptedConString);
        userInstance.UserType = "Tenant";
        userInstance.ConnectionString = userConnectionModel.ConnectionString;
        userInstance.ConnectionName = userConnectionModel.ConnectionName;


        bool isPasswordCorrect = EncryptionHelper.VerifyHash(loginDTO.Password, userInstance.Password);
        if (!isPasswordCorrect) throw new Exception("Username or password is incorrect.");

        GetToken(userInstance);
        bool wasSet = await _redis.SetIfNotExistsAsync(userInstance.LoginId, JsonSerializer.Serialize(userInstance), userInstance.TokenLifeSpanInSecond);

        // Note to Self:
        // Since LoginId is combination of 2 unique GUID, It is 64 character long. There wayyyy too low probability for such LoginId to already exist in Redis.
        // But even if universe decided to screw you up and generate existing GUID, Try to generate token again with new LoginId. 
        // If that too fails, Bro.. your life is miserable. Just Accept. Give up. 
        if (!wasSet)
        {
            GetToken(userInstance);
            bool wasSetAgain = await _redis.SetIfNotExistsAsync(userInstance.LoginId, JsonSerializer.Serialize(userInstance as UserInstance), userInstance.TokenLifeSpanInSecond);

            if (!wasSetAgain) throw new Exception("Unable to generate token.");
        }

        return userInstance as UserProfile;
    }

    public async Task<UserProfile> ConsoleLogin(LoginDTO loginDTO)
    {
        using NpgsqlConnection configCon = new NpgsqlConnection(_env.ConfigConnectionString);
        await configCon.OpenAsync();

        UserInstance userInstance = await configCon.QueryFirstOrDefaultAsync<UserInstance>("select username, password, role from users where username = @username", new { username = loginDTO.UserName }) ?? throw new Exception($"'{loginDTO.UserName}' is not a registered user.");
        bool isPasswordCorrect = EncryptionHelper.VerifyHash(loginDTO.Password, userInstance.Password);
        userInstance.UserType = "Console";

        if (!isPasswordCorrect) throw new Exception("Username or password is incorrect.");

        GetToken(userInstance);
        bool wasSet = await _redis.SetIfNotExistsAsync(userInstance.LoginId, JsonSerializer.Serialize(userInstance), userInstance.TokenLifeSpanInSecond);

        // Note to Self:
        // Since LoginId is combination of 2 unique GUID, It is 64 character long. There wayyyy too low probability for such LoginId to already exist in Redis.
        // But even if universe decided to screw you up and generate existing GUID, Try to generate token again with new LoginId. 
        // If that too fails, Bro.. your life is miserable. Just Accept. Give up. 
        if (!wasSet)
        {
            GetToken(userInstance);
            bool wasSetAgain = await _redis.SetIfNotExistsAsync(userInstance.LoginId, JsonSerializer.Serialize(userInstance as UserInstance), userInstance.TokenLifeSpanInSecond);

            if (!wasSetAgain) throw new Exception("Unable to generate token.");
        }

        return userInstance as UserProfile;
    }

    public void GetToken(UserInstance userInstance)
    {
        userInstance.LoginId = $"{userInstance.ConnectionName}-{userInstance.Username}-{CryptoHelper.GenerateGuid()}";

        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        byte[] key = Encoding.UTF8.GetBytes(_env.EncryptionKey);

        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("username", userInstance.Username),
                new Claim("connectionName", userInstance.ConnectionName),
                new Claim("loginId", userInstance.LoginId),
                new Claim(ClaimTypes.Role, userInstance.Role),
                new Claim("userType", userInstance.UserType)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);
        string token = tokenHandler.WriteToken(securityToken);

        userInstance.Token = token;
        userInstance.TokenLifeSpanInSecond = TimeSpan.FromDays(7);
        userInstance.TokenSetDateTime = DateTime.UtcNow;
    }


    public async Task<UserInstance> GetLoggedInProfile(string loginId)
    {
        string userInstanceString = await _redis.GetAsync(loginId) ?? throw new Exception("Sesson has been expired or is invalid. Please Log back in again to continue.");

        UserInstance userInstance = JsonSerializer.Deserialize<UserInstance>(userInstanceString) ?? throw new Exception("Invalid Session Data.");
        return userInstance;
    }

    public async Task<bool> Logout(string loginId)
    {
        return await _redis.DeleteKeyAsync(loginId);
    }
}
