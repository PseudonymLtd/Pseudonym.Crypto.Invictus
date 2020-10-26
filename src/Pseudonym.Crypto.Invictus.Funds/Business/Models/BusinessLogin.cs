using System;
using System.ComponentModel.DataAnnotations;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Models
{
    public sealed class BusinessLogin
    {
        [Required]
        public string AccessToken { get; set; }

        [Required]
        public DateTime ExpiresAt { get; set; }
    }
}
