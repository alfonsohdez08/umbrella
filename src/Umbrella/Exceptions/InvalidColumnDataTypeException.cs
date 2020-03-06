using System;
using System.Collections.Generic;
using System.Text;

namespace Umbrella.Exceptions
{
    public class InvalidColumnDataTypeException: Exception
    {

        public InvalidColumnDataTypeException(string message): base(message)
        {

        }
    }
}
