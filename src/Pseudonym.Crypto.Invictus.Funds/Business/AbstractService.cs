﻿using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using Microsoft.Extensions.Options;
using Nethereum.Web3;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Business.Models;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Funds.Data.Models;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Shared.Models;

namespace Pseudonym.Crypto.Invictus.Funds.Business
{
    internal abstract class AbstractService
    {
        private readonly AppSettings appSettings;
        private readonly IScopedCancellationToken scopedCancellationToken;

        public AbstractService(
            IOptions<AppSettings> appSettings,
            ICurrencyConverter currencyConverter,
            ITransactionRepository transactionRepository,
            IOperationRepository operationRepository,
            IScopedCancellationToken scopedCancellationToken)
        {
            this.appSettings = appSettings.Value;
            this.scopedCancellationToken = scopedCancellationToken;

            CurrencyConverter = currencyConverter;
            Transactions = transactionRepository;
            Operations = operationRepository;
        }

        protected ICurrencyConverter CurrencyConverter { get; }

        protected ITransactionRepository Transactions { get; }

        protected IOperationRepository Operations { get; }

        protected CancellationToken CancellationToken => scopedCancellationToken.Token;

        protected FundSettings GetFundInfo(Symbol symbol)
        {
            return appSettings.Funds.Single(x => x.Symbol == symbol);
        }

        protected TBusinessTransaction MapTransaction<TBusinessTransaction>(DataTransaction transaction)
            where TBusinessTransaction : BusinessTransaction, new()
        {
            return new TBusinessTransaction()
            {
                Address = new EthereumAddress(transaction.Address),
                Hash = new EthereumTransactionHash(transaction.Hash),
                BlockHash = new EthereumTransactionHash(transaction.BlockHash),
                Nonce = transaction.Nonce,
                Success = transaction.Success,
                BlockNumber = transaction.BlockNumber,
                Sender = new EthereumAddress(transaction.Sender),
                Recipient = new EthereumAddress(transaction.Recipient),
                ConfirmedAt = transaction.ConfirmedAt,
                Confirmations = transaction.Confirmations,
                Eth = transaction.Eth,
                Gas = transaction.Gas,
                GasPrice = transaction.GasPrice,
                GasLimit = transaction.GasLimit,
                Input = transaction.Input
            };
        }

        protected BusinessOperation MapOperation(DataOperation operation, CurrencyCode currencyCode)
        {
            return new BusinessOperation()
            {
                Hash = new EthereumTransactionHash(operation.Hash),
                Order = operation.Order,
                Type = operation.Type,
                Address = string.IsNullOrEmpty(operation.Address)
                    ? default(EthereumAddress?)
                    : new EthereumAddress(operation.Address),
                Sender = string.IsNullOrEmpty(operation.Sender)
                    ? default(EthereumAddress?)
                    : new EthereumAddress(operation.Sender),
                Recipient = string.IsNullOrEmpty(operation.Recipient)
                    ? default(EthereumAddress?)
                    : new EthereumAddress(operation.Recipient),
                Addresses = operation.Addresses
                    .Select(a => new EthereumAddress(a))
                    .ToList(),
                IsEth = operation.IsEth,
                PricePerToken = CurrencyConverter.Convert(operation.Price, currencyCode),
                Quantity = operation.Type == OperationTypes.Transfer
                    ? Web3.Convert.FromWei(BigInteger.Parse(operation.Value))
                    : 0,
                Value = operation.Value,
                Priority = operation.Priority,
                ContractAddress = new EthereumAddress(operation.ContractAddress),
                ContractSymbol = operation.ContractSymbol,
                ContractDecimals = operation.ContractDecimals,
                ContractHolders = operation.ContractHolders,
                ContractIssuances = operation.ContractIssuances,
                ContractName = operation.ContractName,
                ContractLink = string.IsNullOrEmpty(operation.ContractLink)
                    ? null
                    : new Uri(operation.ContractLink, UriKind.Absolute),
            };
        }
    }
}
