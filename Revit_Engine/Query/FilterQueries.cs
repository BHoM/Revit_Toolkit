using System.Linq;
using System.Collections.Generic;

using BH.oM.DataManipulation.Queries;
using System.ComponentModel;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Gets child FilterQueries for given FiletrQuery (FilterQuery can be combined by logical operation - LogicalAndSelectionFilter, LogicalOrSelectionFilter).")]
        [Input("filterQuery", "FilterQuery")]
        [Output("FilterQueries")]
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
