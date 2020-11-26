using System;
using System.Collections.Generic;
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
    internal sealed class TransactionRepository : ITransactionRepository
    {
        private const string TableName = "Pseudonym-Crypto-Invictus-Transactions";
        private const string IndexName = "BlockNumber";

        private readonly IAmazonDynamoDB amazonDynamoDB;
        private readonly IScopedCancellationToken scopedCancellationToken;

        public TransactionRepository(
            IAmazonDynamoDB amazonDynamoDB,
            IScopedCancellationToken scopedCancellationToken)
        {
            this.amazonDynamoDB = amazonDynamoDB;
            this.scopedCancellationToken = scopedCancellationToken;
        }

        public async Task<DataTransaction> GetTransactionsAsync(EthereumAddress contractAddress, EthereumTransactionHash hash)
        {
            var response = await amazonDynamoDB.GetItemAsync(
                TableName,
                new Dictionary<string, AttributeValue>()
                {
                    [nameof(DataTransaction.Address)] = new AttributeValue(contractAddress.Address),
                    [nameof(DataTransaction.Hash)] = new AttributeValue(hash.Hash),
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

        public async IAsyncEnumerable<DataTransaction> ListTransactionsAsync(EthereumAddress contractAddress)
        {
            var response = await amazonDynamoDB.QueryAsync(
                new QueryRequest()
                {
                    TableName = TableName,
                    ScanIndexForward = true,
                    Select = Select.ALL_ATTRIBUTES,
                    KeyConditionExpression = $"#{nameof(DataTransaction.Address)} = :{nameof(DataTransaction.Address)}Val",
                    ExpressionAttributeNames = new Dictionary<string, string>()
                    {
                        [$"#{nameof(DataTransaction.Address)}"] = nameof(DataTransaction.Address),
                    },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                    {
                        [$":{nameof(DataTransaction.Address)}Val"] = new AttributeValue { S = contractAddress.Address }
                    }
                },
                scopedCancellationToken.Token);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new HttpRequestException($"Response code did not indicate success: {response.HttpStatusCode}");
            }

            foreach (var transaction in response.Items)
            {
                yield return Map(transaction);
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

        private async Task<long> GetBlockNumberAsync(EthereumAddress address, bool latest)
        {
            var response = await amazonDynamoDB.QueryAsync(
                new QueryRequest()
                {
                    TableName = TableName,
                    IndexName = IndexName,
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
                GasPrice = long.Parse(attributes[nameof(DataTransaction.GasPrice)].N),
                GasLimit = long.Parse(attributes[nameof(DataTransaction.GasLimit)].N),
                Gas = long.Parse(attributes[nameof(DataTransaction.Gas)].N),
                Input = attributes[nameof(DataTransaction.Input)].S
            };
        }
    }
}
