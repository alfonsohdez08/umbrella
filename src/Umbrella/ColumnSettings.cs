using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Umbrella
{
    /// <summary>
    /// Provides configurable values for a column.
    /// </summary>
    public class ColumnSettings
    {
        private readonly Expression _mapper;

        private string _columnName;
        private Type _columnDataType;

        /// <summary>
        /// An expression used as mapping for set the data into the column when filling the DataTable.
        /// </summary>
        internal Expression Mapper => _mapper;

        /// <summary>
        /// Column's name (DataColumn column name).
        /// </summary>
        internal string ColumnName => _columnName;
        
        /// <summary>
        /// Column's data type (DataColumn data type).
        /// </summary>
        internal Type ColumnDataType => _columnDataType;

        private ColumnSettings(Expression mapper)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            _mapper = mapper;
            _columnDataType = ((LambdaExpression)mapper).Body.Type;
        }

        /// <summary>
        /// Creates a ColumnSettings instance that has the <paramref name="columnProjector"/> as mapper.
        /// </summary>
        /// <typeparam name="T">Data type for the column.</typeparam>
        /// <param name="columnProjector">An expression used for mapping the value into the column when dumping the data into the DataTable.</param>
        /// <returns>A ColumnSettings instance.</returns>
        public static ColumnSettings Build<T>(Expression<Func<T>> columnProjector)
        {
            var columnSettings = new ColumnSettings(columnProjector);

            return columnSettings;
        }

        /// <summary>
        /// Sets the column's name.
        /// </summary>
        /// <param name="columnName">Column's name.</param>
        /// <returns>A ColumnSettings instance that has the name provided.</returns>
        public ColumnSettings Name(string columnName)
        {
            _columnName = columnName;

            return this;
        }
    }
}