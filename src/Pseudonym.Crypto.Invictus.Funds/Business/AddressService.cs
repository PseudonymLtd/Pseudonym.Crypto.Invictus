using System.Collections.Generic;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Business.Models;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Funds.Business
{
    internal sealed class AddressService : IAddressService
    {
        private readonly IFundService fundService;
        private readonly IEtherClient etherClient;
        private readonly IScopedCancellationToken scopedCancellationToken;

        public AddressService(
            IFundService fundService,
            IEtherClient etherClient,
            IScopedCancellationToken scopedCancellationToken)
        {
            this.fundService = fundService;
            this.etherClient = etherClient;
            this.scopedCancellationToken = scopedCancellationToken;
        }

        public async IAsyncEnumerable<IInvestment> ListInvestmentsAsync(EthereumAddress address, CurrencyCode currencyCode)
        {
            await foreach (var fund in fundService
                .ListFundsAsync(currencyCode)
                .WithCancellation(scopedCancellationToken.Token))
            {
                var tokenCount = await etherClient.GetContractBalanceAsync(fund.Token.ContractAddress, address);
                if (tokenCount > 0)
                {
                    yield return new BusinessInvestment()
                    {
                        Fund = fund,
                        Held = tokenCount
                    };
                }
            }
        }

        public async IAsyncEnumerable<ITransaction> ListTransactionsAsync(EthereumAddress contractAddress, EthereumAddress address, CurrencyCode currencyCode)
        {
            await foreach (var transaction in etherClient
                .ListContractTransactionsAsync(contractAddress, address)
                .WithCancellation(scopedCancellationToken.Token))
            {
                yield return new BusinessTransaction()
                {
                    Sender = transaction.Sender,
                    Recipient = transaction.Recipient,
                    Amount = transaction.Amount
                };
            }
        }
    }
}
