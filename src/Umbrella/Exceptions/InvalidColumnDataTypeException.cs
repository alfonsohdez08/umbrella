using System;
using System.Collections.Generic;
using System.Text;

namespace Umbrella.Exceptions
{
    /// <summary>
    /// An exception that denotes that the data type for a column is invalid (a non built-in .NET data type).
    /// </summary>
    public class InvalidColumnDataTypeException: Exception
    {

        public InvalidColumnDataTypeException(string message): base(message)
        {

        }
    }
}
