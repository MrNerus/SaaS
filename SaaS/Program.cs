using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SaaS.Controller;
using SaaS.Middleware;
using SaaS.Model;
using SaaS.Service;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

Env env = new Env
{
    EncryptionKey = builder.Configuration["EncryptionKey"] ?? throw new Exception("Encryption key is not found in this envrionment"),
    ConfigConnection = builder.Configuration["ConfigConnection"] ?? throw new Exception("Config connection string is not found in this envrionment"),
    RedisConnection = builder.Configuration["RedisConnection"] ?? throw new Exception("Redis connection string is not found in this envrionment"),
};

builder.Services.AddSingleton(env);
builder.Services.AddSingleton<RedisService>();
builder.Services.AddTransient<LoginService>();
builder.Services.AddTransient<ServerAdminService>();
builder.Services.AddTransient<DashboardService>();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(env.EncryptionKey))
        };
    });
builder.Services.AddAuthorization();


builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();


var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<UserContextMiddleware>();

app.MapControllers();
app.Run();