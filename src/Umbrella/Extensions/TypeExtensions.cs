using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Umbrella.Extensions
{
    public static class TypeExtension
    {

        /// <summary>
        /// src: copied from Stackoverflow
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsAnonymousType(this Type type)
        {
            bool hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Length > 0;
            bool nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            
            return hasCompilerGeneratedAttribute && nameContainsAnonymousType;
        }

        /// <summary>
        /// Determines whether a given type is a complex structure or not.
        /// </summary>
        /// <param name="type">Type that would be evaluated.</param>
        /// <returns>True if it's a complex type; otherwise false.</returns>
        /// <remarks>
        /// A complex type is either a struct, an user defined type or an anonymous type. It's basically an object that has state and properties.
        /// </remarks>
        public static bool IsComplexType(this Type type)
        {
            bool isStruct = !type.IsPrimitive && type.IsValueType;
            bool isReferenceType = !type.IsValueType && (type != typeof(string) || type != typeof(DateTime));

            return isStruct || isReferenceType;
        }

        /// <summary>
        /// Determines whether a given type is a built-in type from .NET framework.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <returns>True if it's a built-in type; otherwise false.</returns>
        public static bool IsBuiltInType(this Type type)
        {
            return type.IsPrimitive || type == typeof(string) || type == typeof(DateTime);
        }

    }
}
