using BH.oM.Revit;

namespace BH.Engine.Revit
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