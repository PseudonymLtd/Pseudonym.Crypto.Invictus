﻿using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Nethereum.Web3;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Business.Models;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Funds.Data.Models;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Shared.Models;

namespace Pseudonym.Crypto.Invictus.Funds.Business
{
    internal abstract class AbstractService
    {
        private readonly AppSettings appSettings;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IScopedCancellationToken scopedCancellationToken;

        public AbstractService(
            IOptions<AppSettings> appSettings,
            ICurrencyConverter currencyConverter,
            ITransactionRepository transactionRepository,
            IOperationRepository operationRepository,
            IHttpContextAccessor httpContextAccessor,
            IScopedCancellationToken scopedCancellationToken)
        {
            this.appSettings = appSettings.Value;
            this.httpContextAccessor = httpContextAccessor;
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
            var fundSettings = appSettings.Funds.Single(x => x.Symbol == symbol);

            if (httpContextAccessor.HttpContext.Response.Headers.ContainsKey(Headers.Contract))
            {
                httpContextAccessor.HttpContext.Response.Headers[Headers.Contract] = fundSettings.ContractAddress;
            }
            else
            {
                httpContextAccessor.HttpContext.Response.Headers.Add(Headers.Contract, fundSettings.ContractAddress);
            }

            return fundSettings;
        }

        protected TBusinessTransaction MapTransaction<TBusinessTransaction>(DataTransaction transaction)
            where TBusinessTransaction : BusinessTransaction, new()
        {
            return new TBusinessTransaction()
            {
                Address = new EthereumAddress(transaction.Address),
                Hash = new EthereumTransactionHash(transaction.Hash),
                Success = transaction.Success,
                BlockNumber = transaction.BlockNumber,
                Sender = new EthereumAddress(transaction.Sender),
                Recipient = new EthereumAddress(transaction.Recipient),
                ConfirmedAt = transaction.ConfirmedAt,
                Confirmations = transaction.Confirmations,
                Eth = transaction.Eth,
                Gas = transaction.Gas,
                GasLimit = transaction.GasLimit
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
