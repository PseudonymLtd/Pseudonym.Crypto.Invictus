using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;

namespace Pseudonym.Crypto.Invictus.Web.Client.Hosting
{
    internal sealed class EnvironmentNameAccessor : IEnvironmentNameAccessor
    {
        public EnvironmentNameAccessor(IWebAssemblyHostEnvironment webHostEnvironment)
        {
            EnvironmentName = webHostEnvironment.Environment;
        }

        public string EnvironmentName { get; }
    }
}
