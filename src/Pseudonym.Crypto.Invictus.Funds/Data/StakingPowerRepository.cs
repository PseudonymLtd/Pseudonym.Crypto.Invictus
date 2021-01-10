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
    internal sealed class StakingPowerRepository : BaseRepository<DataStakingPower>, IStakingPowerRepository
    {
        private const int PageSize = 100;

        private const string TableName = "Pseudonym-Crypto-Invictus-Staking-Power";

        public StakingPowerRepository(
            IAmazonDynamoDB amazonDynamoDB,
            IScopedCancellationToken scopedCancellationToken)
            : base(amazonDynamoDB, scopedCancellationToken, TableName)
        {
        }

        public async Task<DataStakingPower> GetStakingPowerAsync(EthereumAddress stakeAddress, DateTime timeStamp)
        {
            var response = await DynamoDB.GetItemAsync(
                TableName,
                new Dictionary<string, AttributeValue>()
                {
                    [nameof(DataStakingPower.Address)] = new AttributeValue(stakeAddress),
                    [nameof(DataStakingPower.Date)] = new AttributeValue(timeStamp.ToISO8601String())
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

        public async IAsyncEnumerable<DataStakingPower> ListStakingPowersAsync(
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
                        ScanIndexForward = false,
                        Select = Select.SPECIFIC_ATTRIBUTES,
                        KeyConditionExpression = string.Format(
                            "#{0} = :{0}Val AND #{1} >= :{2}Val",
                            nameof(DataStakingPower.Address),
                            nameof(DataStakingPower.Date),
                            nameof(from)),
                        ExpressionAttributeNames = new Dictionary<string, string>()
                        {
                            [$"#{nameof(DataStakingPower.Address)}"] = nameof(DataStakingPower.Address),
                            [$"#{nameof(DataStakingPower.Date)}"] = nameof(DataStakingPower.Date),
                        },
                        ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                        {
                            [$":{nameof(DataStakingPower.Address)}Val"] = new AttributeValue(contractAddress),
                            [$":{nameof(from)}Val"] = new AttributeValue(from.ToISO8601String())
                        },
                        ProjectionExpression = string.Join(",", new List<string>()
                        {
                            $"#{nameof(DataStakingPower.Address)}",
                            $"#{nameof(DataStakingPower.Date)}",
                            $"{nameof(DataStakingPower.Power)}",
                            $"{nameof(DataStakingPower.Summary)}"
                        }),
                        Limit = PageSize,
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

                foreach (var power in response.Items
                    .Select(Map)
                    .OrderBy(p => p.Date))
                {
                    if (power.Date > to)
                    {
                        break;
                    }

                    yield return power;
                }

                if (!lastEvaluatedKey.Any())
                {
                    break;
                }
            }
        }

        public Task<DataStakingPower> GetLatestAsync(EthereumAddress stakeAddress)
        {
            return GetByBoundsAsync(stakeAddress, true);
        }

        public Task<DataStakingPower> GetLowestAsync(EthereumAddress stakeAddress)
        {
            return GetByBoundsAsync(stakeAddress, false);
        }

        protected sealed override DataStakingPower Map(Dictionary<string, AttributeValue> attributes)
        {
            return new DataStakingPower()
            {
                Address = attributes[nameof(DataStakingPower.Address)].S,
                Date = DateTimeOffset.Parse(attributes[nameof(DataStakingPower.Date)].S, styles: DateTimeStyles.AssumeUniversal).UtcDateTime,
                Power = decimal.Parse(attributes[nameof(DataStakingPower.Power)].N),
                Summary = attributes[nameof(DataStakingPower.Summary)].L != null
                    ? attributes[nameof(DataStakingPower.Summary)].L
                        .Select(x => new DataStakingPowerSummary()
                        {
                            ContractAddress = x.M[nameof(DataStakingPowerSummary.ContractAddress)].S,
                            Power = decimal.Parse(x.M[nameof(DataStakingPowerSummary.Power)].N)
                        })
                        .ToList()
                    : new List<DataStakingPowerSummary>(),
                Breakdown =
                    attributes.ContainsKey(nameof(DataStakingPower.Breakdown)) &&
                    attributes[nameof(DataStakingPower.Breakdown)].L != null
                        ? attributes[nameof(DataStakingPower.Breakdown)].L
                            .Select(x => new DataStakingPowerFund()
                            {
                                ContractAddress = x.M[nameof(DataStakingPowerFund.ContractAddress)].S,
                                FundModifier = decimal.Parse(x.M[nameof(DataStakingPowerFund.FundModifier)].N),
                                PricePerToken = decimal.Parse(x.M[nameof(DataStakingPowerFund.PricePerToken)].N),
                                Events = x.M[nameof(DataStakingPowerFund.Events)].L != null
                                    ? x.M[nameof(DataStakingPowerFund.Events)].L
                                        .Select(y => new DataStakingEvent()
                                        {
                                            UserAddress = y.M[nameof(DataStakingEvent.UserAddress)].S,
                                            Quantity = decimal.Parse(y.M[nameof(DataStakingEvent.Quantity)].N),
                                            TimeModifier = decimal.Parse(y.M[nameof(DataStakingEvent.TimeModifier)].N),
                                            StakedAt = DateTimeOffset.Parse(y.M[nameof(DataStakingEvent.StakedAt)].S, styles: DateTimeStyles.AssumeUniversal).UtcDateTime,
                                            ExpiresAt = DateTimeOffset.Parse(y.M[nameof(DataStakingEvent.ExpiresAt)].S, styles: DateTimeStyles.AssumeUniversal).UtcDateTime,
                                        })
                                        .ToList()
                                    : new List<DataStakingEvent>()
                            })
                            .ToList()
                        : new List<DataStakingPowerFund>()
            };
        }

        private async Task<DataStakingPower> GetByBoundsAsync(EthereumAddress contractAddress, bool latest)
        {
            var response = await DynamoDB.QueryAsync(
                new QueryRequest()
                {
                    TableName = TableName,
                    ScanIndexForward = !latest,
                    Select = Select.ALL_ATTRIBUTES,
                    KeyConditionExpression = $"#{nameof(DataStakingPower.Address)} = :{nameof(DataStakingPower.Address)}Val",
                    ExpressionAttributeNames = new Dictionary<string, string>()
                    {
                        [$"#{nameof(DataStakingPower.Address)}"] = nameof(DataStakingPower.Address),
                    },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                    {
                        [$":{nameof(DataStakingPower.Address)}Val"] = new AttributeValue(contractAddress)
                    },
                    Limit = 1
                },
                CancellationToken);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new HttpRequestException($"Response code did not indicate success: {response.HttpStatusCode}");
            }

            if (response.Items.Count == 1)
            {
                return Map(response.Items.Single());
            }

            return null;
        }
    }
}
