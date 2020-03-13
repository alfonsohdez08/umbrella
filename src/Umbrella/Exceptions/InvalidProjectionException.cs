using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Umbrella.Exceptions
{
    /// <summary>
    /// An exception that denotes that the projection failed one of the validation.
    /// </summary>
    public class InvalidProjectionException: Exception
    {
        public Expression Projector { get; private set; }

        public InvalidProjectionException(string message, Expression projector): base(message)
        {
            Projector = projector;
        }
    }
}
