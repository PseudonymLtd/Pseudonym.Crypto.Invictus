using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Shared.Exceptions;

namespace Pseudonym.Crypto.Invictus.Funds.Ethereum
{
    public readonly struct EthereumAddress : IEquatable<EthereumAddress>
    {
        private static readonly string EmptyHex = string.Empty.PadLeft(40, '0');
        private static readonly Regex AddressRegex = new Regex("^0x+[A-F,a-f,0-9]{40}$");

        public EthereumAddress(string hex)
        {
            if (hex == null)
            {
                throw new ArgumentNullException(nameof(hex), "hex cannot be null.");
            }

            Hex = hex.ToLower();

            if (Hex.StartsWith("0x"))
            {
                Hex = Hex.Substring(2);
            }

            if (!AddressRegex.IsMatch(Address))
            {
                throw new PermanentException("Supplied ethereum address was not valid.");
            }
        }

        public static EthereumAddress Empty => new EthereumAddress(EmptyHex);

        [JsonIgnore]
        public string Hex { get; }

        [JsonProperty("address")]
        public string Address => $"0x{Hex ?? EmptyHex}";

        public static implicit operator string(EthereumAddress addr) => addr.Address;

        public static explicit operator EthereumAddress(string hex) => new EthereumAddress(hex);

        public bool Equals([AllowNull] EthereumAddress other)
        {
            return Hex.Equals(other.Hex, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return obj is EthereumAddress addr && addr.Equals(this);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Hex);
        }

        public override string ToString()
        {
            return Address;
        }
    }
}
