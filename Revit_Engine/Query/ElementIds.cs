using System.Collections.Generic;

using BH.oM.Base;
using BH.oM.DataManipulation.Queries;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

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