using System;

#pragma warning disable SA1402

namespace Pseudonym.Crypto.Invictus.Funds.Configuration
{
    public sealed class Dependency<TSettings>
        where TSettings : DependencySettings, new()
    {
        public Uri Url { get; set; }

        public TSettings Settings { get; set; }
    }

    public class DependencySettings
    {
        public TimeSpan Timeout { get; set; }
    }

    public class ApiKeyDependencySettings : DependencySettings
    {
        public string ApiKey { get; set; }
    }

    public class InfuraDependencySettings : DependencySettings
    {
        public string ProjectId { get; set; }

        public string ProjectSecret { get; set; }
    }
}
