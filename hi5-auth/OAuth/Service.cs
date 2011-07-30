using System;
using System.Collections.Generic;
using System.Linq;
using OAuth2.Mvc;

namespace hi5_auth.OAuth
{
    public class Service : OAuthServiceBase
    {
        static Service()
        {
            Tokens = new List<Token>();
            RequestTokens = new Dictionary<string, DateTime>();
        }

        public static List<Token> Tokens { get; set; }

        public static Dictionary<String, DateTime> RequestTokens { get; set; }

        public override OAuthResponse RequestToken()
        {
            var token = Guid.NewGuid().ToString("N");
            var expire = DateTime.Now.AddMinutes(5);
            RequestTokens.Add(token, expire);

            return new OAuthResponse
            {
                Expires = (int)expire.Subtract(DateTime.Now).TotalSeconds,
                RequestToken = token,
                RequireSsl = false,
                Success = true
            };
        }

        public override OAuthResponse AccessToken(string requestToken, string grantType, string userName, string password, bool persistent)
        {
            // This should go out to a DB and get the users saved information.
            var hash = (requestToken + userName).ToSHA1();

            if (hash.Equals(password, StringComparison.OrdinalIgnoreCase))
                return CreateAccessToken(userName);

            return new OAuthResponse
            {
                Success = false
            };
        }

        public override OAuthResponse RefreshToken(string refreshToken)
        {
            var token = Tokens.FirstOrDefault(t => t.RefreshToken == refreshToken);

            if (token == null)
                return new OAuthResponse
                {
                    Error = "RefreshToken not found.",
                    Success = false
                };

            if (token.IsRefreshExpired)
                return new OAuthResponse
                {
                    Error = "RefreshToken expired.",
                    Success = false
                };

            Tokens.Remove(token);
            return CreateAccessToken(token.Name);
        }

        private OAuthResponse CreateAccessToken(string name)
        {
            var token = new Token(name);
            Tokens.Add(token);

            return new OAuthResponse
            {
                AccessToken = token.AccessToken,
                Expires = token.ExpireSeconds,
                RefreshToken = token.RefreshToken,
                RequireSsl = false,
                Success = true
            };
        }

        public override bool UnauthorizeToken(string accessToken)
        {
            var token = Tokens.FirstOrDefault(t => t.AccessToken == accessToken);
            if (token == null)
                return false;

            Tokens.Remove(token);
            return true;
        }
    }
}