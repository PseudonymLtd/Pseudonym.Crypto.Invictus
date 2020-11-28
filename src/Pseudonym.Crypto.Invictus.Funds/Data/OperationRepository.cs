﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Data.Models;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Funds.Utils;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Data
{
    internal sealed class OperationRepository : IOperationRepository
    {
        private const string TableName = "Pseudonym-Crypto-Invictus-Operations";
        private const string InboundIndexName = "Inbound";
        private const string OutboundIndexName = "Outbound";

        private readonly IAmazonDynamoDB amazonDynamoDB;
        private readonly IScopedCancellationToken scopedCancellationToken;

        public OperationRepository(
            IAmazonDynamoDB amazonDynamoDB,
            IScopedCancellationToken scopedCancellationToken)
        {
            this.amazonDynamoDB = amazonDynamoDB;
            this.scopedCancellationToken = scopedCancellationToken;
        }

        public async Task<DataOperation> GetOperationAsync(EthereumTransactionHash hash, int order)
        {
            var response = await amazonDynamoDB.GetItemAsync(
                TableName,
                new Dictionary<string, AttributeValue>()
                {
                    [nameof(DataOperation.Hash)] = new AttributeValue(hash),
                    [nameof(DataOperation.Order)] = new AttributeValue() { N = order.ToString() }
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

        public async IAsyncEnumerable<DataOperation> ListOperationsAsync(EthereumTransactionHash hash)
        {
            var lastEvaluatedKey = new Dictionary<string, AttributeValue>();

            while (!scopedCancellationToken.Token.IsCancellationRequested)
            {
                var response = await amazonDynamoDB.QueryAsync(
                    new QueryRequest()
                    {
                        TableName = TableName,
                        Select = Select.ALL_ATTRIBUTES,
                        KeyConditionExpression = string.Format("#{0} = :{0}Val", nameof(DataOperation.Hash)),
                        ExpressionAttributeNames = new Dictionary<string, string>()
                        {
                            [$"#{nameof(DataOperation.Hash)}"] = nameof(DataOperation.Hash),
                        },
                        ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                        {
                            [$":{nameof(DataOperation.Hash)}Val"] = new AttributeValue { S = hash },
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

        public async Task UploadOperationAsync(DataOperation operation)
        {
            var attributes = DynamoDbConvert.Serialize(operation);

            var response = await amazonDynamoDB.PutItemAsync(
                TableName,
                attributes,
                scopedCancellationToken.Token);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new HttpRequestException($"Response code did not indicate success: {response.HttpStatusCode}");
            }
        }

        public IAsyncEnumerable<EthereumTransactionHash> ListInboundHashesAsync(EthereumAddress address, string type)
        {
            return ListAddressHashesAsync(address, type, InboundIndexName, nameof(DataOperation.Recipient));
        }

        public IAsyncEnumerable<EthereumTransactionHash> ListOutboundHashesAsync(EthereumAddress address, string type)
        {
            return ListAddressHashesAsync(address, type, OutboundIndexName, nameof(DataOperation.Sender));
        }

        private async IAsyncEnumerable<EthereumTransactionHash> ListAddressHashesAsync(
            EthereumAddress address,
            string type,
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
                        Select = Select.ALL_PROJECTED_ATTRIBUTES,
                        KeyConditionExpression = string.Format(
                            "#{0} = :{0}Val AND #{1} = :{1}Val",
                            attributeName,
                            nameof(DataOperation.Type)),
                        ExpressionAttributeNames = new Dictionary<string, string>()
                        {
                            [$"#{attributeName}"] = attributeName,
                            [$"#{nameof(DataOperation.Type)}"] = nameof(DataOperation.Type)
                        },
                        ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                        {
                            [$":{attributeName}Val"] = new AttributeValue { S = address },
                            [$":{nameof(DataOperation.Type)}Val"] = new AttributeValue { S = type }
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
                    if (attributes.ContainsKey(nameof(DataOperation.Hash)))
                    {
                        yield return new EthereumTransactionHash(attributes[nameof(DataOperation.Hash)].S);
                    }
                }

                if (!lastEvaluatedKey.Any())
                {
                    break;
                }
            }
        }

        private DataOperation Map(Dictionary<string, AttributeValue> attributes)
        {
            return new DataOperation()
            {
                Hash = attributes[nameof(DataOperation.Hash)].S,
                Order = int.Parse(attributes[nameof(DataOperation.Order)].N),
                Type = attributes[nameof(DataOperation.Type)].S,
                Address = attributes[nameof(DataOperation.Address)].S,
                Sender = attributes[nameof(DataOperation.Sender)].S,
                Recipient = attributes[nameof(DataOperation.Recipient)].S,
                Addresses = attributes[nameof(DataOperation.Addresses)].SS,
                IsEth = attributes[nameof(DataOperation.IsEth)].BOOL,
                Price = decimal.Parse(attributes[nameof(DataOperation.Price)].N),
                Value = attributes[nameof(DataOperation.Value)].S,
                Priority = int.Parse(attributes[nameof(DataOperation.Priority)].N),
                ContractAddress = attributes[nameof(DataOperation.ContractAddress)].S,
                ContractSymbol = attributes[nameof(DataOperation.ContractSymbol)].S,
                ContractDecimals = int.Parse(attributes[nameof(DataOperation.ContractDecimals)].N),
                ContractHolders = long.Parse(attributes[nameof(DataOperation.ContractHolders)].N),
                ContractIssuances = long.Parse(attributes[nameof(DataOperation.ContractIssuances)].N),
                ContractLink = attributes[nameof(DataOperation.ContractLink)].S,
                ContractName = attributes[nameof(DataOperation.ContractName)].S
            };
        }
    }
}