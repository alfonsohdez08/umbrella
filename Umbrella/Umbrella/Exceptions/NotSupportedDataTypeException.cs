using System;
using System.Collections.Generic;
using System.Text;

namespace Umbrella.Exceptions
{
    public class NotSupportedDataTypeException: Exception
    {
        public Type InvalidType { get; private set; }

        public NotSupportedDataTypeException(Type type, string message = null) : base(message)
        {
            InvalidType = type;
        }

        public NotSupportedDataTypeException(Type type, string message, Exception innerException) : base(message, innerException)
        {
            InvalidType = type;
        }
    }
}
