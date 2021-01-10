using System;
using Pseudonym.Crypto.Invictus.Funds.Configuration.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Configuration
{
    public sealed class HoldingSettings : IHoldingSettings
    {
        public string Symbol { get; set; }

        public string Colour { get; set; }

        public string Link { get; set; }

        public string ImageLink { get; set; }

        Uri IHoldingSettings.Link => new Uri(Link, UriKind.Absolute);

        Uri IHoldingSettings.ImageLink => new Uri(ImageLink, UriKind.Absolute);
    }
}
