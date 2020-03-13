using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

namespace Umbrella.Expr.Column
{
    /// <summary>
    /// DataTable node types.
    /// </summary>
    internal enum DataTableExpression
    {
        Column = 1000
    }

    /// <summary>
    /// An expression that denotes a DataTable's column.
    /// </summary>
    internal class ColumnExpression : Expression
    {
        private readonly Type _type;
        
        /// <summary>
        /// An expression that describes the data mapping for the column.
        /// </summary>
        public Expression ColumnMapper { get; private set; }
        public override Type Type => _type;
        public override ExpressionType NodeType => (ExpressionType)DataTableExpression.Column;

        public ColumnExpression(Expression columnMapper)
        {
            _type = columnMapper.Type;

            ColumnMapper = columnMapper;
        }

    }
}
