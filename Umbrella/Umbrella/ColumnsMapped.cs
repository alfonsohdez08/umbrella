using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Umbrella.Expressions;
using Umbrella.Visitors;

namespace Umbrella
{
    public class ColumnsMapped : ColumnVisitor
    {
        public List<Column> Columns { get; private set; } = new List<Column>();

        private MemberInfo _memberInScope;

        private readonly ParameterExpression _parameterExp;
        private readonly Expression _projectorBody;

        public ColumnsMapped(Expression projector)
        {
            var lambdaExp = (LambdaExpression)projector;

            _parameterExp = lambdaExp.Parameters[0];
            _projectorBody = lambdaExp.Body;
        }

        public static List<Column> GetMappedColumns(Expression projector)
        {
            var columnsMappedVisitor = new ColumnsMapped(projector);
            columnsMappedVisitor.SetColumns();

            return columnsMappedVisitor.Columns;
        }

        public void SetColumns()
        {
            Visit(_projectorBody);
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
            var constantExp = columnDefinition as ConstantExpression;
            if (constantExp != null && constantExp.Type == typeof(ColumnSettings))
            {
                var columnSettings = (ColumnSettings)constantExp.Value;

                columnDefinition = ((LambdaExpression)columnSettings.Mapper).Body;
                columnName = !string.IsNullOrEmpty(columnSettings.ColumnName) ? columnSettings.ColumnName : columnName;
                columnDataType = columnSettings.ColumnDataType;
            }

            LambdaExpression le = null;
            bool isParameterless = MapperParameterVisitor.IsMapperFunctionParameterless(_parameterExp, columnDefinition);

            if (isParameterless)
                le = Expression.Lambda(columnDefinition);
            else
                le = Expression.Lambda(columnDefinition, _parameterExp);

            var column = new Column()
            {
                Name = columnName,
                DataType = columnDataType,
                IsNullable = isNullable,
                Mapper = le.Compile(),
                IsParameterless = isParameterless
            };

            Columns.Add(column);

            return c;
        }
    }

    public class MapperParameterVisitor: ExpressionVisitor
    {
        private readonly ParameterExpression _parameterExp;
        private readonly Expression _mappingExpression;

        private bool _foundParameter = false;

        public MapperParameterVisitor(ParameterExpression parameterExp, Expression mappingExpression)
        {
            _parameterExp = parameterExp;
            _mappingExpression = mappingExpression;
        }

        public void FindParameter()
        {
            Visit(_mappingExpression);
        }

        public static bool IsMapperFunctionParameterless(ParameterExpression parameter, Expression mappingExpression)
        {
            var mapperParameterVisitor = new MapperParameterVisitor(parameter, mappingExpression);
            mapperParameterVisitor.FindParameter();

            return !mapperParameterVisitor._foundParameter;
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            if (_parameterExp == p)
                _foundParameter = true;

            return p;
        }
    }
}