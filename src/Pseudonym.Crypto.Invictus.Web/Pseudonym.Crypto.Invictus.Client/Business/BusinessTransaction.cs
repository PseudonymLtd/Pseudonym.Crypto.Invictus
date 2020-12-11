using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Shared.Exceptions;
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

        public decimal GetTransferPrice(string contractAddress, TransferAction action)
        {
            var transfers = Operations
                .Where(o =>
                    o.TransferAction == action &&
                    o.Contract.Address == contractAddress);

            switch (action)
            {
                case TransferAction.Inbound:
                    var inbound = transfers.LastOrDefault();
                    return inbound?.Quantity * inbound?.PricePerToken ?? 0;
                case TransferAction.Outbound:
                    var outbound = transfers.FirstOrDefault();
                    return outbound?.Quantity * outbound?.PricePerToken ?? 0;
                default:
                    if (Operations.Count == 1 &&
                        Operations.Single().TransferAction != TransferAction.None)
                    {
                        return Operations.Single().Quantity * Operations.Single().PricePerToken;
                    }
                    else
                    {
                        return 0;
                    }
            }
        }

        public decimal GetTransferQuantity(string contractAddress, TransferAction action)
        {
            var transfers = Operations
                .Where(o =>
                    o.TransferAction == action &&
                    o.Contract.Address == contractAddress);

            switch (action)
            {
                case TransferAction.Inbound:
                    return transfers.LastOrDefault()?.Quantity ?? 0;
                case TransferAction.Outbound:
                    return transfers.FirstOrDefault()?.Quantity ?? 0;
                default:
                    if (Operations.Count == 1 &&
                        Operations.Single().TransferAction != TransferAction.None)
                    {
                        return Operations.Single().Quantity;
                    }
                    else
                    {
                        return 0;
                    }
            }
        }

        public BusinessOperation GetSwap(string contractAddress, TransferAction action)
        {
            if (IsOutboundEtherSwap(contractAddress))
            {
                switch (action)
                {
                    case TransferAction.Inbound:
                        return Operations
                            .Where(o =>
                                o.Type == OperationTypes.Transfer &&
                                o.Contract.Address != contractAddress &&
                                o.TransferAction == TransferAction.None)
                            .LastOrDefault()
                                ?? throw new PermanentException($"Could not find {action} Ether Swap for {Hash}");
                    case TransferAction.Outbound:
                        return Operations
                            .Where(o =>
                                o.Type == OperationTypes.Transfer &&
                                o.Contract.Address == contractAddress &&
                                o.TransferAction == TransferAction.Outbound)
                            .FirstOrDefault()
                                ?? throw new PermanentException($"Could not find {action} Ether Swap for {Hash}");
                    default:
                        throw new ArgumentException($"Cannot Get Swap for action `{nameof(TransferAction.None)}`", nameof(action));
                }
            }
            else if (IsInboundEtherSwap(contractAddress))
            {
                switch (action)
                {
                    case TransferAction.Inbound:
                        return Operations
                            .Where(o =>
                                o.Type == OperationTypes.Transfer &&
                                o.Contract.Address == contractAddress &&
                                o.TransferAction == TransferAction.Inbound)
                            .LastOrDefault()
                                ?? throw new PermanentException($"Could not find {action} Ether Swap for {Hash}");
                    case TransferAction.Outbound:
                        return Operations
                            .Where(o =>
                                o.Type == OperationTypes.Transfer &&
                                o.Contract.Address != contractAddress &&
                                o.TransferAction == TransferAction.None)
                            .FirstOrDefault()
                                ?? throw new PermanentException($"Could not find {action} Ether Swap for {Hash}");
                    default:
                        throw new ArgumentException($"Cannot Get Swap for action `{nameof(TransferAction.None)}`", nameof(action));
                }
            }
            else if (IsOutboundTokenSwap(contractAddress))
            {
                switch (action)
                {
                    case TransferAction.Inbound:
                        return Operations
                            .Where(o =>
                                o.Type == OperationTypes.Transfer &&
                                o.Contract.Address != contractAddress &&
                                o.TransferAction == TransferAction.Inbound)
                            .LastOrDefault()
                                ?? throw new PermanentException($"Could not find {action} Token Swap for {Hash}");
                    case TransferAction.Outbound:
                        return Operations
                            .Where(o =>
                                o.Type == OperationTypes.Transfer &&
                                o.Contract.Address == contractAddress &&
                                o.TransferAction == TransferAction.Outbound)
                            .FirstOrDefault()
                                ?? throw new PermanentException($"Could not find {action} Token Swap for {Hash}");
                    default:
                        throw new ArgumentException($"Cannot Get Swap for action `{nameof(TransferAction.None)}`", nameof(action));
                }
            }
            else if (IsInboundTokenSwap(contractAddress))
            {
                switch (action)
                {
                    case TransferAction.Inbound:
                        return Operations
                            .Where(o =>
                                o.Type == OperationTypes.Transfer &&
                                o.Contract.Address == contractAddress &&
                                o.TransferAction == TransferAction.Inbound)
                            .LastOrDefault()
                                ?? throw new PermanentException($"Could not find {action} Token Swap for {Hash}");
                    case TransferAction.Outbound:
                        return Operations
                            .Where(o =>
                                o.Type == OperationTypes.Transfer &&
                                o.Contract.Address != contractAddress &&
                                o.TransferAction == TransferAction.Outbound)
                            .FirstOrDefault()
                                ?? throw new PermanentException($"Could not find {action} Token Swap for {Hash}");
                    default:
                        throw new ArgumentException($"Cannot Get Swap for action `{nameof(TransferAction.None)}`", nameof(action));
                }
            }
            else
            {
                throw new PermanentException($"{Hash} is not a swap.");
            }
        }
    }
}
