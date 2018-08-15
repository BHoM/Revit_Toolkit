using BH.oM.Revit;

namespace BH.Engine.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static Discipline DefaultDiscipline(this RevitSettings revitSettings)
        {
            if (revitSettings == null)
                return oM.Revit.Discipline.Environmental;

            return revitSettings.DefaultDiscipline;
        }

        /***************************************************/
    }
}