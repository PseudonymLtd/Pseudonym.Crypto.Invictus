using System;
using System.Collections.Generic;

namespace Pseudonym.Crypto.Invictus.TrackerService.Configuration
{
    public class AppSettings
    {
        public string EtherscanApiKey { get; set; }

        public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(1);

        public List<FundConfig> Funds { get; set; } = new List<FundConfig>();
    }
}
