using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Umbrella
{
    public class ColumnSettings
    {
        private readonly Expression _mapper;

        private string _columnName;
        private Type _columnDataType;

        internal Expression Mapper => _mapper;
        internal string ColumnName => _columnName;
        internal Type ColumnDataType => _columnDataType;

        private ColumnSettings(Expression mapper, Type columnDataType)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            _mapper = mapper;
            _columnDataType = columnDataType;
        }

        public static ColumnSettings Build<T>(Expression<Func<T>> columnProjector)
        {
            var columnSettings = new ColumnSettings(columnProjector, typeof(T));

            return columnSettings;
        }

        public ColumnSettings Name(string columnName)
        {
            _columnName = columnName;

            return this;
        }
    }
}