using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Umbrella.Extensions
{
    internal static class TypeExtension
    {
        /// <summary>
        /// Determines whether a type is an anonymous one or not.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <returns>True if it's an anonymous type; otherwise false.</returns>
        public static bool IsAnonymousType(this Type type)
        {
            // Source: https://stackoverflow.com/questions/1650681/determining-whether-a-type-is-an-anonymous-type
            // is there heuristic way to determine whether a type is an anonymous type or not?

            bool hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Length > 0;
            bool nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            
            return hasCompilerGeneratedAttribute && nameContainsAnonymousType;
        }

        /// <summary>
        /// Determines whether a given type is a built-in type from .NET framework.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <returns>True if it's a built-in type; otherwise false.</returns>
        public static bool IsBuiltInType(this Type type)
        {
            //Primitive types in .NET: https://docs.microsoft.com/en-us/dotnet/api/system.type.isprimitive?view=netstandard-2.0

            return type.IsPrimitive || type == typeof(decimal) ||  type == typeof(string) || type == typeof(DateTime);
        }

        /// <summary>
        /// Determines whether a given type is a struct or not.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <returns>True if it's a struct; otherwise false.</returns>
        public static bool IsStruct(this Type type)
        {
            return type.IsValueType && !type.IsPrimitive && (type != typeof(DateTime));
        }
    }
}