using System.Collections.Generic;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Pseudonym.Crypto.Invictus.Web.Client.Utils.Interop
{
    internal abstract class JService
    {
        protected static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter>()
            {
                new StringEnumConverter(),
                new VersionConverter(),
                new IsoDateTimeConverter()
            }
        };

        public JService(IJSRuntime jSRuntime)
        {
            Runtime = (IJSInProcessRuntime)jSRuntime;
        }

        protected IJSInProcessRuntime Runtime { get; }
    }
}
