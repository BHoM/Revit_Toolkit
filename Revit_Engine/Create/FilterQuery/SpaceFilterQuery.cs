using System.ComponentModel;

using BH.oM.DataManipulation.Queries;
using BH.oM.Environment.Elements;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        [Description("Creates FilterQuery which filters all spaces in Active Revit Document.")]
        [Input("pullEdges", "Pull edges (3D represenation) of space")]
        [Output("FilterQuery")]
        public static FilterQuery SpaceFilterQuery(bool pullEdges = true)
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(Space);
            aFilterQuery.Equalities[Convert.FilterQuery.PullEdges] = pullEdges;
            return aFilterQuery;
        }
    }
}
