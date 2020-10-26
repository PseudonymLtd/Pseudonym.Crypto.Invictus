using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Exceptions;

namespace Pseudonym.Crypto.Invictus.Shared.Hosting.Models
{
    internal sealed class ExceptionHandler : IExceptionHandler
    {
        private readonly IEnvironmentNameAccessor environmentNameAccessor;

        public ExceptionHandler(IEnvironmentNameAccessor environmentNameAccessor)
        {
            this.environmentNameAccessor = environmentNameAccessor;
        }

        public async Task HandleAsync(HttpContext context, Exception e)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            var exception = e is BusinessException businessException
                ? businessException
                : new AmbiguousException("Ambiguous Error", e);

            context.Response.StatusCode = exception.GetType().Name switch
            {
                nameof(TransientException) => StatusCodes.Status424FailedDependency,
                nameof(PermanentException) => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError,
            };

            if (!context.Response.Body.CanWrite)
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;

                return;
            }

            if (environmentNameAccessor.EnvironmentName.Equals(Environments.Production) && context.Response.StatusCode >= 500)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = MediaTypeNames.Text.Plain;

                return;
            }

            context.Response.ContentType = MediaTypeNames.Application.Json;

            var body = new FailureDetails()
            {
                Title = "Request Error",
                Status = context.Response.StatusCode
            };

            body.Extensions.Add("exception", ExceptionDetail.Create(e));

            await context.Response.WriteAsync(JsonConvert.SerializeObject(body));
            await context.Response.Body.FlushAsync();
        }

        private class FailureDetails
        {
            public FailureDetails()
            {
                Title = "One or more errors occurred.";
                Status = StatusCodes.Status500InternalServerError;

                Extensions = new Dictionary<string, object>();
                Errors = new Dictionary<string, IReadOnlyList<string>>();
            }

            public FailureDetails(ActionContext context)
                : this()
            {
                Title = "One or more validation errors occurred.";
                Detail = "The inputs supplied to the API are invalid";
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5";
                Status = StatusCodes.Status400BadRequest;

                Extensions.Add("traceId", context.HttpContext.TraceIdentifier);
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

        private class ExceptionDetail
        {
            public string Message { get; set; }

            public List<string> StackTrace { get; set; }

            public ExceptionDetail InnerException { get; set; }

            public static ExceptionDetail Create(Exception e)
            {
                return new ExceptionDetail()
                {
                    StackTrace = e.StackTrace
                        ?.Split("\n", StringSplitOptions.RemoveEmptyEntries)
                        ?.Select(f => f.Replace("\r", string.Empty).Trim())
                        ?.ToList() ?? new List<string>(),
                    Message = e.Message,
                    InnerException = e.InnerException != null
                        ? Create(e.InnerException)
                        : null,
                };
            }
        }
    }
}
