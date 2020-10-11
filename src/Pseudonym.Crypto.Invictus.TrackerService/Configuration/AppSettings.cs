using System;
using System.Collections.Generic;

namespace Pseudonym.Crypto.Invictus.TrackerService.Configuration
{
    public class AppSettings
    {
        public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(1);

        public Version Version { get; set; }

        public InfuriaConfig Infuria { get; set; }

        public List<FundConfig> Funds { get; set; } = new List<FundConfig>();
    }
}
