using System;
using System.Security.Principal;
using System.Web;
using OAuth2.Mvc;

namespace hi5_auth.OAuth
{
    public class Provider : OAuthProviderBase
    {
        public override IIdentity RetrieveIdentity(HttpContext context)
        {
            var token = context.Request.GetToken();
            return String.IsNullOrEmpty(token)
                ? null
                : new Identity(this, token);
        }

        public override IPrincipal CreatePrincipal(IIdentity identity)
        {
            return identity == null || !(identity is Identity)
                ? null
                : new Principal(this, identity);
        }
    }
}