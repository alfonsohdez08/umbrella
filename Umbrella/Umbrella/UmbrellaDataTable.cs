using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Umbrella
{
    internal class UmbrellaDataTable<T>
    {
        private readonly Expression _expression;
        private readonly IEnumerable<T> _source;

        private UmbrellaDataTable(IEnumerable<T> source, Expression expression)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            _source = source;
            _expression = expression;

        }

        /// <summary>
        /// Constructs a DataTable with the <paramref name="source"/>'s data and the columns encountered in the <paramref name="projector"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type that contains the collection.</typeparam>
        /// <param name="source">Source collection.</param>
        /// <param name="projector">Columns projector.</param>
        /// <returns>A filled DataTable.</returns>
        public static DataTable Build<TEntity>(IEnumerable<TEntity> source, Expression projector)
        {
            //TODO: am i able to rewritte an expression?
            /*
                assume p is an anonymous type that has the following properties: Id, Desc
                p => p is equivalent to p => new {p.Id, p.Desc}
             */

            projector = new ProjectionParamRewritter().Rewrite(projector);
            var umbrellaDataTable = new UmbrellaDataTable<TEntity>(source, projector);

            return umbrellaDataTable.GetDataTable();
        }

        private class ProjectionParamRewritter: ExpressionVisitor
        {

            public Expression Rewrite(Expression projector)
            {
                Expression body = projector.GetLambdaExpressionBody();

                var parameterExp = body as ParameterExpression;
                if (parameterExp != null)
                    return Visit(parameterExp);

                return projector;
            }

            // i must ensure that i only receive new{}, new(){}, p (when comes from an object type), and p.Member
            protected override Expression VisitParameter(ParameterExpression p)
            {
                Type type = p.Type;
                if (type.IsAnonymousType())
                {
                    PropertyInfo[] properties = type.GetProperties();
                    Expression[] arguments = new Expression[properties.Length];
                    Type[] propertyTypes = new Type[properties.Length];

                    for (int index = 0; index < propertyTypes.Length; index++)
                    {
                        propertyTypes[index] = properties[index].PropertyType;
                        arguments[index] = Expression.MakeMemberAccess(p, properties[index]);
                    }
                    ConstructorInfo constructor = type.GetConstructor(propertyTypes);

                    NewExpression ne = Expression.New(constructor, arguments, properties);

                    return ne;
                }
                else if (type.IsClass)
                {
                    //static type
                    ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
                    NewExpression ne = Expression.New(constructor);

                    PropertyInfo[] properties = type.GetProperties();
                    List<MemberBinding> memberBindings = new List<MemberBinding>();

                    for (int index = 0; index < properties.Length; index++)
                    {
                        MemberExpression me = Expression.MakeMemberAccess(p, properties[index]);
                        MemberBinding mb = Expression.Bind(properties[index], me);

                        memberBindings.Add(mb);
                    }

                    MemberInitExpression mi = Expression.MemberInit(ne, memberBindings);

                    return mi;
                }

                return p;
            }
        }

        private DataTable GetDataTable()
        {
            Dictionary<DataColumn, Delegate> bindings = ColumnsMapping.GetColumns(_expression);

            var dataTable = new DataTable();
            foreach (DataColumn c in bindings.Keys)
                dataTable.Columns.Add(c);

            foreach (T data in _source)
            {
                DataRow row = dataTable.NewRow();
                foreach (var b in bindings)
                    row[b.Key] = b.Value.DynamicInvoke(data);

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }
    }

    public static class TypeExtension
    {

        public static bool IsAnonymousType(this Type type)
        {
            Boolean hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Count() > 0;
            Boolean nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            Boolean isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

            return isAnonymousType;
        }
    }
}
