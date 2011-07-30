using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace hi5_auth.Json
{
    public static class HttpRequestExtensions
    {
        public static bool IsJsonpRequest(this HttpRequestBase request)
        {
            return request.Params.AllKeys.Contains(JsonpResult.JsonCallbackKey);
        }
    }
}