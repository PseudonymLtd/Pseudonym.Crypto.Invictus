using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Pseudonym.Crypto.Invictus.Shared.Models;

namespace Pseudonym.Crypto.Invictus.Web.Client.Business
{
    public sealed class BusinessTransaction
    {
        [Required]
        public string Hash { get; set; }

        [Required]
        public long BlockNumber { get; set; }

        [Required]
        public bool Success { get; set; }

        [Required]
        public DateTime ConfirmedAt { get; set; }

        [Required]
        public long Confirmations { get; set; }

        [Required]
        public string Sender { get; set; }

        [Required]
        public string Recipient { get; set; }

        [Required]
        public decimal Eth { get; set; }

        [Required]
        public long Gas { get; set; }

        [Required]
        public long GasLimit { get; set; }

        [Required]
        public decimal GasUsed { get; set; }

        [Required]
        public TransferAction TransferAction { get; set; }

        [Required]
        public IReadOnlyList<BusinessOperation> Operations { get; set; }

        [Required]
        public BusinessPrice Price { get; set; }

        public bool IsInbound(string contractAddress)
        {
            var lastOperation = Operations.LastOrDefault();

            return lastOperation != null &&
                lastOperation.TransferAction == TransferAction.Inbound &&
                lastOperation.Contract.Address == contractAddress;
        }

        public bool IsOutbound(string contractAddress)
        {
            return !IsInbound(contractAddress);
        }

        public bool IsSwap(string contractAddress) => IsEtherSwap(contractAddress) || IsTokenSwap(contractAddress);

        public bool IsTokenSwap(string contractAddress) => IsOutboundTokenSwap(contractAddress) || IsInboundTokenSwap(contractAddress);

        public bool IsInboundTokenSwap(string contractAddress) =>
            TransferAction == TransferAction.Outbound &&
            Operations.Any(o =>
                o.TransferAction == TransferAction.Outbound &&
                o.Contract.Address != contractAddress) &&
            Operations.Any(o =>
                o.TransferAction == TransferAction.Inbound &&
                o.Contract.Address == contractAddress);

        public bool IsOutboundTokenSwap(string contractAddress) =>
            TransferAction == TransferAction.Outbound &&
            Operations.Any(o =>
                o.TransferAction == TransferAction.Outbound &&
                o.Contract.Address == contractAddress) &&
            Operations.Any(o =>
                o.TransferAction == TransferAction.Inbound &&
                o.Contract.Address != contractAddress);

        public bool IsEtherSwap(string contractAddress) => IsOutboundEtherSwap(contractAddress) || IsInboundEtherSwap(contractAddress);

        public bool IsInboundEtherSwap(string contractAddress) =>
            TransferAction == TransferAction.Outbound &&
            Eth > 0 &&
            Operations.Any(o =>
                o.TransferAction == TransferAction.Inbound &&
                o.Contract.Address == contractAddress);

        public bool IsOutboundEtherSwap(string contractAddress) =>
            TransferAction == TransferAction.Outbound &&
            Operations.Any(o =>
                o.TransferAction == TransferAction.Outbound &&
                o.Contract.Address == contractAddress) &&
            Operations.Any(o =>
                o.TransferAction == TransferAction.None &&
                o.Contract.Symbol == "WETH");
    }
}
