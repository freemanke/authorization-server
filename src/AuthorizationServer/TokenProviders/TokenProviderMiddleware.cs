using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using System.Security.Principal;
using AuthorizationServer.Entities;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Collections.Generic;

namespace AuthorizationServer.TokenProviders
{
    /// <summary>
    /// JWT授权服务中间件，
    /// 用户通过访问指定授权接口获取JWT格式授权码。
    /// 后续客户端（WEB,Android,IOS,Window)通过在请求头或Cookies添加JWT访问资源服务器的API服务。
    /// </summary>
    public class TokenProviderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TokenProviderOptions _options;

        public TokenProviderMiddleware(RequestDelegate next, IOptions<TokenProviderOptions> options)
        {
            _next = next;
            _options = options.Value;
        }

        public Task Invoke(HttpContext context)
        {
            // If the request path doesn't match, skip
            if (!context.Request.Path.Equals(_options.Path, StringComparison.Ordinal))
            {
                return _next(context);
            }

            // Request must be POST with Content-Type: application/x-www-form-urlencoded
            if (!context.Request.Method.Equals("POST")
               || !context.Request.HasFormContentType)
            {
                context.Response.StatusCode = 400;
                return context.Response.WriteAsync("Bad request.");
            }

            var username = context.Request.Form["username"];
            var password = context.Request.Form["password"];
            var clientId = context.Request.Form["client_id"];

            if (string.IsNullOrEmpty(clientId))
            {
                context.Response.StatusCode = 400;
                return context.Response.WriteAsync(JsonConvert.SerializeObject(
                    new { error = "invalid client_id", error_description = "client_id is not set." }));
            }
            if (string.IsNullOrEmpty(username))
            {
                context.Response.StatusCode = 400;
                return context.Response.WriteAsync(JsonConvert.SerializeObject(
                    new { error = "invalid username", error_description = "username is not set." }));
            }
            if (string.IsNullOrEmpty(password))
            {
                context.Response.StatusCode = 400;
                return context.Response.WriteAsync(JsonConvert.SerializeObject(
                    new { error = "invalid password", error_description = "password is not set." }));
            }

            var audience = GetAudience(clientId).Result;
            if (audience == null)
            {
                context.Response.StatusCode = 400;
                return context.Response.WriteAsync("No audience registered with this client_id.");
            }

            //  Verify username and password
            var identity = GetIdentity(username, password).Result;
            if (identity == null)
            {
                context.Response.StatusCode = 400;
                return context.Response.WriteAsync("Invalid username or password.");
            }

            return GenerateToken(context, identity, audience);
        }

        private async Task GenerateToken(HttpContext context, ClaimsIdentity id, Audience audience)
        {
            // Specifically add the jti (random nonce), iat (issued timestamp), and sub (subject/user) claims.
            // You can add other claims here, if you want:
            var now = DateTime.UtcNow;
            var lat = (now - new DateTime(1970, 1, 1)).TotalSeconds + TimeSpan.FromMinutes(30).TotalSeconds;
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, lat.ToString(), ClaimValueTypes.Integer64),
                new Claim("Name", id.Name),
                new Claim("Role", "User"),
                new Claim("Role", "Admin"),
                new Claim("DateOfBirth", DateTime.Now.ToString()),
            };

            // Create the JWT and write it to a string
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(audience.Base64Secret));
            var jwt = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: audience.Name,
                claims: claims,
                notBefore: now,
                expires: now.Add(_options.Expiration),
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            var response = new
            {
                access_token = encodedJwt,
                expires_in = (int)_options.Expiration.TotalSeconds
            };

            // Serialize and return the response
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented }));
        }

        private Task<Audience> GetAudience(string clientId)
        {
            return Task.FromResult(AudiencesStore.FindAudience(clientId));
        }

        private Task<ClaimsIdentity> GetIdentity(string username, string password)
        {
            // DON'T do this in production, obviously!
            if (username == password)
            {
                return Task.FromResult(new ClaimsIdentity(new GenericIdentity(username, "Token"), new Claim[] { }));
            }

            // Credentials are invalid, or account doesn't exist
            return Task.FromResult<ClaimsIdentity>(null);
        }
    }
}
