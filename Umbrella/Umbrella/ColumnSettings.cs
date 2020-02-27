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

        public Expression Mapper => _mapper;
        public string ColumnName => _columnName;
        public Type ColumnDataType => _columnDataType;

        private ColumnSettings(Expression mapper)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            _mapper = mapper;
        }

        public static ColumnSettings Build<T>(Expression<Func<T>> columnProjector)
        {
            var columnSettings = new ColumnSettings(columnProjector);

            return columnSettings;
        }

        public ColumnSettings Name(string columnName)
        {
            _columnName = columnName;

            return this;
        }

        public ColumnSettings DataType(Type dataType)
        {
            _columnDataType = dataType;

            return this;
        }
    }
}