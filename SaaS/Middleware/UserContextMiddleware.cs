using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using SaaS.Model;
using SaaS.Service;

namespace SaaS.Middleware;

public class UserContextMiddleware
{
    private readonly RequestDelegate _next;

    public UserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, RedisService redis)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");

        if (!string.IsNullOrWhiteSpace(token))
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var loginId = jwtToken.Claims.FirstOrDefault(c => c.Type == "loginId")?.Value;

            if (!string.IsNullOrEmpty(loginId))
            {
                var userInstanceJson = await redis.GetAsync(loginId);
                if (!string.IsNullOrEmpty(userInstanceJson))
                {
                    UserInstance? userInstance = JsonSerializer.Deserialize<UserInstance>(userInstanceJson);
                    if (userInstance == null) {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Invalid session details.");
                        return;
                    }
                    
                    context.Items["UserInstance"] = userInstance;
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Invalid or expired session.");
                    return;
                }
            }
        }

        await _next(context);
    }
}
