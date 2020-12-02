using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Shared.Hosting.Models
{
    public class FailureDetails
    {
        public FailureDetails()
        {
            Title = "One or more errors occurred.";
            Status = StatusCodes.Status500InternalServerError;

            Extensions = new Dictionary<string, object>();
            Errors = new Dictionary<string, IReadOnlyList<string>>();
        }

        public FailureDetails(string traceId)
            : this()
        {
            Title = "One or more validation errors occurred.";
            Detail = "The inputs supplied to the API are invalid";
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5";
            Status = StatusCodes.Status400BadRequest;

            Extensions.Add("traceId", traceId);
        }

        [JsonProperty("detail", NullValueHandling = NullValueHandling.Ignore)]
        public string Detail { get; set; }

        [JsonProperty("instance", NullValueHandling = NullValueHandling.Ignore)]
        public string Instance { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public int? Status { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("errors")]
        public IDictionary<string, IReadOnlyList<string>> Errors { get; }

        [JsonExtensionData]
        public IDictionary<string, object> Extensions { get; }

        public bool ShouldSerializeErrors() => Errors.Any();
    }
}
