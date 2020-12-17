using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Web.Client.Abstractions;

#pragma warning disable SA1402

namespace Pseudonym.Crypto.Invictus.Web.Client.Configuration
{
    public sealed class UserSettings : IUserSettings
    {
        private static readonly Regex AddressRegex = new Regex("^0x+[A-F,a-f,0-9]{40}$");

        private readonly Dictionary<Symbol, FundInfo> funds;
        private readonly List<string> secondaryAddresses;

        public UserSettings(Dictionary<Symbol, FundInfo> funds, List<string> secondaryAddresses)
        {
            this.funds = funds;
            this.secondaryAddresses = secondaryAddresses;
        }

        public CurrencyCode CurrencyCode { get; set; } = CurrencyCode.USD;

        public string WalletAddress { get; set; }

        public IReadOnlyList<string> SecondaryWalletAddresses => secondaryAddresses;

        public IReadOnlyDictionary<Symbol, FundInfo> Funds => funds;

        public bool HasValidAddress()
        {
            return IsValidAddress(WalletAddress);
        }

        public bool IsValidAddress(string candidateAddress)
        {
            return AddressRegex.IsMatch(candidateAddress ?? string.Empty);
        }

        public void AddFund(Symbol symbol, FundInfo fund)
        {
            if (!funds.ContainsKey(symbol))
            {
                funds.Add(symbol, fund);
            }
            else
            {
                funds[symbol].Name = fund.Name;
                funds[symbol].DisplayName = fund.DisplayName;
                funds[symbol].Description = fund.Description;
                funds[symbol].Decimals = fund.Decimals;
                funds[symbol].ContractAddress = fund.ContractAddress;
            }
        }

        public void AddSecondaryAddress(string address)
        {
            if (!secondaryAddresses.Contains(address) && IsValidAddress(address))
            {
                secondaryAddresses.Add(address);
            }
        }

        public void RemoveSecondaryAddress(string address)
        {
            if (secondaryAddresses.Contains(address))
            {
                secondaryAddresses.Remove(address);
            }
        }
    }

    public sealed class FundInfo
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public int Decimals { get; set; }

        public string ContractAddress { get; set; }

        public Uri FundLink { get; set; }
    }
}
