using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Umbrella.Exceptions;
using Umbrella.Expr;
using Umbrella.Expr.Column;
using Umbrella.Expr.Evaluators;
using Umbrella.Expr.Projection;
using Umbrella.Expr.Rewritters;
using Umbrella.Extensions;

namespace Umbrella
{
    internal class ColumnsMapping: ColumnVisitor
    {
        private List<Column> _columns = new List<Column>();
        private readonly ParameterExpression _projectorParameter;
        private readonly Expression _projection;
        private readonly ParameterReferencesFinder _parameterFinder = new ParameterReferencesFinder();

        private MemberInfo _memberInScope;

        public ColumnsMapping(LambdaExpression projector)
        {
            projector = ShapeProjector(projector);

            _projectorParameter = projector.Parameters[0];
            _projection = projector.Body;
        }

        private LambdaExpression ShapeProjector(LambdaExpression projector)
        {
            var implicitProjectionRewritter = new ImplicitProjectionRewritter();
            projector = (LambdaExpression)implicitProjectionRewritter.Rewrite(projector);

            var partialEvaluator = new PartialEvaluator();
            projector = (LambdaExpression)partialEvaluator.Evaluate(projector);

            var columnSettingsEvaluator = new ColumnSettingsEvaluator();
            projector = (LambdaExpression)columnSettingsEvaluator.Evaluate(projector);

            // add here the member projection rewritter

            var projectionValidator = new ProjectionValidator();
            projectionValidator.Validate(projector);

            var columnExpressionsMapper = new ColumnExpressionMapper();
            projector = (LambdaExpression)columnExpressionsMapper.Map(projector);

            return projector;
        }

        public List<Column> GetColumns()
        {
            List<Column> columns = null;

            try
            {
                Visit(_projection);
                columns = _columns;
            }
            finally
            {
                _columns = null;
            }

            return columns;
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment ma)
        {
            _memberInScope = ma.Member;

            return base.VisitMemberAssignment(ma);
        }

        protected override Expression VisitNew(NewExpression ne)
        {
            //TODO: Check whether when arguments are provided to an user defined constructor (For raise an exception)
            for (int index = 0; index < ne.Arguments.Count; index++)
            {
                _memberInScope = ne.Members[index];
                Visit(ne.Arguments[index]);
            }

            return ne;
        }

        protected override Expression VisitColumn(ColumnExpression c)
        {
            string columnName = string.Empty;
            Type columnDataType = null;
            bool isNullable = false;

            if (_memberInScope != null)
            {
                columnName = _memberInScope.Name;
                columnDataType = ((PropertyInfo)_memberInScope).PropertyType;

                _memberInScope = null;
            }

            Type nullableType = Nullable.GetUnderlyingType(columnDataType);
            if (nullableType != null)
            {
                columnDataType = nullableType;
                isNullable = true;
            }

            Expression columnDefinition = c.ColumnDefinition;
            if (columnDefinition is ConstantExpression constantExp && constantExp.Value is ColumnSettings columnSettings)
            {
                columnDefinition = ((LambdaExpression)columnSettings.Mapper).Body;
                columnName = !string.IsNullOrEmpty(columnSettings.ColumnName) ? columnSettings.ColumnName : columnName;
                columnDataType = columnSettings.ColumnDataType;
            }

            if (!columnDataType.IsBuiltInType())
                throw new InvalidColumnDataTypeException($"The data type for the column \"{columnName}\" is invalid: {columnDataType.ToString()}.");

            LambdaExpression le = null;
            bool isParameterless = !_parameterFinder.Find(columnDefinition, _projectorParameter);

            if (isParameterless)
                le = Expression.Lambda(columnDefinition);
            else
                le = Expression.Lambda(columnDefinition, _projectorParameter);

            var column = new Column()
            {
                Name = columnName,
                DataType = columnDataType,
                IsNullable = isNullable,
                Mapper = le.Compile(),
                IsParameterless = isParameterless
            };

            _columns.Add(column);

            return c;
        }
    }
}