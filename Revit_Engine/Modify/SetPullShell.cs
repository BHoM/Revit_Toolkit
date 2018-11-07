using System.ComponentModel;

using BH.oM.DataManipulation.Queries;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Sets Pull Shell option for FilterQuery.")]
        [Input("filterQuery", "FilterQuery")]
        [Input("pullShell", "Set to true if include shell geometry of Revit Element in CustomData of BHoMObject")]
        [Output("FilterQuery")]
        public static FilterQuery SetPullShell(this FilterQuery filterQuery, bool pullShell)
        {
            if (filterQuery == null)
                return null;

            FilterQuery aFilterQuery = Query.Duplicate(filterQuery);

            if (aFilterQuery.Equalities.ContainsKey(Convert.FilterQuery.PullShell))
                aFilterQuery.Equalities[Convert.FilterQuery.PullShell] = pullShell;
            else
                aFilterQuery.Equalities.Add(Convert.FilterQuery.PullShell, pullShell);

            return aFilterQuery;
        }

        /***************************************************/
    }
}
