﻿using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace hi5_auth.Json
{
    public class JsonHelper
    {
        public static readonly JsonSerializerSettings DefaultSerializerSettings;

        static JsonHelper()
        {
            DefaultSerializerSettings = new JsonSerializerSettings();

            var timeConverter = new IsoDateTimeConverter();
            timeConverter.DateTimeStyles = DateTimeStyles.AssumeLocal;

            DefaultSerializerSettings.Converters.Add(timeConverter);

            DefaultSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            DefaultSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            DefaultSerializerSettings.NullValueHandling = NullValueHandling.Include;
            DefaultSerializerSettings.DefaultValueHandling = DefaultValueHandling.Include;
            DefaultSerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
        }
    }
}