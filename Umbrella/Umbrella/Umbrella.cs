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
        /// Creates a filled DataTable whose columns are inferred from a projection.
        /// </summary>
        /// <typeparam name="TEntity">Type that contains the collection.</typeparam>
        /// <typeparam name="TProjection">Type produced by the projection.</typeparam>
        /// <param name="source">Source collection.</param>
        /// <param name="projector">Defines the shape of the DataTable (its columns).</param>
        /// <returns>A filled DataTable with the columns listed by the projector.</returns>
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
