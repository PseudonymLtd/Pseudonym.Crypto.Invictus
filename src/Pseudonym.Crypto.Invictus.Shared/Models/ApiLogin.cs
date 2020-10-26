using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiLogin
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}
