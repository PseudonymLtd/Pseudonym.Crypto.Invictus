using System;
using System.Collections.Generic;

namespace Pseudonym.Crypto.Invictus.Funds.Configuration
{
    public class AppSettings
    {
        public string ApiKey { get; set; }

        public string ServiceName { get; set; }

        public bool CacheTransactions { get; set; }

        public bool CacheStakePower { get; set; }

        public bool CacheFundPerformance { get; set; }

        public string Version { get; set; }

        public Uri HostUrl { get; set; }

        public string JwtSecret { get; set; }

        public string JwtIssuer => HostUrl.OriginalString;

        public string JwtAudience { get; set; }

        public TimeSpan JwtTimeout { get; set; }

        public string StakingAddress { get; set; }

        public List<StakeSettings> Stakes { get; set; } = new List<StakeSettings>();

        public List<FundSettings> Funds { get; set; } = new List<FundSettings>();

        public List<AssetSettings> Assets { get; set; } = new List<AssetSettings>();

        public List<HoldingSettings> Holdings { get; set; } = new List<HoldingSettings>();
    }
}
