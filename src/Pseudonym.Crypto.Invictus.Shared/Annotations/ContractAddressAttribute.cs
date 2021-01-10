namespace System.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class ContractAddressAttribute : Attribute
    {
        public ContractAddressAttribute(string address)
        {
            Address = address.ToLower();
        }

        public string Address { get; }
    }
}