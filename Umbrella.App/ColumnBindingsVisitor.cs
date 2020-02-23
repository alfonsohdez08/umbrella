using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Umbrella.App
{
    public class ColumnBindingsVisitor: ExpressionVisitor
    {
        private readonly ColumnCandidates _columnCandidates;
        private readonly List<PropertyInfo> _properties;

        public Dictionary<DataColumn, Delegate> Bindings { get; private set; } = new Dictionary<DataColumn, Delegate>();

        private ColumnBindingsVisitor(List<PropertyInfo> properties, ColumnCandidates columnCandidates)
        {
            _properties = properties;
            _columnCandidates = columnCandidates;
        }

        public static Dictionary<DataColumn, Delegate> GetColumnBindings(Expression projector, List<PropertyInfo> properties, ColumnCandidates columnCandidates)
        {
            var columnBindingsVisitor = new ColumnBindingsVisitor(properties, columnCandidates);
            columnBindingsVisitor.Visit(projector);

            return columnBindingsVisitor.Bindings;
        }

        public override Expression Visit(Expression node)
        {
            if (node == null)
                return node;

            if (_columnCandidates.Candidates.Contains(node))
            {
                BindColumn(node);

                return node;
            }

            return base.Visit(node);
        }

        private void BindColumn(Expression expression)
        {
            LambdaExpression lambdaExp = Expression.Lambda(expression, _columnCandidates.Parameter);

            PropertyInfo property = _properties[Bindings.Count];
            var column = new DataColumn(property.Name, property.PropertyType);

            Bindings.Add(column, lambdaExp.Compile());
        }
    }
}
