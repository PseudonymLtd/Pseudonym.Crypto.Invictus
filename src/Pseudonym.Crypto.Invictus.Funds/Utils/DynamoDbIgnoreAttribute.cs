using System;

namespace Pseudonym.Crypto.Invictus.Funds.Utils
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DynamoDbIgnoreAttribute : Attribute
    {
    }
}
