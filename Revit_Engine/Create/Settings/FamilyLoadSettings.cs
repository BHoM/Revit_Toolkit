using BH.oM.Adapters.Revit.Generic;
using BH.oM.Adapters.Revit.Settings;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

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
