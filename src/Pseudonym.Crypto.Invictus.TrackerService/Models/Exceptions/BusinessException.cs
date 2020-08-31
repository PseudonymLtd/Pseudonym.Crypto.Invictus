using System;
using System.Runtime.Serialization;

namespace Pseudonym.Crypto.Invictus.TrackerService.Models.Exceptions
{
    [Serializable]
    public abstract class BusinessException : Exception
    {
        protected BusinessException()
            : base()
        {
        }

        protected BusinessException(string message)
            : base(message)
        {
        }

        protected BusinessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected BusinessException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
