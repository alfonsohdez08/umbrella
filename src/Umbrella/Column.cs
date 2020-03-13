using System;
using System.Collections.Generic;
using System.Text;

namespace Umbrella
{
    /// <summary>
    /// Column inferred/discovered by the columns mapping process.
    /// </summary>
    internal class Column
    {
        /// <summary>
        /// Name of the DataColumn.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Data type of the DataColumn.
        /// </summary>
        public Type DataType { get; set; }

        /// <summary>
        /// Denotes whether the DataColumn allows NULL or not.
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        /// A compiled expression that maps the data into the DataColumn when creating DataRows.
        /// </summary>
        public Delegate Mapper { get; set; }

        /// <summary>
        /// A flag that says when the mapper function/delegate is parameterless.
        /// </summary>
        public bool IsMapperParameterless { get; set; }
    }
}
