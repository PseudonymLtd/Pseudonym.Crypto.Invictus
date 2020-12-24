using System;
using System.Collections.Generic;

namespace Pseudonym.Crypto.Invictus.Funds.Configuration
{
    public class AppSettings
    {
        public string ApiKey { get; set; }

        public string ServiceName { get; set; }

        public bool CachingEnabled { get; set; }

        public string Version { get; set; }

        public Uri HostUrl { get; set; }

        public string JwtSecret { get; set; }

        public string JwtIssuer => HostUrl.OriginalString;

        public string JwtAudience { get; set; }

        public TimeSpan JwtTimeout { get; set; }

        public string StakingAddress { get; set; }

        public List<FundSettings> Funds { get; set; } = new List<FundSettings>();

        public List<AssetSettings> Assets { get; set; } = new List<AssetSettings>();
    }
}
