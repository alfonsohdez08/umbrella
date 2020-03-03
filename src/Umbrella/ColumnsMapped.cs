using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Umbrella.Expr;
using Umbrella.Expr.Column;

namespace Umbrella
{
    public class ColumnsMapped: ColumnVisitor
    {
        private readonly List<Column> _columns = new List<Column>();
        private readonly ParameterExpression _parameter;
        private readonly Expression _projectorBody;
        private readonly ParameterReferencesFinder _parameterFinder = new ParameterReferencesFinder();

        private MemberInfo _memberInScope;

        public ColumnsMapped(Expression projector)
        {
            var lambdaExp = (LambdaExpression)projector;

            _parameter = lambdaExp.Parameters[0];
            _projectorBody = lambdaExp.Body;
        }

        public List<Column> GetColumns()
        {
            Visit(_projectorBody);

            return _columns;
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
            string columnName = string.Empty;
            Type columnDataType = null;
            bool isNullable = false;

            if (_memberInScope != null)
            {
                columnName = _memberInScope.Name;
                columnDataType = ((PropertyInfo)_memberInScope).PropertyType;

                _memberInScope = null;
            }
            else
            {
                var m = c.ColumnDefinition as MemberExpression;
                if (m == null)
                    throw new NotSupportedException($"Can not understand this projector's part: {c.ColumnDefinition.ToString()}");

                columnName = m.Member.Name;
                columnDataType = ((PropertyInfo)m.Member).PropertyType;
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

            LambdaExpression le = null;
            bool isParameterless = !_parameterFinder.Find(columnDefinition, _parameter);

            if (isParameterless)
                le = Expression.Lambda(columnDefinition);
            else
                le = Expression.Lambda(columnDefinition, _parameter);

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