using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Funds.Utils;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Services
{
    internal sealed class TransactionService : ITransactionService
    {
        private const string TableName = "Pseudonym-Crypto-Invictus-Transactions";
        private const string IndexName = "Pseudonym-Crypto-Invictus-Transactions-Block-Number";

        private readonly IAmazonDynamoDB amazonDynamoDB;
        private readonly IScopedCancellationToken scopedCancellationToken;

        public TransactionService(
            IAmazonDynamoDB amazonDynamoDB,
            IScopedCancellationToken scopedCancellationToken)
        {
            this.amazonDynamoDB = amazonDynamoDB;
            this.scopedCancellationToken = scopedCancellationToken;
        }

        public IReadOnlyList<ITransaction> GetAddressTransactions(EthereumAddress contractAddress, EthereumAddress address)
        {
            return new List<ITransaction>();
        }

        public async Task<long> GetLastBlockNumberAsync(EthereumAddress address)
        {
            var response = await amazonDynamoDB.QueryAsync(
                new QueryRequest()
                {
                    TableName = TableName,
                    IndexName = IndexName,
                    ScanIndexForward = false,
                    Select = Select.ALL_PROJECTED_ATTRIBUTES,
                    KeyConditionExpression = $"#{nameof(ITransaction.Address)} = :{nameof(ITransaction.Address)}Val",
                    ExpressionAttributeNames = new Dictionary<string, string>()
                    {
                        [$"#{nameof(ITransaction.Address)}"] = nameof(ITransaction.Address),
                    },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                    {
                        [$":{nameof(ITransaction.Address)}Val"] = new AttributeValue { S = address.Address }
                    },
                    Limit = 1
                },
                scopedCancellationToken.Token);

            if (response.HttpStatusCode == HttpStatusCode.OK &&
                response.Items.Count == 1 &&
                response.Items.Single().ContainsKey(nameof(ITransaction.BlockNumber)) &&
                long.TryParse(response.Items.Single()[nameof(ITransaction.BlockNumber)].N, out long blockNumber))
            {
                return blockNumber;
            }

            return default;
        }

        public async Task UploadTransactionAsync(ITransaction transaction)
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
    }
}
