using BH.oM.Adapters.Revit.Generic;
using BH.oM.Adapters.Revit.Settings;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static RevitSettings SetFamilyLibrary(this RevitSettings revitSettings, FamilyLibrary familyLibrary)
        {
            if (revitSettings == null || revitSettings.FamilyLoadSettings == null)
                return null;

            RevitSettings aRevitSettings = revitSettings.GetShallowClone() as RevitSettings;

            aRevitSettings.FamilyLoadSettings = SetFamilyLibrary(revitSettings.FamilyLoadSettings, familyLibrary);

            return aRevitSettings;
        }

        /***************************************************/

        public static FamilyLoadSettings SetFamilyLibrary(this FamilyLoadSettings familyLoadSettings, FamilyLibrary familyLibrary)
        {
            if (familyLoadSettings == null)
                return null;

            FamilyLoadSettings aFamilyLoadSettings = familyLoadSettings.GetShallowClone() as FamilyLoadSettings;

            aFamilyLoadSettings.FamilyLibrary = familyLibrary;

            return aFamilyLoadSettings;
        }

        /***************************************************/
    }
}
