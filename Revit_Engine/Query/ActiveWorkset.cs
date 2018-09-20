using BH.oM.DataManipulation.Queries;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static bool ActiveWorkset(this FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return false;

            if (!filterQuery.Equalities.ContainsKey(Convert.FilterQuery.ActiveWorkset))
                return false;

            if (filterQuery.Equalities[Convert.FilterQuery.ActiveWorkset] is bool)
                return false;

            return (bool)filterQuery.Equalities[Convert.FilterQuery.ActiveWorkset];
        }

        /***************************************************/
    }
}
