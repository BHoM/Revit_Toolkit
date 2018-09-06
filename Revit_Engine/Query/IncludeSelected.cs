using BH.oM.Adapters.Revit;

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
    }
}