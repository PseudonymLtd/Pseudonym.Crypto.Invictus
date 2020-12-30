using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Pseudonym.Crypto.Invictus.Shared.Exceptions;
using Pseudonym.Crypto.Invictus.Shared.Models;
using Pseudonym.Crypto.Invictus.Web.Client.Abstractions;
using Pseudonym.Crypto.Invictus.Web.Client.Business.Abstractions;

namespace Pseudonym.Crypto.Invictus.Web.Client.Business
{
    public sealed class BusinessTransaction
    {
        [Required]
        public string Hash { get; set; }

        [Required]
        public string ContractAddress { get; set; }

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

        public ITrade GetTrade(ApiFund fund, IUserSettings settings)
        {
            return IsSwap()
                ? GetSwapData(fund)
                : GetTradeData(fund, settings);
        }

        public BusinessTrade GetTradeData(ApiFund fund, IUserSettings settings)
        {
            if (!IsSwap())
            {
                return new BusinessTrade()
                {
                    IsTradeable = fund.Market.IsTradeable,
                    IsInbound = IsInbound(),
                    Quantity = GetTransferQuantity(TransferAction),
                    NetCurrentPricePerToken = fund.Nav.ValuePerToken,
                    NetSnapshotPricePerToken = Price.NetAssetValuePerToken,
                    MarketCurrentPricePerToken = fund.Market.IsTradeable
                        ? fund.Market.PricePerToken
                        : default(decimal?),
                    MarketSnapshotPricePerToken = fund.Market.IsTradeable
                        ? GetTransferPricePerToken(TransferAction)
                        : default(decimal?),
                    IsStake = Recipient.Equals(settings.StakingAddress, StringComparison.OrdinalIgnoreCase),
                    IsOwned = settings.SecondaryWalletAddresses.Contains(Recipient) || settings.SecondaryWalletAddresses.Contains(Sender)
                };
            }
            else
            {
                throw new PermanentException($"{Hash} is not a trade.");
            }
        }

        public BusinessSwap GetSwapData(ApiFund fund)
        {
            if (IsSwap())
            {
                var outboundSwap = GetSwap(TransferAction.Outbound);
                var inboundSwap = GetSwap(TransferAction.Inbound);
                var isInbound = inboundSwap.Contract.Address == fund.Token.Address;

                var fundOp = isInbound
                    ? inboundSwap
                    : outboundSwap;

                return new BusinessSwap()
                {
                    InboundSymbol = IsOutboundEtherSwap()
                        ? "ETH"
                        : inboundSwap.Contract.Symbol,
                    InboundQuantity = inboundSwap.Quantity,
                    OutboundSymbol = IsInboundEtherSwap()
                        ? "ETH"
                        : outboundSwap.Contract.Symbol,
                    OutboundQuantity = outboundSwap.Quantity,
                    IsTradeable = fund.Market.IsTradeable,
                    IsInbound = isInbound,
                    Quantity = fundOp.Quantity,
                    NetCurrentPricePerToken = fund.Nav.ValuePerToken,
                    NetSnapshotPricePerToken = Price.NetAssetValuePerToken,
                    MarketCurrentPricePerToken = fund.Market.IsTradeable
                        ? fund.Market.PricePerToken
                        : default(decimal?),
                    MarketSnapshotPricePerToken = fund.Market.IsTradeable
                        ? fundOp.PricePerToken
                        : default(decimal?),
                    IsStake = false,
                    IsOwned = false
                };
            }
            else
            {
                throw new PermanentException($"{Hash} is not a swap.");
            }
        }

        public bool IsInbound()
        {
            var lastOperation = Operations.LastOrDefault();

            return lastOperation != null &&
                lastOperation.TransferAction == TransferAction.Inbound &&
                lastOperation.Contract.Address == ContractAddress;
        }

        public bool IsOutbound()
        {
            return !IsInbound();
        }

        public bool IsSwap() => IsEtherSwap() || IsTokenSwap();

        public bool IsTokenSwap() => IsOutboundTokenSwap() || IsInboundTokenSwap();

        public bool IsInboundTokenSwap() =>
            TransferAction == TransferAction.Outbound &&
            Operations.Any(o =>
                o.TransferAction == TransferAction.Outbound &&
                o.Contract.Address != ContractAddress) &&
            Operations.Any(o =>
                o.TransferAction == TransferAction.Inbound &&
                o.Contract.Address == ContractAddress);

        public bool IsOutboundTokenSwap() =>
            TransferAction == TransferAction.Outbound &&
            Operations.Any(o =>
                o.TransferAction == TransferAction.Outbound &&
                o.Contract.Address == ContractAddress) &&
            Operations.Any(o =>
                o.TransferAction == TransferAction.Inbound &&
                o.Contract.Address != ContractAddress);

        public bool IsEtherSwap() => IsOutboundEtherSwap() || IsInboundEtherSwap();

