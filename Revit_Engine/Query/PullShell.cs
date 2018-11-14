using BH.oM.DataManipulation.Queries;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Returns true if FilterQuery should pull edges from Revit Element")]
        [Input("filterQuery", "FilterQuery")]
        [Output("PullEdges")]
        public static bool PullEdges(this FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return false;

            if (filterQuery.Equalities.ContainsKey(Convert.FilterQuery.PullEdges))
            {
                object aObject = filterQuery.Equalities[Convert.FilterQuery.PullEdges];
                if (aObject is bool)
                    return (bool)aObject;
            }

            return false;
        }

        /***************************************************/
    }
}