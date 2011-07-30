using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using hi5_auth.Json;
using hi5_auth.Models;
using OAuth2.Mvc;

namespace hi5_auth.Controllers
{
    public class OAuthController : Controller
    {
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult RequestToken()
        {
            var response = OAuthServiceBase.Instance.RequestToken();

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult AccessToken(string grant_type, string username, string password, bool? persistent)
        {
            var requestToken = Request.GetToken();
            var response = OAuthServiceBase.Instance.AccessToken(requestToken, grant_type, username, password, persistent.HasValue && persistent.Value);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult RefreshToken(string refreshToken)
        {
            if (String.IsNullOrEmpty(refreshToken))
                refreshToken = Request.GetToken();

            var response = OAuthServiceBase.Instance.RefreshToken(refreshToken);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult Unauthorize()
        {
            var response = new JsonResponse();

            var accessToken = Request.GetToken();
            response.Success = OAuthServiceBase.Instance.UnauthorizeToken(accessToken);

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        protected override JsonResult Json(object data, string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return Request.IsJsonpRequest()
                ? new JsonpResult(data, contentType, contentEncoding, behavior)
                : base.Json(data, contentType, contentEncoding, behavior);
        }
    }
}
