using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace OAuth2.Mvc
{
    [DataContract(Name = "oauthResponse")]
    [JsonObject(MemberSerialization.OptIn)]
    public class OAuthResponse
    {
        [DataMember(Name = "request_token")]
        [JsonProperty("request_token", NullValueHandling = NullValueHandling.Ignore)]
        public string RequestToken { get; set; }

        [DataMember(Name = "access_token")]
        [JsonProperty("access_token", NullValueHandling = NullValueHandling.Ignore)]
        public string AccessToken { get; set; }

        [DefaultValue(0)]
        [DataMember(Name = "expires_in")]
        [JsonProperty("expires_in", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Expires { get; set; }

        [DataMember(Name = "refresh_token")]
        [JsonProperty("refresh_token", NullValueHandling = NullValueHandling.Ignore)]
        public string RefreshToken { get; set; }

        [DataMember(Name = "scope")]
        [JsonProperty("scope", NullValueHandling = NullValueHandling.Ignore)]
        public string Scope { get; set; }

        [DataMember(Name = "error")]
        [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
        public string Error { get; set; }

        [DataMember(Name = "success")]
        [JsonProperty("success")]
        public bool Success { get; set; }

        [DefaultValue(false)]
        [DataMember(Name = "require_ssl")]
        [JsonProperty("require_ssl", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool RequireSsl { get; set; }

        public void Clear()
        {
            AccessToken = null;
            Expires = 0;
            RefreshToken = null;
            Scope = null;
            Error = null;
            RequireSsl = false;
            Success = false;
        }

        public void SetExpires(DateTime expireDate)
        {
            Expires = (int)expireDate.Subtract(DateTime.Now).TotalSeconds;
        }
    }
}