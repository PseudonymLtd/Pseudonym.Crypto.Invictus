namespace Pseudonym.Crypto.Invictus.Shared.Models.Abstractions
{
    public interface IProduct
    {
        string Name { get; }

        string DisplayName { get; }

        string Description { get; }

        ApiToken Token { get; }

        decimal CirculatingSupply { get; }

        ApiNav Nav { get; }

        ApiMarket Market { get; }
    }
}
