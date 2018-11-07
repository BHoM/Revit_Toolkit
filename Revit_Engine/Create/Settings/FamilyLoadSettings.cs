using BH.oM.Adapters.Revit.Generic;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Creates Family Load Settings class which contols Family loading behaviour when family not available in project")]
        [Input("familyLibrary", "FamilyLibrary which defines directory to be searched")]
        [Input("overwriteFamily", "Overwrite Family if exists in model")]
        [Input("overwriteParameterValues", "Overwrite parameter values for existing types")]
        [Output("FamilyLoadSettings")]
        public static FamilyLoadSettings FamilyLoadSettings(FamilyLibrary familyLibrary = null, bool overwriteFamily = true, bool overwriteParameterValues = true)
        {
            FamilyLoadSettings aFamilyLoadSettings = new FamilyLoadSettings()
            {
                OverwriteFamily = overwriteFamily,
                OverwriteParameterValues = overwriteParameterValues
            };

            if (familyLibrary != null)
                aFamilyLoadSettings.FamilyLibrary = familyLibrary;

            return aFamilyLoadSettings;
        }

        /***************************************************/
    }
}