        public bool IsInboundEtherSwap() =>
            TransferAction == TransferAction.Outbound &&
            Eth > 0 &&
            Operations.Any(o =>
                o.TransferAction == TransferAction.Inbound &&
                o.Contract.Address == ContractAddress);

        public bool IsOutboundEtherSwap() =>
            TransferAction == TransferAction.Outbound &&
            Operations.Any(o =>
                o.TransferAction == TransferAction.Outbound &&
                o.Contract.Address == ContractAddress) &&
            Operations.Any(o =>
                o.TransferAction == TransferAction.None &&
                o.Contract.Symbol == "WETH");

        public decimal GetTransferPricePerToken(TransferAction action)
        {
            var transfers = Operations
                .Where(o =>
                    o.TransferAction == action &&
                    o.Contract.Address == ContractAddress);

            switch (action)
            {
                case TransferAction.Inbound:
                    return transfers.LastOrDefault()?.PricePerToken ?? 0;
                case TransferAction.Outbound:
                    return transfers.FirstOrDefault()?.PricePerToken ?? 0;
                default:
                    if (Operations.Count == 1 &&
                        Operations.Single().TransferAction != TransferAction.None)
                    {
                        return Operations.Single().PricePerToken;
                    }
                    else
                    {
                        return 0;
                    }
            }
        }

        public decimal GetTransferQuantity(TransferAction action)
        {
            var transfers = Operations
                .Where(o =>
                    o.TransferAction == action &&
                    o.Contract.Address == ContractAddress);

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

        public BusinessOperation GetSwap(TransferAction action)
        {
            if (IsOutboundEtherSwap())
            {
                switch (action)
                {
                    case TransferAction.Inbound:
                        return Operations
                            .Where(o =>
                                o.Type == OperationTypes.Transfer &&
                                o.Contract.Address != ContractAddress &&
                                o.TransferAction == TransferAction.None)
                            .LastOrDefault()
                                ?? throw new PermanentException($"Could not find {action} Ether Swap for {Hash}");
                    case TransferAction.Outbound:
                        return Operations
                            .Where(o =>
                                o.Type == OperationTypes.Transfer &&
                                o.Contract.Address == ContractAddress &&
                                o.TransferAction == TransferAction.Outbound)
                            .FirstOrDefault()
                                ?? throw new PermanentException($"Could not find {action} Ether Swap for {Hash}");
                    default:
                        throw new ArgumentException($"Cannot Get Swap for action `{nameof(TransferAction.None)}`", nameof(action));
                }
            }
            else if (IsInboundEtherSwap())
            {
                switch (action)
                {
                    case TransferAction.Inbound:
                        return Operations
                            .Where(o =>
                                o.Type == OperationTypes.Transfer &&
                                o.Contract.Address == ContractAddress &&
                                o.TransferAction == TransferAction.Inbound)
                            .LastOrDefault()
                                ?? throw new PermanentException($"Could not find {action} Ether Swap for {Hash}");
                    case TransferAction.Outbound:
                        return Operations
                            .Where(o =>
                                o.Type == OperationTypes.Transfer &&
                                o.Contract.Address != ContractAddress &&
                                o.TransferAction == TransferAction.None)
                            .FirstOrDefault()
                                ?? throw new PermanentException($"Could not find {action} Ether Swap for {Hash}");
                    default:
                        throw new ArgumentException($"Cannot Get Swap for action `{nameof(TransferAction.None)}`", nameof(action));
                }
            }
            else if (IsOutboundTokenSwap())
            {
                switch (action)
                {
                    case TransferAction.Inbound:
                        return Operations
                            .Where(o =>
                                o.Type == OperationTypes.Transfer &&
                                o.Contract.Address != ContractAddress &&
                                o.TransferAction == TransferAction.Inbound)
                            .LastOrDefault()
                                ?? throw new PermanentException($"Could not find {action} Token Swap for {Hash}");
                    case TransferAction.Outbound:
                        return Operations
                            .Where(o =>
                                o.Type == OperationTypes.Transfer &&
                                o.Contract.Address == ContractAddress &&
                                o.TransferAction == TransferAction.Outbound)
                            .FirstOrDefault()
                                ?? throw new PermanentException($"Could not find {action} Token Swap for {Hash}");
                    default:
                        throw new ArgumentException($"Cannot Get Swap for action `{nameof(TransferAction.None)}`", nameof(action));
                }
            }
            else if (IsInboundTokenSwap())
            {
                switch (action)
                {
                    case TransferAction.Inbound:
                        return Operations
                            .Where(o =>
                                o.Type == OperationTypes.Transfer &&
                                o.Contract.Address == ContractAddress &&
                                o.TransferAction == TransferAction.Inbound)
                            .LastOrDefault()
                                ?? throw new PermanentException($"Could not find {action} Token Swap for {Hash}");
                    case TransferAction.Outbound:
                        return Operations
                            .Where(o =>
                                o.Type == OperationTypes.Transfer &&
                                o.Contract.Address != ContractAddress &&
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
