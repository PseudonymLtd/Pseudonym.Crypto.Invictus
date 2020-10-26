using System;

namespace Pseudonym.Crypto.Invictus.Web.Server.Configuration
{
    public sealed class AppSettings
    {
        public string ApiKey { get; set; }

        public Uri ApiUrl { get; set; }

        public string ServiceName { get; set; }

        public Version Version { get; set; }

        public Uri HostUrl { get; set; }
    }
}
