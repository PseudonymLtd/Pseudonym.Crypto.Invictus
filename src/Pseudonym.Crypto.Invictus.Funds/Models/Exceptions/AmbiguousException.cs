using System;
using System.Runtime.Serialization;

namespace Pseudonym.Crypto.Invictus.Funds.Models.Exceptions
{
    [Serializable]
    public class AmbiguousException : BusinessException
    {
        public AmbiguousException()
            : base()
        {
        }

        public AmbiguousException(string message)
            : base(message)
        {
        }

        public AmbiguousException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected AmbiguousException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
