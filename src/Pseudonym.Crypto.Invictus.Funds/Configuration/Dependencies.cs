namespace Pseudonym.Crypto.Invictus.Funds.Configuration
{
    public class Dependencies
    {
        public Dependency<ApiKeyDependencySettings> Bloxy { get; set; }

        public Dependency<ApiKeyDependencySettings> Ethplorer { get; set; }

        public Dependency<DependencySettings> Invictus { get; set; }

        public Dependency<InfuraDependencySettings> Infura { get; set; }

        public Dependency<DependencySettings> ExchangeRate { get; set; }
    }
}
