using System;
using System.Runtime.Serialization;

namespace Pseudonym.Crypto.Invictus.Shared.Exceptions
{
    [Serializable]
    public class PermanentException : BusinessException
    {
        public PermanentException()
            : base()
        {
        }

        public PermanentException(string message)
            : base(message)
        {
        }

        public PermanentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected PermanentException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
