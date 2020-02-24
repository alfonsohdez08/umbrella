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
                _expression = expression.GetLambdaExpressionBody();
                _candidates = candidates;
            }

            public void Replace()
            {
                Visit(_expression);
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

        public class ColumnExpression: Expression
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

        //private class ProjectionVisitor : ExpressionVisitor
        //{
        //    private readonly Dictionary<DataColumn, Delegate> _bindings;

        //    private readonly ParameterExpression _parameterExp;
        //    private readonly Expression _expression;

        //    public ProjectionVisitor(Expression expression)
        //    {
        //        _bindings = new Dictionary<DataColumn, Delegate>();

        //        var lambdaExp = (LambdaExpression)expression;

        //        _parameterExp = lambdaExp.Parameters[0];
        //        _expression = lambdaExp.Body;
        //    }

        //    public Dictionary<DataColumn, Delegate> GetBindings()
        //    {
        //        Visit(_expression);

        //        return _bindings;
        //    }

        //    protected override Expression VisitParameter(ParameterExpression p)
        //    {
        //        MemberInfo[] members = p.Type.GetProperties();
        //        Expression[] expressions = new Expression[members.Length];

        //        for (int index = 0; index < members.Length; index++)
        //            expressions[index] = Expression.MakeMemberAccess(p, members[index]);

        //        MapColumns(members, expressions);

        //        return p;
        //    }

        //    protected override Expression VisitMemberInit(MemberInitExpression m)
        //    {
        //        Expression[] expressions = new Expression[m.Bindings.Count];
        //        MemberInfo[] members = new MemberInfo[m.Bindings.Count];

        //        int count = 0;
        //        foreach (MemberBinding mb in m.Bindings)
        //        {
        //            MemberAssignment ma = mb as MemberAssignment;
        //            if (ma == null)
        //            {
        //                var invalidOpException = new InvalidOperationException("The member initialization only supports member assignment.");
        //                invalidOpException.HelpLink = "https://docs.microsoft.com/en-us/dotnet/api/system.linq.expressions.memberassignment?view=netstandard-2.0";

        //                throw invalidOpException;
        //            }

        //            members[count] = ma.Member;
        //            expressions[count] = ma.Expression;

        //            count++;
        //        }

        //        MapColumns(members, expressions);

        //        return m;
        //    }

        //    protected override Expression VisitNew(NewExpression n)
        //    {
        //        Expression[] expressions = new Expression[n.Arguments.Count];
        //        n.Arguments.CopyTo(expressions, 0);

        //        MemberInfo[] members = new MemberInfo[n.Members.Count];
        //        n.Members.CopyTo(members, 0);

        //        MapColumns(members, expressions);

        //        return n;
        //    }

        //    private void MapColumns(MemberInfo[] members, Expression[] expressions)
        //    {
        //        CheckIfHasNestedObjInstantiator(expressions);

        //        for (int index = 0; index < members.Length; index++)
        //        {
        //            PropertyInfo property = (PropertyInfo)members[index];
        //            Expression expression = expressions[index];

        //            LambdaExpression lambdaExp = Expression.Lambda(expression, _parameterExp);
        //            var column = new DataColumn(property.Name, property.PropertyType);

        //            _bindings.Add(column, lambdaExp.Compile());
        //        }
        //    }

        //    /// <summary>
        //    /// Checks for nested object instatiator (expressions that denotes object instatiation).
        //    /// </summary>
        //    /// <param name="expressions">Set of expressions (immediate node expressions).</param>
        //    private void CheckIfHasNestedObjInstantiator(Expression[] expressions)
        //    {
        //        for (int index = 0; index < expressions.Length; index++)
        //            if (expressions[index].IsObjectInstantiator())
        //                throw new InvalidOperationException("The projector can not have nested object instantiator.");
        //    }
        //}

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
            var projectionVisitor = new ColumnsNominator(projector);

            return null;
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
}
