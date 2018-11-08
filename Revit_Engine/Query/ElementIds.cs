using System.ComponentModel;
using System.Collections.Generic;

using BH.oM.DataManipulation.Queries;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Gets integer representation of ElementIds for given FilterQuery (Example: SelectionFilterQuery).")]
        [Input("filterQuery", "FilterQuery")]
        [Output("ElementIds")]
        public static IEnumerable<int> ElementIds(this FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return null;

            if (!filterQuery.Equalities.ContainsKey(Convert.FilterQuery.ElementIds))
                return null;

            return filterQuery.Equalities[Convert.FilterQuery.ElementIds] as IEnumerable<int>;
        }

        /***************************************************/
    }
}