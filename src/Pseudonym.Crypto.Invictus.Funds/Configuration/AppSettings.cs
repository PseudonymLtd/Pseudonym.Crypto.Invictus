using System;
using System.Collections.Generic;

namespace Pseudonym.Crypto.Invictus.Funds.Configuration
{
    public class AppSettings
    {
        public string ApiKey { get; set; }

        public string ServiceName { get; set; }

        public Version Version { get; set; }

        public Uri HostUrl { get; set; }

        public List<FundSettings> Funds { get; set; } = new List<FundSettings>();
    }
}
