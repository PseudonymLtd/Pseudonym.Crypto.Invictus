﻿using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Pseudonym.Crypto.Invictus.Shared.Models
{
    public sealed class ApiLogin
    {
        [Required]
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [Required]
        [JsonProperty("expires_at")]
        public DateTime ExpiresAt { get; set; }
    }
}
