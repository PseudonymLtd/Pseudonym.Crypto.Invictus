namespace System.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class EthereumAddressAttribute : RegularExpressionAttribute
    {
        public EthereumAddressAttribute()
            : base("^0x+[A-F,a-f,0-9]{40}$")
        {
        }
    }
}
