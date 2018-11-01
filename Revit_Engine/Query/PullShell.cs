using BH.oM.DataManipulation.Queries;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static bool PullShell(this FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return false;

            if (filterQuery.Equalities.ContainsKey(Convert.FilterQuery.PullShell))
            {
                object aObject = filterQuery.Equalities[Convert.FilterQuery.PullShell];
                if (aObject is bool)
                    return (bool)aObject;
            }

            return false;
        }

        /***************************************************/
    }
}