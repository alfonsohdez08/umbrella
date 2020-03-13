using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Umbrella
{
    public static class Umbrella
    {
        /// <summary>
        /// Converts an IEnumerable instance to a DataTable.
        /// </summary>
        /// <typeparam name="TEntity">Type that holds the set (the type parameter of an IEnumerable<>).</typeparam>
        /// <typeparam name="TProjection">Type produced by the projector.</typeparam>
        /// <param name="source">Dataset.</param>
        /// <param name="projector">Projects a structure that defines the columns of the DataTable and how the data would be mapped into it (mapping expressions).</param>
        /// <returns>A filled DataTable where its columns were inferred/discovered by the projection.</returns>
        public static DataTable ToDataTable<TEntity, TProjection>(this IEnumerable<TEntity> source, Expression<Func<TEntity, TProjection>> projector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (projector == null)
                throw new ArgumentNullException(nameof(projector));

            return UmbrellaDataTable<TEntity>.Build(source, projector);
        }

    }
}
