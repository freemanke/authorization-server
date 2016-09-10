using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using AuthorizationServer.TokenProviders;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using AuthorizationServer.Entities;

namespace AuthorizationServer
{
    public class Startup
    {
        const string issuer = "Authorization Server";
        const string audienceName = "ResourceServer 1#";
        const string ClientId = "RS1001";
        const string base64Secret = "DAFDASEEREGAGAGAGDAFDAERWEAGRAGASDGADG";

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // 配置资源服务器的用于验证JWT
            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme‌​)
                    .RequireAuthenticatedUser()
                    .Build());
            });

            // Add framework services.
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // 配置授权服务器用于签发JWT
            var tokenOptions = new TokenProviderOptions { Issuer = issuer };
            app.UseMiddleware<TokenProviderMiddleware>(Options.Create(tokenOptions));

            // 配置资源服务器的用于验证JWT
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(base64Secret));
            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,        // The signing key must match!
                    IssuerSigningKey = signingKey,
                    ValidateIssuer = true,                  // 是否验证JWT发行方Validate the JWT Issuer (iss) claim
                    ValidIssuer = issuer,
                    ValidateAudience = true,                // Validate the JWT Audience (aud) claim
                    ValidAudience = audienceName,           // 
                    ValidateLifetime = true,                // Validate the token expiry
                    ClockSkew = TimeSpan.Zero               // If you want to allow a certain amount of clock drift, set that here:
                }
            });

            app.UseMvc();
        }
    }
}
