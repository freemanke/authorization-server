using System;
using Microsoft.IdentityModel.Tokens;

namespace AuthorizationServer.TokenProviders
{
    public class TokenProviderOptions
    {
        public string Path { get; set; } = "/oauth2/token";
        public string Issuer { get; set; } = "Authorization Server";
        public string Audience { get; set; }
        public TimeSpan Expiration { get; set; } = TimeSpan.FromHours(8);
        public SigningCredentials SigningCredentials { get; set; }
    }
}
