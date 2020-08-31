using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Pseudonym.Crypto.Investments.Business.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Business.Abstractions;
using Pseudonym.Crypto.Invictus.TrackerService.Business.Models;
using Pseudonym.Crypto.Invictus.TrackerService.Clients.Models;
using Pseudonym.Crypto.Invictus.TrackerService.Configuration;
using Pseudonym.Crypto.Invictus.TrackerService.Models;

namespace Pseudonym.Crypto.Invictus.TrackerService.Business
{
    internal sealed class FundService : IFundService
    {
        private readonly AppSettings appSettings;
        private readonly IInvictusClient invictusClient;

        public FundService(
            IOptions<AppSettings> appSettings,
            IInvictusClient invictusClient)
        {
            this.appSettings = appSettings.Value;
            this.invictusClient = invictusClient;
        }

        public async IAsyncEnumerable<IFund> ListFundsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var fund in invictusClient.ListFundsAsync(cancellationToken))
            {
                yield return Map(fund);
            }
        }

        public async Task<IFund> GetFundAsync(string fundName, CancellationToken cancellationToken)
        {
            var fund = await invictusClient.GetFundAsync(fundName, cancellationToken);

            return Map(fund);
        }

        private IFund Map(InvictusFund fund)
        {
            var tokenInfo = appSettings.Funds.SingleOrDefault(f => f.Symbol == fund.Symbol);

            return new BusinessFund()
            {
                Name = fund.Name,
                Token = tokenInfo != null
                    ? new BusinessToken()
                    {
                        Symbol = tokenInfo.Symbol,
                        ContractAddress = new EthereumAddress(tokenInfo.ContractAddress),
                        Decimals = tokenInfo.Decimals
                    }
                    : new BusinessToken(),
                CirculatingSupply = fund.CirculatingSupply,
                NetAssetValue = fund.NetAssetValue,
                MarketValuePerToken = fund.MarketValuePerToken,
                NetAssetValuePerToken = fund.NetAssetValuePerToken,
                Assets = fund.Assets
                        .Select(a => new BusinessAsset()
                        {
                            Symbol = a.Symbol,
                            Name = a.Name,
                            Value = a.Value
                        })
                        .ToList()
            };
        }
    }
}
