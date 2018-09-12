using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Settings;

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
    }
}