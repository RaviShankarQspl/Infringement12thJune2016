using System;
using System.Runtime.Serialization;

namespace InfringementWeb.Helpers
{
    [Serializable]
    internal class InvalidOfficerCodeException : Exception
    {
        public InvalidOfficerCodeException()
        {
        }

        public InvalidOfficerCodeException(string message) : base(message)
        {
        }

        public InvalidOfficerCodeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidOfficerCodeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}