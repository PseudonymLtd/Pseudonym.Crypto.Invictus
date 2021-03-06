﻿using System;
using System.Collections.Generic;
using Pseudonym.Crypto.Invictus.Funds.Abstractions;
using Pseudonym.Crypto.Invictus.Funds.Clients.Models;
using Pseudonym.Crypto.Invictus.Shared.Enums;

namespace Pseudonym.Crypto.Invictus.Funds.Services
{
    internal sealed class CurrencyConverter : ICurrencyConverter
    {
        private readonly Dictionary<CurrencyCode, decimal> rates;

        public CurrencyConverter()
        {
            rates = new Dictionary<CurrencyCode, decimal>();
        }

        public decimal? Convert(decimal? amount, CurrencyCode currencyCode)
        {
            if (!amount.HasValue)
            {
                return default;
            }

            return Convert(amount.Value, currencyCode);
        }

        public decimal Convert(decimal amount, CurrencyCode currencyCode)
        {
            if (currencyCode == CurrencyCode.USD)
            {
                return amount;
            }

            if (rates.ContainsKey(currencyCode))
            {
                return amount * rates[currencyCode];
            }

            throw new Exception($"MISSING_RATE_{currencyCode}");
        }

        public void UpdateRates(CurrencyRates rateResult)
        {
            foreach (var key in rateResult.Rates.Keys)
            {
                if (Enum.TryParse(key, out CurrencyCode code))
                {
                    if (rates.ContainsKey(code))
                    {
                        rates[code] = rateResult.Rates[key];
                    }
                    else
                    {
                        rates.Add(code, rateResult.Rates[key]);
                    }
                }
            }
        }
    }
}
