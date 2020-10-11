﻿namespace Pseudonym.Crypto.Invictus.TrackerService.Configuration
{
    public class Dependencies
    {
        public Dependency<DependencySettings> Invictus { get; set; }

        public Dependency<InfuriaDependencySettings> Infuria { get; set; }

        public Dependency<DependencySettings> ExchangeRate { get; set; }
    }
}
