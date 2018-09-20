using BH.oM.Adapters.Revit.Settings;
using BH.oM.DataManipulation.Queries;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static bool IncludeSelected(this RevitSettings revitSettings)
        {
            if (revitSettings == null || revitSettings.SelectionSettings == null)
                return false;

            return revitSettings.SelectionSettings.IncludeSelected;
        }

        /***************************************************/

        public static bool IncludeSelected(this FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return false;

            if (!filterQuery.Equalities.ContainsKey(Convert.FilterQuery.IncludeSelected))
                return false;

            if (filterQuery.Equalities[Convert.FilterQuery.IncludeSelected] is bool)
                return (bool)filterQuery.Equalities[Convert.FilterQuery.IncludeSelected];
            else
                return false;
        }

        /***************************************************/
    }
}