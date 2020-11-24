using System;

namespace Pseudonym.Crypto.Invictus.Funds.Utils
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public sealed class DynamoDbStringAttribute : Attribute
    {
    }
}
