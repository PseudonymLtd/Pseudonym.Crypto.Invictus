using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Shared.Exceptions;
using Pseudonym.Crypto.Invictus.Shared.Models;
using Pseudonym.Crypto.Invictus.Shared.Models.Abstractions;
using Pseudonym.Crypto.Invictus.Web.Client.Abstractions;
using Pseudonym.Crypto.Invictus.Web.Client.Business.Abstractions;

namespace Pseudonym.Crypto.Invictus.Web.Client.Business
{
    public sealed class BusinessTransaction
    {
        private const string ZeroAddress = "0x0000000000000000000000000000000000000000";

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
        public IReadOnlyList<BusinessOperation> Operations { get; set; }

        [Required]
        public BusinessPrice Price { get; set; }

        [Required]
        public TransferAction TransferAction => Operations.LastOrDefault(
            x =>
                x.Contract.Address.Equals(ContractAddress, StringComparison.OrdinalIgnoreCase) &&
                x.TransferAction != TransferAction.None)
            ?.TransferAction
            ?? TransferAction.None;

        public ITrade GetTrade(IProduct product, IUserSettings settings)
        {
            return IsSwap()
                ? GetSwapData(product)
                : GetTradeData(product, settings);
        }

        public BusinessTrade GetTradeData(IProduct product, IUserSettings settings)
        {
            if (!IsSwap())
            {
                return new BusinessTrade()
                {
                    Date = ConfirmedAt,
                    IsTradeable = product.Market.IsTradeable,
                    IsInbound = IsInbound(),
                    Quantity = GetTransferQuantity(TransferAction, settings),
                    NetCurrentPricePerToken = product.Nav.ValuePerToken,
                    NetSnapshotPricePerToken = Price.NetAssetValuePerToken,
                    MarketCurrentPricePerToken = product.Market.IsTradeable
                        ? product.Market.PricePerToken
                        : default(decimal?),
                    MarketSnapshotPricePerToken = product.Market.IsTradeable
                        ? GetTransferPricePerToken(TransferAction, settings)
                        : default(decimal?),
                    IsStakeLockup = Recipient.Equals(settings.StakingAddress, StringComparison.OrdinalIgnoreCase),
                    IsStakeRelease = Operations.Any(x =>
                        x.Type == OperationTypes.Transfer &&
                        x.Sender.Equals(settings.StakingAddress, StringComparison.OrdinalIgnoreCase)),
                    IsOwned = settings.SecondaryWalletAddresses.Contains(Recipient) || settings.SecondaryWalletAddresses.Contains(Sender),
                    IsBurn = Operations.Any(x =>
                        x.Contract.Address.Equals(ContractAddress, StringComparison.OrdinalIgnoreCase) &&
                        x.Recipient.Equals(ZeroAddress, StringComparison.OrdinalIgnoreCase))
                };
            }
            else
            {
                throw new PermanentException($"{Hash} is not a trade.");
            }
        }

        public BusinessSwap GetSwapData(IProduct product)
        {
            if (IsSwap())
            {
                var outboundSwap = GetSwap(TransferAction.Outbound);
                var inboundSwap = GetSwap(TransferAction.Inbound);
                var isInbound = inboundSwap.Contract.Address == product.Token.Address;

                var fundOp = isInbound
                    ? inboundSwap
                    : outboundSwap;

                return new BusinessSwap()
                {
                    Date = ConfirmedAt,
                    InboundSymbol = IsOutboundEtherSwap()
                        ? "ETH"
                        : inboundSwap.Contract.Symbol,
                    InboundQuantity = inboundSwap.Quantity,
                    OutboundSymbol = IsInboundEtherSwap()
                        ? "ETH"
                        : outboundSwap.Contract.Symbol,
                    OutboundQuantity = outboundSwap.Quantity,
                    IsTradeable = product.Market.IsTradeable,
                    IsInbound = isInbound,
                    Quantity = fundOp.Quantity,
                    NetCurrentPricePerToken = product.Nav.ValuePerToken,
                    NetSnapshotPricePerToken = Price.NetAssetValuePerToken,
                    MarketCurrentPricePerToken = product.Market.IsTradeable
                        ? product.Market.PricePerToken
                        : default(decimal?),
                    MarketSnapshotPricePerToken = product.Market.IsTradeable
                        ? fundOp.PricePerToken
                        : default(decimal?),
                    IsStakeLockup = false,
                    IsStakeRelease = false,
                    IsOwned = false,
                    IsBurn = false
                };
            }
            else
            {
                throw new PermanentException($"{Hash} is not a swap.");
            }
        }

        public bool IsInbound()
        {
            var lastOperation = Operations
                .Where(x => x.Type == OperationTypes.Transfer)
                .LastOrDefault();

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
            TransferAction == TransferAction.Inbound &&
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
            TransferAction == TransferAction.Inbound &&
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

        public decimal GetTransferPricePerToken(TransferAction action, IUserSettings userSettings)
        {
            var transfers = Operations
                .Where(o =>
                    o.TransferAction == action &&
                    o.Contract.Address == ContractAddress);

            var pricePerToken = default(decimal?);

            switch (action)
            {
                case TransferAction.Inbound:
                    pricePerToken = transfers
                        .LastOrDefault(x => x.Recipient.Equals(userSettings.WalletAddress, StringComparison.OrdinalIgnoreCase))
                        ?.PricePerToken;
                    break;
                case TransferAction.Outbound:
                    pricePerToken = transfers
                        .FirstOrDefault(x => x.Sender.Equals(userSettings.WalletAddress, StringComparison.OrdinalIgnoreCase))
                        ?.PricePerToken;
                    break;
            }

            if (!pricePerToken.HasValue)
            {
                pricePerToken = transfers.LastOrDefault()?.PricePerToken ?? decimal.Zero;
            }

            return pricePerToken ?? decimal.Zero;
        }

        public decimal GetTransferQuantity(TransferAction action, IUserSettings userSettings)
        {
            var transfers = Operations
                .Where(o =>
                    o.TransferAction == action &&
                    o.Contract.Address == ContractAddress);

            var pricePerToken = default(decimal?);

            switch (action)
            {
                case TransferAction.Inbound:
                    pricePerToken = transfers
                        .Where(x => x.Recipient.Equals(userSettings.WalletAddress, StringComparison.OrdinalIgnoreCase))
                        .Sum(x => x.Quantity);
                    break;
                case TransferAction.Outbound:
                    pricePerToken = transfers
                        .Where(x => x.Sender.Equals(userSettings.WalletAddress, StringComparison.OrdinalIgnoreCase))
                        .Sum(x => x.Quantity);
                    break;
            }

            if (!pricePerToken.HasValue)
            {
                pricePerToken = transfers.Sum(x => x.Quantity);
            }

            return pricePerToken ?? decimal.Zero;
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
