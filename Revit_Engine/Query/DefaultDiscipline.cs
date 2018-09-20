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
        
        public static Discipline DefaultDiscipline(this RevitSettings revitSettings)
        {
            if (revitSettings == null)
                return oM.Adapters.Revit.Enums.Discipline.Environmental;

            return revitSettings.DefaultDiscipline;
        }

        /***************************************************/

        public static Discipline DefaultDiscipline(this FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return oM.Adapters.Revit.Enums.Discipline.Environmental;

            if (!filterQuery.Equalities.ContainsKey("DefaultDiscipline"))
                return oM.Adapters.Revit.Enums.Discipline.Environmental;

            if(!(filterQuery.Equalities["DefaultDiscipline"] is Discipline))
                return oM.Adapters.Revit.Enums.Discipline.Environmental;

            return (Discipline)filterQuery.Equalities["DefaultDiscipline"];
        }

        /***************************************************/
    }
}