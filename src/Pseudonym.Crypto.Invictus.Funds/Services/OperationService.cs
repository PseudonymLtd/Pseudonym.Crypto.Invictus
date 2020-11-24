using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Utils;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Services
{
    internal sealed class OperationService : IOperationService
    {
        private const string TableName = "Pseudonym-Crypto-Invictus-Operations";
        private const string InboundIndexName = "Pseudonym-Crypto-Invictus-Operations-Inbound";
        private const string OutboundIndexName = "Pseudonym-Crypto-Invictus-Operations-Outbound";

        private readonly IAmazonDynamoDB amazonDynamoDB;
        private readonly IScopedCancellationToken scopedCancellationToken;

        public OperationService(
            IAmazonDynamoDB amazonDynamoDB,
            IScopedCancellationToken scopedCancellationToken)
        {
            this.amazonDynamoDB = amazonDynamoDB;
            this.scopedCancellationToken = scopedCancellationToken;
        }

        public async Task UploadOperationAsync(IOperation operation)
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
    }
}