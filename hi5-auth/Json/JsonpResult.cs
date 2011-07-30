using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace hi5_auth.Json
{
    public class JsonpResult : JsonResult
    {
        public const string JsonCallbackKey = "jsoncallback";

        public JsonpResult(object data)
        {
            Data = data;
        }

        public JsonpResult(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            Data = data;
            ContentType = contentType;
            ContentEncoding = contentEncoding;
            JsonRequestBehavior = behavior;
        }

        public string JsonCallback { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            JsonCallback = context.HttpContext.Request[JsonCallbackKey];

            if (string.IsNullOrEmpty(JsonCallback))
                JsonCallback = context.HttpContext.Request["callback"];

            if (string.IsNullOrEmpty(JsonCallback))
                throw new ArgumentNullException("JsonCallback required for JSONP response.");

            var response = context.HttpContext.Response;
            response.ContentType = !String.IsNullOrEmpty(ContentType) ? ContentType : "application/json";

            if (ContentEncoding != null)
                response.ContentEncoding = ContentEncoding;

            if (Data != null)
            {
                var serializedObject = JsonConvert.SerializeObject(Data, Formatting.Indented, JsonHelper.DefaultSerializerSettings);
                response.Write(String.Format("{0}({1});", JsonCallback, serializedObject));
            }
        }
    }
}