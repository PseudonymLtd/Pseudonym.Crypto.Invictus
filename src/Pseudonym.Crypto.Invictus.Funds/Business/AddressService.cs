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
        private readonly ITransactionService transactionService;
        private readonly IScopedCancellationToken scopedCancellationToken;

        public AddressService(
            IFundService fundService,
            IEtherClient etherClient,
            ITransactionService transactionService,
            IScopedCancellationToken scopedCancellationToken)
        {
            this.fundService = fundService;
            this.etherClient = etherClient;
            this.transactionService = transactionService;
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

        public async Task<IInvestment> GetInvestmentAsync(EthereumAddress address, Symbol symbol, CurrencyCode currencyCode)
        {
            var fund = await fundService.GetFundAsync(symbol, currencyCode);
            var tokenCount = await etherClient.GetContractBalanceAsync(fund.Token.ContractAddress, address);

            return new BusinessInvestment()
            {
                Fund = fund,
                Held = tokenCount
            };
        }

        public IReadOnlyList<ITransaction> ListTransactions(EthereumAddress contractAddress, EthereumAddress address)
        {
            return transactionService.GetAddressTransactions(contractAddress, address);
        }
    }
}
