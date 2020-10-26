using System;
using System.Runtime.Serialization;

namespace Pseudonym.Crypto.Invictus.Shared.Exceptions
{
    [Serializable]
    public class TransientException : BusinessException
    {
        public TransientException()
            : base()
        {
        }

        public TransientException(string message)
            : base(message)
        {
        }

        public TransientException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected TransientException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
