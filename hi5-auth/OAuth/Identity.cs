using System.Linq;
using OAuth2.Mvc;

namespace hi5_auth.OAuth
{
    public class Identity : OAuthIdentityBase
    {
        public Identity(IOAuthProvider provider, string token)
            : base(provider)
        {
            Token = token;
            Realm = "hi5-auth";
        }

        protected override void Load()
        {
            var token = Service.Tokens.FirstOrDefault(t => t.AccessToken == Token && !t.IsAccessExpired);
            if (token == null)
                return;

            IsAuthenticated = true;
            Name = token.Name;
        }
    }
}