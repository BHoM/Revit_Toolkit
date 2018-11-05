using BH.oM.Base;
using BH.oM.DataManipulation.Queries;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static string TypeName(this FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return null;

            if (!filterQuery.Equalities.ContainsKey(Convert.FilterQuery.TypeName))
                return null;

            return filterQuery.Equalities[Convert.FilterQuery.TypeName] as string;
        }

        /***************************************************/
    }
}
