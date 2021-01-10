namespace System.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class StakeAttribute : Attribute
    {
        public StakeAttribute()
        {
        }
    }
}
