using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

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

        public Type CreateType(Dictionary<string, Type> properties)
        {
            TypeBuilder typeBuilder = _moduleBuilder.DefineType("AnnonymousType", TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class);

            var genericTypeParametersName = properties.Select(p => p.Key);

            // Define Generic Type Parameters
            var genericTypeParameters = typeBuilder.DefineGenericParameters(genericTypeParametersName.ToArray());
            //for (int index = 0; index < genericTypeParameters.Length; index++)
            //{
            //    GenericTypeParameterBuilder genericTypeParameterBuilder = genericTypeParameters[index];
            //}

            // Define fields
            foreach (var property in properties)
            {
                FieldBuilder fieldBuilder = typeBuilder.DefineField(property.Key, property.Value, FieldAttributes.Private);


            }

            throw new NotImplementedException();
        }
    }
}
