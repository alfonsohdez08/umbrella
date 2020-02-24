using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Umbrella
{
    internal static class ColumnsMapping
    {
        private class ColumnsNominator: ExpressionVisitor
        {
            private readonly Expression _expression;

            private readonly List<Expression> _expressions;
            private bool _isPartOfColumn = true;

            public ColumnsNominator(Expression expression)
            {
                _expression = expression.GetLambdaExpressionBody();
                _expressions = new List<Expression>();
            }

            public void Nominate()
            {
                Visit(_expression);
            }

            public static List<Expression> GetColumnCandidates(Expression projector)
            {
                var columnsNominator = new ColumnsNominator(projector);
                columnsNominator.Nominate();

                return columnsNominator._expressions;
            }

            public override Expression Visit(Expression node)
            {
                if (node == null)
                    return node;

                bool saveIsPartOfColumn = _isPartOfColumn;
                _isPartOfColumn = true;

                base.Visit(node);

                if (node.IsObjectInstantiation())
                    _isPartOfColumn = false;
                else
                    _expressions.Add(node);

                _isPartOfColumn &= saveIsPartOfColumn;

                return node;
            }
        }

        private class ColumnsReplacer: ExpressionVisitor
        {
            private readonly List<Expression> _candidates;
            private readonly Expression _expression;

            public ColumnsReplacer(Expression expression, List<Expression> candidates)
            {
                //_expression = expression.GetLambdaExpressionBody();
                _expression = expression;
                _candidates = candidates;
            }

            public static Expression Replace(Expression projector, List<Expression> candidates)
            {
                var lambdaExp = (LambdaExpression)projector;

                var columnsReplacer = new ColumnsReplacer(lambdaExp.Body, candidates);
                Expression e = columnsReplacer.Replace();

                return Expression.Lambda(e, lambdaExp.Parameters);
            }

            public Expression Replace()
            {
                return Visit(_expression);
            }

            public override Expression Visit(Expression node)
            {
                if (node == null)
                    return node;

                if (_candidates.Contains(node))
                {
                    var columnExpression = new ColumnExpression(node);

                    return columnExpression;
                }

                return base.Visit(node);
            }
        }

        private class ColumnsMappedVisitor: DataTableVisitor
        {
            public Dictionary<DataColumn, Delegate> Columns { get; private set; } = new Dictionary<DataColumn, Delegate>();

            private MemberInfo _memberInScope;
            private readonly ParameterExpression _parameterExp;
            private readonly Expression _projector;

            public ColumnsMappedVisitor(Expression projector)
            {
                var lambdaExp = (LambdaExpression)projector;

                _parameterExp = lambdaExp.Parameters[0];
                _projector = lambdaExp.Body;
            }

            public static Dictionary<DataColumn, Delegate> GetMappedColumns(Expression projector)
            {
                var columnsMappedVisitor = new ColumnsMappedVisitor(projector);
                columnsMappedVisitor.SetColumns();

                return columnsMappedVisitor.Columns;
            }

            public void SetColumns()
            {
                Visit(_projector);
            }

            protected override MemberAssignment VisitMemberAssignment(MemberAssignment ma)
            {
                _memberInScope = ma.Member;

                return base.VisitMemberAssignment(ma);
            }

            protected override Expression VisitNew(NewExpression ne)
            {
                for (int index = 0; index < ne.Arguments.Count; index++)
                {
                    _memberInScope = ne.Members[index];
                    Visit(ne.Arguments[index]);
                }

                return ne;
            }

            protected override Expression VisitColumn(ColumnExpression c)
            {
                (string, Type) column = (string.Empty, null);

                if (_memberInScope != null)
                {
                    column.Item1 = _memberInScope.Name;
                    column.Item2 = ((PropertyInfo)_memberInScope).PropertyType;

                    _memberInScope = null;
                }
                else
                {
                    var m = c.ColumnDefinition as MemberExpression;
                    if (m == null)
                        throw new NotSupportedException($"Can not understand this projector's part: {c.ColumnDefinition.ToString()}");

                    column.Item1 = m.Member.Name;
                    column.Item2 = ((PropertyInfo)m.Member).PropertyType;
                }

                DataColumn dataColumn = new DataColumn(column.Item1, column.Item2); 
                LambdaExpression le = Expression.Lambda(c.ColumnDefinition, _parameterExp);

                Columns.Add(dataColumn, le.Compile());

                return c;
            }
        }

        /// <summary>
        /// Retrieves the columns identified given a projector.
        /// </summary>
        /// <param name="projector">Defines the columns that would have the DataTable.</param>
        /// <returns>A mapping set of System.Data.DataColumn and Delegate.</returns>
        /// <remarks>
        /// The Delegate represents the function required in order to fetch the data for in a specific column.
        /// </remarks>
        public static Dictionary<DataColumn, Delegate> GetColumns(Expression projector)
        {
            List<Expression> candidates = ColumnsNominator.GetColumnCandidates(projector);
            projector = ColumnsReplacer.Replace(projector, candidates);

            return ColumnsMappedVisitor.GetMappedColumns(projector);
        }
    }

    internal static class ExpressionExtensions
    {
        public static bool IsObjectInstantiation(this Expression e)
        {
            return e.NodeType == ExpressionType.New || e.NodeType == ExpressionType.MemberInit
                || e.NodeType == ExpressionType.NewArrayInit || e.NodeType == ExpressionType.ListInit;
        }

        public static Expression GetLambdaExpressionBody(this Expression e)
        {
            var le = e as LambdaExpression;
            if (le == null)
                throw new InvalidCastException("The underlying expression is not a LambdaExpression.");

            return le.Body;
        }
    }

    public class DataTableVisitor : ExpressionVisitor
    {
        public override Expression Visit(Expression node)
        {
            if (node == null)
                return null;

            switch ((DataTableExpression)node.NodeType)
            {
                case DataTableExpression.Column:
                    return VisitColumn((ColumnExpression)node);
                default:
                    return base.Visit(node);
            }
        }

        protected virtual Expression VisitColumn(ColumnExpression c)
        {
            Expression columnDefinition = Visit(c.ColumnDefinition);
            if (c.ColumnDefinition != columnDefinition)
                return new ColumnExpression(columnDefinition);

            return c;
        }
    }

    public class ColumnExpression : Expression
    {
        private readonly Type _type;
        public Expression ColumnDefinition { get; private set; }
        public override Type Type => _type;
        public override ExpressionType NodeType => (ExpressionType)DataTableExpression.Column;

        public ColumnExpression(Expression columnDefinition)
        {
            _type = columnDefinition.Type;

            ColumnDefinition = columnDefinition;
        }

    }

    public enum DataTableExpression
    {
        Column = 1000
    }
}
