using System.Collections.Generic;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models.Invictus
{
    public interface IInvictusFund
    {
        string Name { get; }

        Symbol Symbol { get; }

        string CirculatingSupply { get; }

        string NetValue { get; }

        string NetAssetValuePerToken { get; }

        IReadOnlyList<InvictusAsset> Assets { get; }
    }
}
