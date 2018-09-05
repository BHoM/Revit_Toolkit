using BH.oM.Adapters.Revit;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static bool IncludeSelected(this RevitSettings RevitSettings)
        {
            if (RevitSettings == null || RevitSettings.SelectionSettings == null)
                return false;

            return RevitSettings.SelectionSettings.IncludeSelected;
        }

        /***************************************************/
    }
}