using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Reflection.Emit;
using System.Linq;

namespace Umbrella
{
    internal class AnonymousType
    {
        private const string AssemblyName = "Umbrella";
        private readonly ModuleBuilder _moduleBuilder;

        public AnonymousType()
        {
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(AssemblyName), AssemblyBuilderAccess.Run);
            
            _moduleBuilder = assemblyBuilder.DefineDynamicModule(AssemblyName);
        }

        /// <summary>
        /// Creates an anonymous type.
        /// </summary>
        /// <param name="properties">The properties that would have the type.</param>
        /// <returns>An anonymous type.</returns>
        public static Type CreateAnonymousType(Dictionary<string, Type> properties)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            if (properties.Count == 0)
                throw new ArgumentException(nameof(properties));

            var anonymousType = new AnonymousType();

            return anonymousType.CreateType(properties);
        }

        public Type CreateType(Dictionary<string, Type> properties)
        {
            TypeBuilder typeBuilder = _moduleBuilder.DefineType(GenerateAnonymousTypeName(), TypeAttributes.Public | TypeAttributes.AutoLayout
                | TypeAttributes.AnsiClass | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit, null);

            // i have to generate the constructor
            Type[] types = properties.Values.ToArray();
            ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, types);

            foreach (KeyValuePair<string, Type> p in properties)
            {
                PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(p.Key, PropertyAttributes.None, p.Value, null);
                
                
            }

            return typeBuilder.CreateType();
        }

        private static string GenerateAnonymousTypeName() => "AnonymousType";
    }
}
