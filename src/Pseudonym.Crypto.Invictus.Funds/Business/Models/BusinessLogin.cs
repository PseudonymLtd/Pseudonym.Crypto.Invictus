using System;
using System.ComponentModel.DataAnnotations;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Models
{
    internal sealed class BusinessLogin : ILogin
    {
        [Required]
        public string AccessToken { get; set; }

        [Required]
        public DateTime ExpiresAt { get; set; }
    }
}
