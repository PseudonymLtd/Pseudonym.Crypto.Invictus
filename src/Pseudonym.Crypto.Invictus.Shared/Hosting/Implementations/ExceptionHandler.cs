using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
    }
}
