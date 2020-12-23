using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Pseudonym.Crypto.Invictus.Funds.Utils;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Data
{
    internal abstract class BaseRepository<TEntity>
        where TEntity : class, new()
    {
        private const int MaxBatchWrites = 25;

        private readonly IScopedCancellationToken scopedCancellationToken;
        private readonly string tableName;

        protected BaseRepository(
            IAmazonDynamoDB amazonDynamoDB,
            IScopedCancellationToken scopedCancellationToken,
            string tableName)
        {
            this.scopedCancellationToken = scopedCancellationToken;
            this.tableName = tableName;

            DynamoDB = amazonDynamoDB;
        }

        protected IAmazonDynamoDB DynamoDB { get; }

        protected CancellationToken CancellationToken => scopedCancellationToken.Token;

        public async Task UploadItemsAsync(params TEntity[] items)
        {
            if (items.Length == 1)
            {
                var attributes = DynamoDbConvert.Serialize(items.Single());

                var response = await DynamoDB.PutItemAsync(
                    tableName,
                    attributes,
                    scopedCancellationToken.Token);

                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    throw new HttpRequestException($"Response code did not indicate success: {response.HttpStatusCode}");
                }
            }
            else
            {
                for (int skip = 0; skip < items.Length; skip += MaxBatchWrites)
                {
                    var attributeGroups = items
                        .Skip(skip)
                        .Take(MaxBatchWrites)
                        .Select(DynamoDbConvert.Serialize);

                    var response = await DynamoDB.BatchWriteItemAsync(
                        new Dictionary<string, List<WriteRequest>>()
                        {
                            [tableName] = attributeGroups
                                .Select(attributes => new WriteRequest(new PutRequest(attributes)))
                                .ToList()
                        },
                        scopedCancellationToken.Token);

                    if (response.HttpStatusCode != HttpStatusCode.OK)
                    {
                        throw new HttpRequestException($"Response code did not indicate success: {response.HttpStatusCode}");
                    }
                }
            }
        }

        protected abstract TEntity Map(Dictionary<string, AttributeValue> attributes);
    }
}
