using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Json
{
    public static class Json
    {
        private static JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter>()
            {
                new StringEnumConverter(),
                new VersionConverter(),
                new DateTimeOffsetConvertor(),
                new DateTimeOffsetNullableConvertor(),
                new TimeSpanConverter(),
                new TimeSpanNullableConvertor()
            },
            DateParseHandling = DateParseHandling.DateTimeOffset,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
        };

        public static string Serialize<T>(T item)
        {
            return JsonConvert.SerializeObject(item, settings);
        }

        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, settings);
        }
    }
}
