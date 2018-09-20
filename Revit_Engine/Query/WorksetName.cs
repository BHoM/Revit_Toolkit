using BH.oM.DataManipulation.Queries;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static string WorksetName(this FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return null;

            if (!filterQuery.Equalities.ContainsKey(Convert.FilterQuery.WorksetName))
                return null;

            return filterQuery.Equalities[Convert.FilterQuery.WorksetName] as string;
        }

        /***************************************************/
    }
}
