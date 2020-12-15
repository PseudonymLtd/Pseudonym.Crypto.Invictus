using Newtonsoft.Json;

#pragma warning disable SA1402

namespace Pseudonym.Crypto.Invictus.Web.Client.Utils.Interop
{
    public sealed class JPieClickData
    {
        [JsonRequired]
        [JsonProperty("_model")]
        public JPieModel Model { get; set; }
    }

    public sealed class JPieModel
    {
        [JsonRequired]
        [JsonProperty("label")]
        public string Label { get; set; }
    }

    public sealed class JLineClickData
    {
        [JsonRequired]
        [JsonProperty("_datasetIndex")]
        public int ParentIndex { get; set; }

        [JsonRequired]
        [JsonProperty("_index")]
        public int Index { get; set; }
    }
}
