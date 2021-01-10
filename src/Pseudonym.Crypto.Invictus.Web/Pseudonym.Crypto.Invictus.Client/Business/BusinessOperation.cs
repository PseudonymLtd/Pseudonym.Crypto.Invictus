using System.ComponentModel.DataAnnotations;
using Pseudonym.Crypto.Invictus.Shared.Enums;
using Pseudonym.Crypto.Invictus.Shared.Models;

namespace Pseudonym.Crypto.Invictus.Web.Client.Business
{
    public sealed class BusinessOperation
    {
        [Required]
        public string Type { get; set; }

        [Required]
        public string Value { get; set; }

        [Required]
        public decimal PricePerToken { get; set; }

        public decimal Quantity { get; set; }

        public decimal Total => PricePerToken * Quantity;

        [Required]
        public bool IsEth { get; set; }

        [Required]
        public int Priority { get; set; }

        public string Sender { get; set; }

        public string Recipient { get; set; }

        public string Address { get; set; }

        [Required]
        public TransferAction TransferAction { get; set; }

        [Required]
        public BusinessContract Contract { get; set; }
    }
}
