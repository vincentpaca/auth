using System;
using System.Configuration;
using System.Web;
using System.Web.Configuration;
using OAuth2.Mvc.Configuration;

namespace OAuth2.Mvc.Module
{
    /// <summary>
    /// Sets the identity of the user for an ASP.NET application when forms 
    /// authentication is OAuth. This class cannot be inherited.
    /// </summary>
    public sealed class OAuthAuthenticationModule : IHttpModule
    {
        private static IOAuthProvider _provider;
        private static bool _checked;
        private static bool _required;

        /// <summary>
        /// Occurs when the application authenticates the current request.
        /// </summary>
        public event OAuthAuthenticationEventHandler Authenticate;

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">
        /// An <see cref="T:System.Web.HttpApplication"/> that provides access 
        /// to the methods, properties, and events common to all  application 
        /// objects within an ASP.NET application.
        /// </param>
        public void Init(HttpApplication context)
        {
            context.AuthenticateRequest += OnAuthenticateRequest;
            context.EndRequest += OnEndRequest;
        }
        
        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that 
        /// implements <see cref="T:System.Web.IHttpModule"/>.
        /// </summary>
        public void Dispose()
        { }

        private void OnAuthenticateRequest(object sender, EventArgs e)
        {
            if (_checked && !_required)
                return;

            var application = sender as HttpApplication;
            if (application == null)
                return;

            var context = application.Context;
            if (context == null)
                return;

            if (!_checked)
            {
                InitializeProvider();
                InitializeService();
                _required = _provider != null;
                _checked = true;
            }

            if (!_required)
                return;

            var identity = _provider.RetrieveIdentity(context);
            var eventArgs = new OAuthAuthenticationEventArgs(_provider, identity, context);
            OnAuthenticate(eventArgs);
        }
        
        private void OnEndRequest(object sender, EventArgs e)
        {
            if (!_required)
                return;
            
            var application = sender as HttpApplication;
            if (application == null)
                return;

            var context = application.Context;
            if (context == null)
                return;

            if (context.Response.StatusCode != 401)
                return;

            // add challenge header
            string header = UnauthorizedHeader(context);
            context.Response.AddHeader(OAuthConstants.UnauthorizedHeader, header);

            string url = UnauthorizedPage();
            RedirectToErrorPage(context, url);
        }

        private string UnauthorizedHeader(HttpContext context)
        {
            if (context.User == null)
                return "OAuth realm=\"OAuth\"";

            var u = context.User as OAuthPrincipalBase;
            if (u == null)
                return "OAuth realm=\"OAuth\"";

            var i = u.Identity as OAuthIdentityBase;
            if (i == null)
                return "OAuth realm=\"OAuth\"";

            string header = String.Format("OAuth realm=\"{0}\"", i.Realm ?? "OAuth");
            if (!String.IsNullOrEmpty(i.Error))
                header += String.Format(", error=\"{0}\"", i.Error);

            return header;
        }

        private void OnAuthenticate(OAuthAuthenticationEventArgs e)
        {
            if (Authenticate != null)
            {
                Authenticate(this, e);
                if (e.Context.User == null && e.User != null)
                    e.Context.User = e.User;
            }

            if (e.Context.User == null)
                e.Context.User = e.Provider.CreatePrincipal(e.Identity);
        }

        private static void InitializeProvider()
        {
            if (_provider != null)
                return;

            var section = ConfigurationManager.GetSection("oauth") as OAuthSection;

            // if there is no oauth section, then don't use this module
            if (section == null)
                return;

            var providers = new OAuthProviderCollection();
            ProvidersHelper.InstantiateProviders(section.Providers, providers, typeof(OAuthProviderBase));

            if ((section.DefaultProvider == null) || (providers.Count < 1))
                ThrowConfigurationError(section, "Default OAuth Provider must be specified.");

            _provider = providers[section.DefaultProvider];
            if (_provider == null)
                ThrowConfigurationError(section, "Default OAuth Provider could not be found.");
        }

        private static void InitializeService()
        {
            if (OAuthServiceBase.Instance != null)
                return;

            var section = ConfigurationManager.GetSection("oauth") as OAuthSection;

            // if there is no oauth section, then don't use this module
            if (section == null)
                return;

            var services = new OAuthServiceCollection();
            ProvidersHelper.InstantiateProviders(section.Services, services, typeof(OAuthServiceBase));

            if ((section.DefaultProvider == null) || (services.Count < 1))
                ThrowConfigurationError(section, "Default OAuth Service must be specified.");

            OAuthServiceBase.Instance = services[section.DefaultService];
            if (_provider == null)
                ThrowConfigurationError(section, "Default OAuth Service could not be found.");
        }

        private static void ThrowConfigurationError(OAuthSection section, string message)
        {
            var elementInformation = section.ElementInformation;
            var propertyInformation = elementInformation.Properties["defaultProvider"];

            if (propertyInformation == null)
                throw new ConfigurationErrorsException(message);

            throw new ConfigurationErrorsException(
                message,
                propertyInformation.Source,
                propertyInformation.LineNumber);
        }

        private static void RedirectToErrorPage(HttpContext context, string url)
        {
            if (string.IsNullOrEmpty(url))
                return;

            if (!context.IsCustomErrorEnabled)
                return;

            context.Response.ClearContent();
            context.Response.Redirect(url, true);
        }
        
        private static string UnauthorizedPage()
        {
            // Get the object related to the <identity> section.
            var errorsSection = WebConfigurationManager.GetSection("system.web/customErrors") as CustomErrorsSection;
            if (errorsSection == null)
                return string.Empty;
            
            if (null == errorsSection.Errors["401"])
                return string.Empty;

            string redirectUrl = errorsSection.Errors["401"].Redirect;
            return redirectUrl;
        }
    }
}
