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

        [Description("Sets Pull Edges option for FilterQuery.")]
        [Input("filterQuery", "FilterQuery")]
        [Input("pullEdges", "Set to true if include geometry edges of Revit Element in CustomData of BHoMObject")]
        [Output("FilterQuery")]
        public static FilterQuery SetPullEdges(this FilterQuery filterQuery, bool pullEdges)
        {
            if (filterQuery == null)
                return null;

            FilterQuery aFilterQuery = Query.Duplicate(filterQuery);

            if (aFilterQuery.Equalities.ContainsKey(Convert.FilterQuery.PullEdges))
                aFilterQuery.Equalities[Convert.FilterQuery.PullEdges] = pullEdges;
            else
                aFilterQuery.Equalities.Add(Convert.FilterQuery.PullEdges, pullEdges);

            return aFilterQuery;
        }

        /***************************************************/
    }
}
