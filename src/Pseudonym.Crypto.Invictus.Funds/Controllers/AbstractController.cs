using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Configuration;
using Pseudonym.Crypto.Invictus.Shared.Models;

namespace Pseudonym.Crypto.Invictus.Funds.Controllers
{
    public abstract class AbstractController : Controller
    {
        public AbstractController(IOptions<AppSettings> appSettings)
        {
            AppSettings = appSettings.Value;
        }

        protected AppSettings AppSettings { get; }

        protected ApiFund MapFund(IFund fund)
        {
            return new ApiFund()
            {
                Name = fund.Name,
                DisplayName = fund.DisplayName,
                Description = fund.Description,
                Token = new ApiToken()
                {
                    Symbol = fund.Token.Symbol,
                    Decimals = fund.Token.Decimals,
                    Address = fund.Token.ContractAddress.Address
                },
                CirculatingSupply = fund.CirculatingSupply,
                NetAssetValue = fund.NetValue,
                NetAssetValuePerToken = fund.NetAssetValuePerToken,
                Market = new ApiMarket()
                {
                    IsTradeable = fund.Market.IsTradable,
                    Cap = fund.Market.Cap,
                    Total = fund.Market.Total,
                    PricePerToken = fund.Market.PricePerToken,
                    DiffDaily = fund.Market.DiffDaily,
                    DiffWeekly = fund.Market.DiffWeekly,
                    DiffMonthly = fund.Market.DiffMonthly,
                    Volume = fund.Market.Volume,
                    VolumeDiffDaily = fund.Market.VolumeDiffDaily,
                    VolumeDiffWeekly = fund.Market.VolumeDiffWeekly,
                    VolumeDiffMonthly = fund.Market.VolumeDiffMonthly
                },
                Assets = fund.Assets
                    .Select(a => new ApiAsset()
                    {
                        Name = a.Name,
                        Symbol = a.Symbol ?? "-",
                        Value = a.Value,
                        Share = a.Share,
                        Link = a.Link
                    })
                    .ToList(),
                Links = new ApiLinks()
                {
                    [nameof(ApiLinks.Self)] = new Uri(AppSettings.HostUrl.OriginalString.TrimEnd('/') + $"/api/v1/funds/{fund.Token.Symbol}", UriKind.Absolute),
                    [nameof(ApiLinks.Lite)] = fund.LitepaperUri,
                    [nameof(ApiLinks.Fact)] = fund.FactSheetUri,
                    [nameof(ApiLinks.External)] = fund.InvictusUri
                }
            };
        }
    }
}
