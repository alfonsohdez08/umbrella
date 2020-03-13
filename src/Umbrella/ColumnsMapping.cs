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
        private readonly ParameterSeeker _parameterSeeker = new ParameterSeeker();

        private MemberInfo _memberInScope;

        public ColumnsMapping(LambdaExpression projector)
        {
            projector = ShapeProjector(projector);

            _projectorParameter = projector.Parameters[0];
            _projection = projector.Body;
        }

        /// <summary>
        /// Modify a projector for make it understandable to the columns mapping process.
        /// </summary>
        /// <param name="projector">Projector.</param>
        /// <returns>A projector that can be used by the columns mapping process for discover the DataTable columns.</returns>
        private LambdaExpression ShapeProjector(LambdaExpression projector)
        {
            // Rewrites an projection of the form p => p into a new projection
            // that uses the new operator (explicit projection): p => new {p.UnitPrice, p.InStock}
            var implicitProjectionRewritter = new ImplicitProjectionRewritter();
            projector = (LambdaExpression)implicitProjectionRewritter.Rewrite(projector);

            // Evaluates any subtree that do not reference the projector's parameter
            var partialEvaluator = new PartialEvaluator();
            projector = (LambdaExpression)partialEvaluator.Evaluate(projector);

            // Evalatues any ColumnSettings object within the projection
            var columnSettingsEvaluator = new ColumnSettingsEvaluator();
            projector = (LambdaExpression)columnSettingsEvaluator.Evaluate(projector);
            
            // Rewrites a projection of the form p => p.Member into a new projection
            // that wraps the member access as a ColumnSettings
            var memberAccessProjRewritter = new SingleMemberProjectionRewritter();
            projector = (LambdaExpression)memberAccessProjRewritter.Rewrite(projector);

            // Checks if the projection is valid or not (if it's invalid, an exception would be
            // thrown to the client)
            var projectionValidator = new ProjectionValidator();
            projectionValidator.Validate(projector);

            // Marks as column any subtree that's not a NewExpression
            var columnExpressionsMapper = new ColumnExpressionMapper();
            projector = (LambdaExpression)columnExpressionsMapper.Map(projector);

            return projector;
        }

        /// <summary>
        /// Retrieves columns discovered/inferred.
        /// </summary>
        /// <returns>A list of columns that represent the DataTable's columns.</returns>
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
            //TODO: Check whether arguments provided correspond to an user defined constructor (an exception should be raised)

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
            Expression columnMapper = c.ColumnMapper;

            if (_memberInScope != null)
            {
                columnName = _memberInScope.Name;
                columnDataType = ((PropertyInfo)_memberInScope).PropertyType;

                _memberInScope = null;
            }
            
            if (columnMapper is ConstantExpression constantExp && constantExp.Value is ColumnSettings columnSettings)
            {
                columnMapper = ((LambdaExpression)columnSettings.Mapper).Body;
                columnName = !string.IsNullOrEmpty(columnSettings.ColumnName) ? columnSettings.ColumnName : columnName;
                columnDataType = columnSettings.ColumnDataType;
            }

            if (string.IsNullOrEmpty(columnName))
                throw new InvalidOperationException("Can't find/infer the column's name. Review your projection and ensure you either implicit or explicitily you set the column's name.");

            if (columnDataType == typeof(string))
            {
                isNullable = true;
            }
            else
            {
                Type nullableType = Nullable.GetUnderlyingType(columnDataType);
                if (nullableType != null)
                {
                    columnDataType = nullableType;
                    isNullable = true;
                }
            }

            if (!columnDataType.IsBuiltInType())
                throw new InvalidColumnDataTypeException($"The data type for the column \"{columnName}\" is invalid: {columnDataType.ToString()}.");

            LambdaExpression le = null;
            bool isMapperExprParameterless = !_parameterSeeker.Exists(columnMapper, _projectorParameter);

            if (isMapperExprParameterless)
                le = Expression.Lambda(columnMapper);
            else
                le = Expression.Lambda(columnMapper, _projectorParameter);

            var column = new Column()
            {
                Name = columnName,
                DataType = columnDataType,
                IsNullable = isNullable,
                Mapper = le.Compile(),
                IsMapperParameterless = isMapperExprParameterless
            };

            _columns.Add(column);

            return c;
        }
    }
}