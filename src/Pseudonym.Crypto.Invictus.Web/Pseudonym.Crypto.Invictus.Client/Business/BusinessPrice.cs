using System.ComponentModel.DataAnnotations;

namespace Pseudonym.Crypto.Invictus.Web.Client.Business
{
    public sealed class BusinessPrice
    {
        [Required]
        public decimal NetAssetValuePerToken { get; set; }

        [Required]
        public decimal? MarketValuePerToken { get; set; }
    }
}
