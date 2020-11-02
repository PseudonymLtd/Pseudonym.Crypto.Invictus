using Newtonsoft.Json;

#pragma warning disable SA1402

namespace Pseudonym.Crypto.Invictus.Web.Client.Utils.Interop
{
    public sealed class JClickData
    {
        [JsonRequired]
        [JsonProperty("_model")]
        public JModel Model { get; set; }
    }

    public sealed class JModel
    {
        [JsonRequired]
        [JsonProperty("label")]
        public string Label { get; set; }
    }
}
