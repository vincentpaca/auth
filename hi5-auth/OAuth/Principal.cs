using System.Security.Principal;
using OAuth2.Mvc;

namespace hi5_auth.OAuth
{
    public class Principal : OAuthPrincipalBase
    {
         public Principal(IOAuthProvider provider, IIdentity identity)
            : base(provider, identity)
        { }

        protected override void Load()
        {
            // Everyone is an admin in the demo!
            Roles = new[] {"Admin"};
        }
    }
}