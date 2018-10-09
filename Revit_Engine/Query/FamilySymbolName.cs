using BH.oM.Base;
using BH.oM.DataManipulation.Queries;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static string FamilySymbolName(this FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return null;

            if (!filterQuery.Equalities.ContainsKey(Convert.FilterQuery.FamilySymbolName))
                return null;

            return filterQuery.Equalities[Convert.FilterQuery.FamilySymbolName] as string;
        }

        /***************************************************/
    }
}
