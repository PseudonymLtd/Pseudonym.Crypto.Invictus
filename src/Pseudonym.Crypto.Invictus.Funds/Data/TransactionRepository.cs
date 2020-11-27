﻿using System;
using System.Collections.Generic;
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
using Pseudonym.Crypto.Invictus.Funds.Utils;
using Pseudonym.Crypto.Invictus.Shared;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Data
{
    internal sealed class TransactionRepository : ITransactionRepository
    {
        private const int PageSize = 100;
        private const string TableName = "Pseudonym-Crypto-Invictus-Transactions";
        private const string BlockNumberIndexName = "BlockNumber";
        private const string DateIndexName = "Date";
        private const string InboundIndexName = "Inbound";
        private const string OutboundIndexName = "Outbound";

        private readonly IAmazonDynamoDB amazonDynamoDB;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IScopedCancellationToken scopedCancellationToken;

        public TransactionRepository(
            IAmazonDynamoDB amazonDynamoDB,
            IHttpContextAccessor httpContextAccessor,
            IScopedCancellationToken scopedCancellationToken)
        {
            this.amazonDynamoDB = amazonDynamoDB;
            this.httpContextAccessor = httpContextAccessor;
            this.scopedCancellationToken = scopedCancellationToken;
        }

        public async Task<DataTransaction> GetTransactionAsync(EthereumAddress contractAddress, EthereumTransactionHash hash)
        {
            var response = await amazonDynamoDB.GetItemAsync(
                TableName,
                new Dictionary<string, AttributeValue>()
                {
                    [nameof(DataTransaction.Address)] = new AttributeValue(contractAddress),
                    [nameof(DataTransaction.Hash)] = new AttributeValue(hash)
                },
                scopedCancellationToken.Token);

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

            var response = await amazonDynamoDB.QueryAsync(
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
                        [$":{nameof(DataTransaction.Address)}Val"] = new AttributeValue { S = contractAddress },
                        [$":{nameof(from)}Val"] = new AttributeValue { S = from.ToISO8601String() }
                    },
                    Limit = PageSize,
                    ExclusiveStartKey = lastEvaluatedKey
                },
                scopedCancellationToken.Token);

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

        public async Task UploadTransactionAsync(DataTransaction transaction)
        {
            var attributes = DynamoDbConvert.Serialize(transaction);

            var response = await amazonDynamoDB.PutItemAsync(
                TableName,
                attributes,
                scopedCancellationToken.Token);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new HttpRequestException($"Response code did not indicate success: {response.HttpStatusCode}");
            }
        }

        public IAsyncEnumerable<DataTransaction> ListInboundTransactionsAsync(EthereumAddress contractAddress, EthereumAddress address)
        {
            return ListAddressTransactionsAsync(contractAddress, address, InboundIndexName, nameof(DataTransaction.Recipient));
        }

        public IAsyncEnumerable<DataTransaction> ListOutboundTransactionsAsync(EthereumAddress contractAddress, EthereumAddress address)
        {
            return ListAddressTransactionsAsync(contractAddress, address, OutboundIndexName, nameof(DataTransaction.Sender));
        }

        private async IAsyncEnumerable<DataTransaction> ListAddressTransactionsAsync(
            EthereumAddress contractAddress,
            EthereumAddress address,
            string indexName,
            string attributeName)
        {
            var lastEvaluatedKey = new Dictionary<string, AttributeValue>();

            while (!scopedCancellationToken.Token.IsCancellationRequested)
            {
                var response = await amazonDynamoDB.QueryAsync(
                    new QueryRequest()
                    {
                        TableName = TableName,
                        IndexName = indexName,
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
                            [$":{attributeName}Val"] = new AttributeValue { S = address },
                            [$":{nameof(DataTransaction.Address)}Val"] = new AttributeValue { S = contractAddress }
                        },
                        ExclusiveStartKey = lastEvaluatedKey.Any()
                            ? lastEvaluatedKey
                            : null
                    },
                    scopedCancellationToken.Token);

                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    throw new HttpRequestException($"Response code did not indicate success: {response.HttpStatusCode}");
                }
                else
                {
                    lastEvaluatedKey = response.LastEvaluatedKey;
                }

                foreach (var attributes in response.Items)
                {
                    yield return Map(attributes);
                }

                if (!lastEvaluatedKey.Any())
                {
                    break;
                }
            }
        }

        private async Task<long> GetBlockNumberAsync(EthereumAddress address, bool latest)
        {
            var response = await amazonDynamoDB.QueryAsync(
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
                        [$":{nameof(DataTransaction.Address)}Val"] = new AttributeValue { S = address.Address }
                    },
                    Limit = 1
                },
                scopedCancellationToken.Token);

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

        private DataTransaction Map(Dictionary<string, AttributeValue> attributes)
        {
            return new DataTransaction()
            {
                Address = attributes[nameof(DataTransaction.Address)].S,
                Hash = attributes[nameof(DataTransaction.Hash)].S,
                BlockHash = attributes[nameof(DataTransaction.BlockHash)].S,
                Nonce = long.Parse(attributes[nameof(DataTransaction.Nonce)].N),
                Success = attributes[nameof(DataTransaction.Success)].BOOL,
                BlockNumber = long.Parse(attributes[nameof(DataTransaction.BlockNumber)].N),
                Sender = attributes[nameof(DataTransaction.Sender)].S,
                Recipient = attributes[nameof(DataTransaction.Recipient)].S,
                ConfirmedAt = DateTime.Parse(attributes[nameof(DataTransaction.ConfirmedAt)].S),
                Confirmations = long.Parse(attributes[nameof(DataTransaction.Confirmations)].N),
                Eth = decimal.Parse(attributes[nameof(DataTransaction.Eth)].N),
                GasPrice = decimal.Parse(attributes[nameof(DataTransaction.GasPrice)].N),
                GasLimit = long.Parse(attributes[nameof(DataTransaction.GasLimit)].N),
                Gas = long.Parse(attributes[nameof(DataTransaction.Gas)].N),
                Input = attributes[nameof(DataTransaction.Input)].S
            };
        }
    }
}
