using System;
using System.ComponentModel.DataAnnotations;

namespace Pseudonym.Crypto.Invictus.Web.Client.Business
{
    public sealed class BusinessContract
    {
        [Required]
        public string Address { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Symbol { get; set; }

        [Required]
        public int Decimals { get; set; }

        [Required]
        public long Holders { get; set; }

        [Required]
        public long Issuances { get; set; }

        public Uri Link { get; set; }
    }
}
