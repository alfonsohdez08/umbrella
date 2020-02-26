﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Umbrella.Expressions;

namespace Umbrella.Visitors
{
    public class ColumnsMappedVisitor : ColumnVisitor
    {
        //public Dictionary<DataColumn, Delegate> Columns { get; private set; } = new Dictionary<DataColumn, Delegate>();
        public List<Column> Columns { get; private set; } = new List<Column>();

        private MemberInfo _memberInScope;

        private readonly ParameterExpression _parameterExp;
        private readonly Expression _projector;

        public ColumnsMappedVisitor(Expression projector)
        {
            var lambdaExp = (LambdaExpression)projector;

            _parameterExp = lambdaExp.Parameters[0];
            _projector = lambdaExp.Body;
        }

        public static List<Column> GetMappedColumns(Expression projector)
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
            
            LambdaExpression le = Expression.Lambda(c.ColumnDefinition, _parameterExp);
            var column = new Column()
            {
                Name = columnName,
                DataType = columnDataType,
                IsNullable = isNullable,
                Mapper = le.Compile()
            };

            Columns.Add(column);

            return c;
        }
    }
}
