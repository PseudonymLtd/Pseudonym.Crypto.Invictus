using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Business.Models;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Funds.Data.Models;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Shared.Models;

namespace Pseudonym.Crypto.Invictus.Funds.Business
{
    internal sealed class AddressService : AbstractService, IAddressService
    {
        private readonly IFundService fundService;
        private readonly IEtherClient etherClient;

        public AddressService(
            IOptions<AppSettings> appSettings,
            IFundService fundService,
            IEtherClient etherClient,
            ICurrencyConverter currencyConverter,
            ITransactionRepository transactionRepository,
            IOperationRepository operationRepository,
            IHttpContextAccessor httpContextAccessor,
            IScopedCancellationToken scopedCancellationToken)
            : base(appSettings, currencyConverter, transactionRepository, operationRepository, httpContextAccessor, scopedCancellationToken)
        {
            this.fundService = fundService;
            this.etherClient = etherClient;
        }

        public async IAsyncEnumerable<IInvestment> ListInvestmentsAsync(EthereumAddress address, CurrencyCode currencyCode)
        {
            await foreach (var fund in fundService
                .ListFundsAsync(currencyCode)
                .WithCancellation(CancellationToken))
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
            var fundInfo = GetFundInfo(symbol);
            var fund = await fundService.GetFundAsync(symbol, currencyCode);
            var tokenCount = await etherClient.GetContractBalanceAsync(fundInfo.Address, address);

            return new BusinessInvestment()
            {
                Fund = fund,
                Held = tokenCount
            };
        }

        public async IAsyncEnumerable<ITransactionSet> ListTransactionsAsync(EthereumAddress address, Symbol symbol, CurrencyCode currencyCode)
        {
            var fundInfo = GetFundInfo(symbol);

            var transactionHashes = new List<EthereumTransactionHash>();
            var transactions = new List<DataTransaction>();

            await foreach (var transaction in Transactions
                .ListInboundTransactionsAsync(fundInfo.Address, address)
                .WithCancellation(CancellationToken))
            {
                transactions.Add(transaction);
            }

            await foreach (var transaction in Transactions
                .ListOutboundTransactionsAsync(fundInfo.Address, address)
                .WithCancellation(CancellationToken))
            {
                transactions.Add(transaction);
            }

            await foreach (var hash in Operations
                .ListInboundHashesAsync(address, OperationTypes.Transfer)
                .WithCancellation(CancellationToken))
            {
                transactionHashes.Add(hash);
            }

            await foreach (var hash in Operations
                .ListOutboundHashesAsync(address, OperationTypes.Transfer)
                .WithCancellation(CancellationToken))
            {
                transactionHashes.Add(hash);
            }

            foreach (var hash in transactionHashes.Distinct())
            {
                var transaction = transactions
                    .SingleOrDefault(t => t.Address == address && t.Hash == hash)
                        ?? await Transactions.GetTransactionAsync(fundInfo.Address, hash);

                if (transaction != null &&
                    transaction.Address == fundInfo.Address)
                {
                    var businessTransaction = MapTransaction<BusinessTransactionSet>(transaction);

                    var operations = new List<IOperation>();

                    await foreach (var operation in Operations
                        .ListOperationsAsync(hash)
                        .WithCancellation(CancellationToken))
                    {
                        operations.Add(MapOperation(operation, currencyCode));
                    }

                    businessTransaction.Operations = operations;

                    yield return businessTransaction;
                }
            }
        }
    }
}
