using System;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;

#pragma warning disable SA1402

namespace Pseudonym.Crypto.Invictus.Funds.Clients.Models.BlockCypher
{
    public sealed class BlockCypherAddressResponse
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("total_received")]
        public BigInteger TotalIn { get; set; }

        [JsonProperty("total_sent")]
        public BigInteger TotalOut { get; set; }

        [JsonProperty("balance")]
        public BigInteger ConfirmedBalance { get; set; }

        [JsonProperty("unconfirmed_balance")]
        public BigInteger UnconfirmedBalance { get; set; }

        [JsonProperty("final_balance")]
        public BigInteger ProjectedBalance { get; set; }

        [JsonProperty("n_tx")]
        public int ConfirmedTransactionCount { get; set; }

        [JsonProperty("unconfirmed_n_tx")]
        public int UnconfirmedTransactionCount { get; set; }

        [JsonProperty("final_n_tx")]
        public int ProjectedTransactionCount { get; set; }

        [JsonProperty("nonce")]
        public int Nonce { get; set; }

        [JsonProperty("pool_nonce")]
        public int PoolNonce { get; set; }

        [JsonProperty("txrefs")]
        public IReadOnlyList<BlockCypherTransactionSummary> Transactions { get; set; }

        [JsonProperty("hasMore")]
        public bool HasMoreTransactions { get; set; }
    }

    public sealed class BlockCypherTransactionSummary
    {
        [JsonProperty("tx_hash")]
        public string Hash { get; set; }

        [JsonProperty("block_height")]
        public int BlockNumber { get; set; }

        [JsonProperty("tx_input_n")]
        public int InputIndex { get; set; }

        [JsonProperty("tx_output_n")]
        public int OutputIndex { get; set; }

        [JsonProperty("value")]
        public BigInteger Value { get; set; }

        [JsonProperty("ref_balance")]
        public BigInteger PreviousBalance { get; set; }

        [JsonProperty("confirmations")]
        public int Confirmations { get; set; }

        [JsonProperty("confirmed")]
        public DateTime ConfirmedAt { get; set; }

        [JsonProperty("double_spend")]
        public bool IsDoubleSpend { get; set; }
    }
}
