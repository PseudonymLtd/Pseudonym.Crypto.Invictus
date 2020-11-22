namespace Pseudonym.Crypto.Invictus.Funds.Configuration
{
    public class Dependencies
    {
        public Dependency<EthplorerDependencySettings> Ethplorer { get; set; }

        public Dependency<DependencySettings> Invictus { get; set; }

        public Dependency<InfuriaDependencySettings> Infuria { get; set; }

        public Dependency<DependencySettings> ExchangeRate { get; set; }
    }
}
