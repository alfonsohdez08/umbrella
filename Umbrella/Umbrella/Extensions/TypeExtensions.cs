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
        /// <returns>True if it's an object; otherwise false.</returns>
        public static bool IsComplexType(this Type type)
        {
            //TODO: review more about reflections

            /*
                Any type that allows an object construction it's valid: it's not only a refernece type, it can be even a value type (a struct).
             */

            return !type.IsValueType && (type != typeof(string));
        }
    }
}
