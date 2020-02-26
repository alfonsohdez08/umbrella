using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Umbrella.Visitors;

namespace Umbrella
{
    internal static class ColumnsMapping
    {
        /// <summary>
        /// Retrieves the columns identified given a projector.
        /// </summary>
        /// <param name="projector">Defines the columns that would have the DataTable.</param>
        /// <returns>A mapping set of System.Data.DataColumn and Delegate.</returns>
        /// <remarks>
        /// The Delegate represents the function required in order to fetch the data for in a specific column.
        /// </remarks>
        public static List<Column> GetColumns(Expression projector)
        {
            projector = PropertyColumnReplacement.ReplacePropertiesByColumns(projector);

            return ColumnsMappedVisitor.GetMappedColumns(projector);
        }
    }

}
