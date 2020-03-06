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
        /// Determines whether a given type is a built-in type from .NET framework.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <returns>True if it's a built-in type; otherwise false.</returns>
        public static bool IsBuiltInType(this Type type)
        {
            return type.IsPrimitive || type == typeof(string) || type == typeof(DateTime);
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
