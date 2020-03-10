using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Umbrella
{
    class AnonymousType
    {
        private const string AssemblyName = "Umbrella";

        private readonly ModuleBuilder _moduleBuilder;

        private TypeBuilder _typeBuilder = null;

        public AnonymousType()
        {
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(AssemblyName), AssemblyBuilderAccess.Run);
            _moduleBuilder = assemblyBuilder.DefineDynamicModule(AssemblyName);
        }

        public static Type Create(Dictionary<string, Type> typeDefinition)
        {
            if (typeDefinition == null)
                throw new ArgumentNullException(nameof(typeDefinition));

            var anonymousType = new AnonymousType();

            return anonymousType.Generate(typeDefinition);
        }

        private Type Generate(Dictionary<string, Type> typeDefinition)
        {
            Type anonymousType = null;

            try
            {
                _typeBuilder = _moduleBuilder.DefineType(
                    "AnnonymousType",
                    TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass |
                    TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit, typeof(object)
                );

                var propertyNames = typeDefinition.Select(td => td.Key);

                var genericTypeParametersName = propertyNames.Select(p => string.Format("T{0}", p));

                GenericTypeParameterBuilder[] typeParameterBuilders = _typeBuilder.DefineGenericParameters(
                    genericTypeParametersName.ToArray()
                );

                var propertiesDefinition = propertyNames.Zip(typeParameterBuilders, (property, typeParameter) => (property, typeParameter));
                
                DefineProperties(propertiesDefinition.ToList(), out var fields);

                var constructorDefinition = propertyNames.Zip(fields, (property, fieldBuilder) => (property, fieldBuilder));
                DefineConstructor(constructorDefinition.ToList());

                anonymousType = _typeBuilder.CreateType().MakeGenericType(typeDefinition.Select(t => t.Value).ToArray());
            }
            finally
            {
                _typeBuilder = null;
            }

            return anonymousType;
        }

        private void DefineConstructor(List<(string, FieldBuilder)> fields)
        {
            ConstructorBuilder constructorBuilder = _typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.HasThis,
                fields.Select(f => f.Item2.FieldType).ToArray()
            );
            
            for (int index = 0; index < fields.Count; index++)
            {
                constructorBuilder.DefineParameter(
                    index + 1, 
                    ParameterAttributes.None,
                    GenerateParameterName(fields[index].Item1)
                );
            }

            ILGenerator constructorILGenerator = constructorBuilder.GetILGenerator();

            constructorILGenerator.Emit(OpCodes.Ldarg_0);
            constructorILGenerator.Emit(OpCodes.Call, typeof(object).GetConstructors()[0]); //Calls the object's constructor

            for (int index = 0; index < fields.Count; index++)
            {
                constructorILGenerator.Emit(OpCodes.Ldarg_0);

                constructorILGenerator.Emit(OpCodes.Ldarg, index + 1);
                constructorILGenerator.Emit(OpCodes.Stfld, fields[index].Item2);
            }

            constructorILGenerator.Emit(OpCodes.Ret);
        }

        private void DefineProperties(List<(string, GenericTypeParameterBuilder)> propertyDefinitions, out List<FieldBuilder> fields)
        {
            fields = new List<FieldBuilder>();

            foreach (var propertyDefinition in propertyDefinitions)
            {
                Type returnType = propertyDefinition.Item2;

                FieldBuilder fieldBuilder = _typeBuilder.DefineField(
                    GenerateFieldName(propertyDefinition.Item1),
                    returnType,
                    FieldAttributes.Private | FieldAttributes.InitOnly
                );
                fields.Add(fieldBuilder);

                PropertyBuilder propertyBuilder = _typeBuilder.DefineProperty(
                    propertyDefinition.Item1,
                    PropertyAttributes.None,
                    CallingConventions.HasThis,
                    returnType,
                    Type.EmptyTypes
                );

                MethodBuilder getMethodBuilder = _typeBuilder.DefineMethod(
                    string.Format("get{0}", propertyDefinition.Item1),
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
                    CallingConventions.HasThis,
                    returnType,
                    Type.EmptyTypes
                );

                ILGenerator getMethodILGenerator = getMethodBuilder.GetILGenerator();
                
                getMethodILGenerator.Emit(OpCodes.Ldarg_0); // Loads "this" pointer into stack
                getMethodILGenerator.Emit(OpCodes.Ldfld, fieldBuilder); // Uses the "this" pointer already in the stack to get the "field"
                getMethodILGenerator.Emit(OpCodes.Ret); // Returns the value allocated in the evaluation stack (in this case, the result of the "Ldfld" operator)

                propertyBuilder.SetGetMethod(getMethodBuilder);
            }
        }

        private static string GenerateFieldName(string input) => string.Format("_{0}", input);

        private static string GenerateParameterName(string input) => string.Format("p_{0}", input);
    }
}
