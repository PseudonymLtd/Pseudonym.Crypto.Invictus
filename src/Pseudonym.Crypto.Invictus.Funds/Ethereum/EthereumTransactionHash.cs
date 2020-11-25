using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Shared.Exceptions;

namespace Pseudonym.Crypto.Invictus.Funds.Ethereum
{
    public readonly struct EthereumTransactionHash : IEquatable<EthereumTransactionHash>
    {
        private static readonly string EmptyHex = string.Empty.PadLeft(64, '0');
        private static readonly Regex AddressRegex = new Regex("^0x+[A-F,a-f,0-9]{64}$");

        public EthereumTransactionHash(string hex)
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

            if (!AddressRegex.IsMatch(Hash))
            {
                throw new PermanentException("Supplied ethereum address was not valid.");
            }
        }

        public static EthereumTransactionHash Empty => new EthereumTransactionHash(EmptyHex);

        [JsonIgnore]
        public string Hex { get; }

        [JsonProperty("hash")]
        public string Hash => $"0x{Hex ?? EmptyHex}";

        public static implicit operator string(EthereumTransactionHash addr) => addr.Hash;

        public static explicit operator EthereumTransactionHash(string hex) => new EthereumTransactionHash(hex);

        public bool Equals([AllowNull] EthereumTransactionHash other)
        {
            return Hex.Equals(other.Hex, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return obj is EthereumTransactionHash addr && addr.Equals(this);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Hex);
        }

        public override string ToString()
        {
            return Hash;
        }
    }
}