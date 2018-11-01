using BH.oM.DataManipulation.Queries;
using BH.oM.Environment.Elements;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        public static FilterQuery SpaceFilterQuery(bool pullShell = true)
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(Space);
            aFilterQuery.Equalities[Convert.FilterQuery.PullShell] = pullShell;
            return aFilterQuery;
        }
    }
}
