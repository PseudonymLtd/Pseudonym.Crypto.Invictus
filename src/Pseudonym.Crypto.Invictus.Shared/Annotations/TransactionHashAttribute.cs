namespace System.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class TransactionHashAttribute : RegularExpressionAttribute
    {
        public TransactionHashAttribute()
            : base("^0x+[A-F,a-f,0-9]{64}$")
        {
        }
    }
}