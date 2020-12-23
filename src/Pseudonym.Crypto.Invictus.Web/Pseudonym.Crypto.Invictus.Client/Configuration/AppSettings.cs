using System;

namespace Pseudonym.Crypto.Invictus.Web.Client.Configuration
{
    public sealed class AppSettings
    {
        public Uri ApiUrl { get; set; }

        public string ServiceName { get; set; }

        public string Version { get; set; }

        public string StakingAddress { get; set; }
    }
}
