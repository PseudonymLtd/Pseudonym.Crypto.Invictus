using Microsoft.AspNetCore.Hosting;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;

namespace Pseudonym.Crypto.Invictus.Web.Server.Hosting
{
    internal sealed class EnvironmentNameAccessor : IEnvironmentNameAccessor
    {
        public EnvironmentNameAccessor(IWebHostEnvironment webHostEnvironment)
        {
            EnvironmentName = webHostEnvironment.EnvironmentName;
        }

        public string EnvironmentName { get; }
    }
}
