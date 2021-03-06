﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Http;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Data.Models;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Data
{
    internal sealed class TransactionRepository : BaseRepository<DataTransaction>, ITransactionRepository
    {
        private const int PageSize = 100;
        private const string TableName = "Pseudonym-Crypto-Invictus-Transactions";
        private const string BlockNumberIndexName = "BlockNumber";
        private const string DateIndexName = "Date";
        private const string InboundIndexName = "Inbound";
        private const string OutboundIndexName = "Outbound";

        private readonly IHttpContextAccessor httpContextAccessor;

        public TransactionRepository(
            IAmazonDynamoDB amazonDynamoDB,
            IHttpContextAccessor httpContextAccessor,
            IScopedCancellationToken scopedCancellationToken)
            : base(amazonDynamoDB, scopedCancellationToken, TableName)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<DataTransaction> GetTransactionAsync(EthereumAddress contractAddress, EthereumTransactionHash hash)
        {
            var response = await DynamoDB.GetItemAsync(
                TableName,
                new Dictionary<string, AttributeValue>()
                {
                    [nameof(DataTransaction.Address)] = new AttributeValue(contractAddress),
                    [nameof(DataTransaction.Hash)] = new AttributeValue(hash)
                },
                CancellationToken);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new HttpRequestException($"Response code did not indicate success: {response.HttpStatusCode}");
            }

            if (response.Item.Any())
            {
                return Map(response.Item);
            }

            return null;
        }

        public async IAsyncEnumerable<DataTransaction> ListTransactionsAsync(
            EthereumAddress contractAddress,
            EthereumTransactionHash? startHash,
            DateTime? offset,
            DateTime from,
            DateTime to)
        {
            var lastEvaluatedKey = offset.HasValue && startHash.HasValue
                ? new Dictionary<string, AttributeValue>()
                {
                    [nameof(DataTransaction.Address)] = new AttributeValue(contractAddress),
                    [nameof(DataTransaction.Hash)] = new AttributeValue(startHash.Value),
                    [nameof(DataTransaction.ConfirmedAt)] = new AttributeValue(offset.Value.ToISO8601String())
                }
                : null;

            var response = await DynamoDB.QueryAsync(
                new QueryRequest()
                {
                    TableName = TableName,
                    ScanIndexForward = true,
                    IndexName = DateIndexName,
                    Select = Select.ALL_ATTRIBUTES,
                    KeyConditionExpression = string.Format(
                        "#{0} = :{0}Val AND #{1} >= :{2}Val",
                        nameof(DataTransaction.Address),
                        nameof(DataTransaction.ConfirmedAt),
                        nameof(from)),
                    ExpressionAttributeNames = new Dictionary<string, string>()
                    {
                        [$"#{nameof(DataTransaction.Address)}"] = nameof(DataTransaction.Address),
                        [$"#{nameof(DataTransaction.ConfirmedAt)}"] = nameof(DataTransaction.ConfirmedAt),
                    },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                    {
                        [$":{nameof(DataTransaction.Address)}Val"] = new AttributeValue(contractAddress),
                        [$":{nameof(from)}Val"] = new AttributeValue { S = from.ToISO8601String() }
                    },
                    Limit = PageSize,
                    ExclusiveStartKey = lastEvaluatedKey
                },
                CancellationToken);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new HttpRequestException($"Response code did not indicate success: {response.HttpStatusCode}");
            }
            else if (
                response.LastEvaluatedKey.Any() &&
                DateTime.Parse(response.LastEvaluatedKey[nameof(DataTransaction.ConfirmedAt)].S) <= to)
            {
                httpContextAccessor.HttpContext.Response.Headers.Add(
                    Headers.PaginationId,
                    response.LastEvaluatedKey[nameof(DataTransaction.Hash)].S);

                httpContextAccessor.HttpContext.Response.Headers.Add(
                    Headers.PaginationOffset,
                    response.LastEvaluatedKey[nameof(DataTransaction.ConfirmedAt)].S);
            }

            foreach (var transaction in response.Items
                .Select(Map)
                .Where(t => t.ConfirmedAt <= to)
                .OrderBy(t => t.ConfirmedAt))
            {
                yield return transaction;
            }
        }

        public Task<long> GetLatestBlockNumberAsync(EthereumAddress address)
        {
            return GetBlockNumberAsync(address, true);
        }

        public Task<long> GetLowestBlockNumberAsync(EthereumAddress address)
        {
            return GetBlockNumberAsync(address, false);
        }

        public Task<DateTime?> GetLatestDateAsync(EthereumAddress address)
        {
            return GetDateAsync(address, true);
        }

        public Task<DateTime?> GetLowestDateAsync(EthereumAddress address)
        {
            return GetDateAsync(address, false);
        }

        public IAsyncEnumerable<DataTransaction> ListInboundTransactionsAsync(
            EthereumAddress contractAddress,
            EthereumAddress address,
            EthereumAddress? filterAddress = null,
            DateTime? from = null,
            DateTime? to = null)
        {
            return ListAddressTransactionsAsync(contractAddress, address, false, filterAddress, from, to);
        }

        public IAsyncEnumerable<DataTransaction> ListOutboundTransactionsAsync(
            EthereumAddress contractAddress,
            EthereumAddress address,
            EthereumAddress? filterAddress = null,
            DateTime? from = null,
            DateTime? to = null)
        {
            return ListAddressTransactionsAsync(contractAddress, address, true, filterAddress, from, to);
        }

        protected sealed override DataTransaction Map(Dictionary<string, AttributeValue> attributes)
        {
            return new DataTransaction()
            {
                Address = attributes[nameof(DataTransaction.Address)].S,
                Hash = attributes[nameof(DataTransaction.Hash)].S,
                Success = attributes[nameof(DataTransaction.Success)].BOOL,
                BlockNumber = long.Parse(attributes[nameof(DataTransaction.BlockNumber)].N),
                Sender = attributes[nameof(DataTransaction.Sender)].S,
                Recipient = attributes[nameof(DataTransaction.Recipient)].S,
                Input = attributes[nameof(DataTransaction.Input)].S,
                ConfirmedAt = DateTimeOffset.Parse(attributes[nameof(DataTransaction.ConfirmedAt)].S, styles: DateTimeStyles.AssumeUniversal).UtcDateTime,
                Confirmations = long.Parse(attributes[nameof(DataTransaction.Confirmations)].N),
                Eth = decimal.Parse(attributes[nameof(DataTransaction.Eth)].N),
                GasLimit = long.Parse(attributes[nameof(DataTransaction.GasLimit)].N),
                Gas = long.Parse(attributes[nameof(DataTransaction.Gas)].N),
            };
        }

        private async IAsyncEnumerable<DataTransaction> ListAddressTransactionsAsync(
            EthereumAddress contractAddress,
            EthereumAddress address,
            bool outbound,
            EthereumAddress? filterAddress = null,
            DateTime? from = null,
            DateTime? to = null)
        {
            var lastEvaluatedKey = new Dictionary<string, AttributeValue>();
            var attributeName = outbound
                ? nameof(DataTransaction.Sender)
                : nameof(DataTransaction.Recipient);
            var filterAttributeName = outbound
                ? nameof(DataTransaction.Recipient)
                : nameof(DataTransaction.Sender);

            while (!CancellationToken.IsCancellationRequested)
            {
                var request = new QueryRequest()
                {
                    TableName = TableName,
                    IndexName = outbound
                        ? OutboundIndexName
                        : InboundIndexName,
                    Select = Select.ALL_ATTRIBUTES,
                    KeyConditionExpression = string.Format(
                        "#{0} = :{0}Val AND #{1} = :{1}Val",
                        attributeName,
                        nameof(DataTransaction.Address)),
                    ExpressionAttributeNames = new Dictionary<string, string>()
                    {
                        [$"#{attributeName}"] = attributeName,
                        [$"#{nameof(DataTransaction.Address)}"] = nameof(DataTransaction.Address),
                    },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                    {
                        [$":{attributeName}Val"] = new AttributeValue(address),
                        [$":{nameof(DataTransaction.Address)}Val"] = new AttributeValue(contractAddress)
                    },
                    Limit = PageSize,
                    ExclusiveStartKey = lastEvaluatedKey.Any()
                        ? lastEvaluatedKey
                        : null
                };

                var response = await DynamoDB.QueryAsync(request, CancellationToken);
                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    throw new HttpRequestException($"Response code did not indicate success: {response.HttpStatusCode}");
                }
                else
                {
                    lastEvaluatedKey = response.LastEvaluatedKey;
                }

                foreach (var attributes in response.Items
                    .Where(x =>
                        !filterAddress.HasValue ||
                        filterAddress.Value.Address.Equals(x[filterAttributeName].S, StringComparison.OrdinalIgnoreCase)))
                {
                    var transaction = Map(attributes);

                    if (!from.HasValue ||
                        !to.HasValue ||
                        (from.HasValue && to.HasValue &&
                         from <= transaction.ConfirmedAt &&
                         to >= transaction.ConfirmedAt))
                    {
                        yield return transaction;
                    }
                }

                if (!lastEvaluatedKey.Any())
                {
                    break;
                }
            }
        }

        private async Task<long> GetBlockNumberAsync(EthereumAddress address, bool latest)
        {
            var response = await DynamoDB.QueryAsync(
                new QueryRequest()
                {
                    TableName = TableName,
                    IndexName = BlockNumberIndexName,
                    ScanIndexForward = !latest,
                    Select = Select.ALL_PROJECTED_ATTRIBUTES,
                    KeyConditionExpression = $"#{nameof(DataTransaction.Address)} = :{nameof(DataTransaction.Address)}Val",
                    ExpressionAttributeNames = new Dictionary<string, string>()
                    {
                        [$"#{nameof(DataTransaction.Address)}"] = nameof(DataTransaction.Address),
                    },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                    {
                        [$":{nameof(DataTransaction.Address)}Val"] = new AttributeValue(address.Address)
                    },
                    Limit = 1
                },
                CancellationToken);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new HttpRequestException($"Response code did not indicate success: {response.HttpStatusCode}");
            }

            if (response.Items.Count == 1 &&
                response.Items.Single().ContainsKey(nameof(DataTransaction.BlockNumber)) &&
                long.TryParse(response.Items.Single()[nameof(DataTransaction.BlockNumber)].N, out long blockNumber))
            {
                return blockNumber;
            }

            return default;
        }

        private async Task<DateTime?> GetDateAsync(EthereumAddress address, bool latest)
        {
            var response = await DynamoDB.QueryAsync(
                new QueryRequest()
                {
                    TableName = TableName,
                    IndexName = DateIndexName,
                    ScanIndexForward = !latest,
                    Select = Select.SPECIFIC_ATTRIBUTES,
                    ProjectionExpression = $"{nameof(DataTransaction.Address)},{nameof(DataTransaction.ConfirmedAt)}",
                    KeyConditionExpression = $"#{nameof(DataTransaction.Address)} = :{nameof(DataTransaction.Address)}Val",
                    ExpressionAttributeNames = new Dictionary<string, string>()
                    {
                        [$"#{nameof(DataTransaction.Address)}"] = nameof(DataTransaction.Address),
                    },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                    {
                        [$":{nameof(DataTransaction.Address)}Val"] = new AttributeValue(address)
                    },
                    Limit = 1
                },
                CancellationToken);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new HttpRequestException($"Response code did not indicate success: {response.HttpStatusCode}");
            }

            if (response.Items.Count == 1 &&
                response.Items.Single().ContainsKey(nameof(DataTransaction.ConfirmedAt)) &&
                DateTime.TryParse(response.Items.Single()[nameof(DataTransaction.ConfirmedAt)].S, out DateTime date))
            {
                return date;
            }

            return null;
        }
    }
}
