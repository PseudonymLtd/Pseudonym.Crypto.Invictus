using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Data.Models;
using Pseudonym.Crypto.Invictus.Funds.Ethereum;
using Pseudonym.Crypto.Invictus.Shared.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Data
{
    internal sealed class FundPerformanceRepository : BaseRepository<DataFundPerformance>, IFundPerformanceRepository
    {
        private const string TableName = "Pseudonym-Crypto-Invictus-Fund-Performance";

        public FundPerformanceRepository(
            IAmazonDynamoDB amazonDynamoDB,
            IScopedCancellationToken scopedCancellationToken)
            : base(amazonDynamoDB, scopedCancellationToken, TableName)
        {
        }

        public async Task<DataFundPerformance> GetPerformanceAsync(EthereumAddress contractAddress, DateTime timeStamp)
        {
            var response = await DynamoDB.GetItemAsync(
                TableName,
                new Dictionary<string, AttributeValue>()
                {
                    [nameof(DataFundPerformance.Address)] = new AttributeValue(contractAddress),
                    [nameof(DataFundPerformance.Date)] = new AttributeValue(timeStamp.ToISO8601String())
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

        public async IAsyncEnumerable<DataFundPerformance> ListPerformancesAsync(
            EthereumAddress contractAddress,
            DateTime from,
            DateTime to)
        {
            var lastEvaluatedKey = new Dictionary<string, AttributeValue>();

            while (!CancellationToken.IsCancellationRequested)
            {
                var response = await DynamoDB.QueryAsync(
                    new QueryRequest()
                    {
                        TableName = TableName,
                        ScanIndexForward = true,
                        Select = Select.ALL_ATTRIBUTES,
                        KeyConditionExpression = string.Format(
                            "#{0} = :{0}Val AND #{1} >= :{2}Val",
                            nameof(DataFundPerformance.Address),
                            nameof(DataFundPerformance.Date),
                            nameof(from)),
                        ExpressionAttributeNames = new Dictionary<string, string>()
                        {
                            [$"#{nameof(DataFundPerformance.Address)}"] = nameof(DataFundPerformance.Address),
                            [$"#{nameof(DataFundPerformance.Date)}"] = nameof(DataFundPerformance.Date),
                        },
                        ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                        {
                            [$":{nameof(DataFundPerformance.Address)}Val"] = new AttributeValue { S = contractAddress },
                            [$":{nameof(from)}Val"] = new AttributeValue { S = from.ToISO8601String() }
                        },
                        ExclusiveStartKey = lastEvaluatedKey
                    },
                    CancellationToken);

                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    throw new HttpRequestException($"Response code did not indicate success: {response.HttpStatusCode}");
                }
                else
                {
                    lastEvaluatedKey = response.LastEvaluatedKey;
                }

                foreach (var perf in response.Items
                    .Select(Map)
                    .OrderBy(p => p.Date))
                {
                    if (perf.Date > to)
                    {
                        break;
                    }

                    yield return perf;
                }

                if (!lastEvaluatedKey.Any())
                {
                    break;
                }
            }
        }

        public Task<DateTime?> GetLatestDateAsync(EthereumAddress address)
        {
            return GetDateAsync(address, true);
        }

        public Task<DateTime?> GetLowestDateAsync(EthereumAddress address)
        {
            return GetDateAsync(address, false);
        }

        protected sealed override DataFundPerformance Map(Dictionary<string, AttributeValue> attributes)
        {
            return new DataFundPerformance()
            {
                Address = attributes[nameof(DataFundPerformance.Address)].S,
                Date = DateTime.Parse(attributes[nameof(DataFundPerformance.Date)].S),
                Nav = decimal.Parse(attributes[nameof(DataFundPerformance.Nav)].N),
                Price = decimal.Parse(attributes[nameof(DataFundPerformance.Price)].N),
                MarketCap = decimal.Parse(attributes[nameof(DataFundPerformance.MarketCap)].N),
                Volume = decimal.Parse(attributes[nameof(DataFundPerformance.Volume)].N),
            };
        }

        private async Task<DateTime?> GetDateAsync(EthereumAddress address, bool latest)
        {
            var response = await DynamoDB.QueryAsync(
                new QueryRequest()
                {
                    TableName = TableName,
                    ScanIndexForward = !latest,
                    Select = Select.ALL_ATTRIBUTES,
                    KeyConditionExpression = $"#{nameof(DataFundPerformance.Address)} = :{nameof(DataFundPerformance.Address)}Val",
                    ExpressionAttributeNames = new Dictionary<string, string>()
                    {
                        [$"#{nameof(DataFundPerformance.Address)}"] = nameof(DataFundPerformance.Address),
                    },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                    {
                        [$":{nameof(DataFundPerformance.Address)}Val"] = new AttributeValue { S = address }
                    },
                    Limit = 1
                },
                CancellationToken);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new HttpRequestException($"Response code did not indicate success: {response.HttpStatusCode}");
            }

            if (response.Items.Count == 1 &&
                response.Items.Single().ContainsKey(nameof(DataFundPerformance.Date)) &&
                DateTimeOffset.TryParse(
                    response.Items.Single()[nameof(DataFundPerformance.Date)].S,
                    null,
                    DateTimeStyles.AssumeUniversal,
                    out DateTimeOffset date))
            {
                return date.UtcDateTime;
            }

            return null;
        }
    }
}