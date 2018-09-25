using System.Linq;
using System.Collections.Generic;

using BH.oM.DataManipulation.Queries;


namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static IEnumerable<FilterQuery> FilterQueries(this FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return null;

            if (!filterQuery.Equalities.ContainsKey(Convert.FilterQuery.FilterQueries))
                return null;

            if (filterQuery.Equalities[Convert.FilterQuery.FilterQueries] is IEnumerable<FilterQuery>)
                return (IEnumerable<FilterQuery>)filterQuery.Equalities[Convert.FilterQuery.FilterQueries];

            if (filterQuery.Equalities[Convert.FilterQuery.FilterQueries] is IEnumerable<object>)
            {
                IEnumerable<object> aObjects = filterQuery.Equalities[Convert.FilterQuery.FilterQueries] as IEnumerable<object>;
                if (aObjects != null)
                    return aObjects.Cast<FilterQuery>();
            }

            return null;
        }

        /***************************************************/
    }
}
