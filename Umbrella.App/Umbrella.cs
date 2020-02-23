using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Umbrella.App
{
    public static class Umbrella
    {
        /// <summary>
        /// Creates a DataTable that projects the given list.
        /// </summary>
        /// <typeparam name="TEntity">Type of entity.</typeparam>
        /// <typeparam name="TProjection">Type produced by the projection.</typeparam>
        /// <param name="list">Source list.</param>
        /// <param name="projector">Defines the shape of the DataTable (its columns).</param>
        /// <returns>A filled DataTable with the columns listed by the projector.</returns>
        public static DataTable ToDataTable<TEntity, TProjection>(this List<TEntity> list, Expression<Func<TEntity, TProjection>> projector)
        {
            // TODO: I should remove nested NewExpression/MemberInitExpression before visiting the projection (throw an exception i wuold say)

            return UmbrellaDataTable<TEntity>.Build(list, projector);
        }
    }
}
