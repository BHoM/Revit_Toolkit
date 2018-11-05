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
            return new FamilyLoadSettings()
            {
                FamilyLibrary = familyLibrary,
                OverwriteFamily = overwriteFamily,
                OverwriteParameterValues = overwriteParameterValues
            };
        }

        /***************************************************/
    }
}
