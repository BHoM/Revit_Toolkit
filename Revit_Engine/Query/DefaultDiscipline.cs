using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.DataManipulation.Queries;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static Discipline? DefaultDiscipline(this RevitSettings revitSettings)
        {
            if (revitSettings == null || revitSettings.GeneralSettings == null)
                return null;

            return revitSettings.GeneralSettings.DefaultDiscipline;
        }

        /***************************************************/

        public static Discipline? DefaultDiscipline(this FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return null;

            if (!filterQuery.Equalities.ContainsKey(Convert.FilterQuery.DefaultDiscipline))
                return null;

            //if(!(filterQuery.Equalities[Convert.FilterQuery.DefaultDiscipline] is Discipline))
            //    return null;

            return (Discipline)filterQuery.Equalities[Convert.FilterQuery.DefaultDiscipline];
        }

        /***************************************************/
    }
}