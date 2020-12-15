namespace Pseudonym.Crypto.Invictus.Web.Client.Business
{
    public sealed class BusinessSwap : BusinessTrade
    {
        public string InboundSymbol { get; set; }

        public string OutboundSymbol { get; set; }

        public decimal InboundQuantity { get; set; }

        public decimal OutboundQuantity { get; set; }
    }
}
