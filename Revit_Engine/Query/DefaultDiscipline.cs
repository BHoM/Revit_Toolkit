using BH.oM.Adapters.Revit;

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
                return oM.Adapters.Revit.Discipline.Environmental;

            return revitSettings.DefaultDiscipline;
        }

        /***************************************************/
    }
}