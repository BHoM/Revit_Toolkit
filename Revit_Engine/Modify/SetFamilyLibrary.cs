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
            if (revitSettings == null)
                return null;

            RevitSettings aRevitSettings = revitSettings.GetShallowClone() as RevitSettings;

            aRevitSettings.FamilyLibrary = familyLibrary;

            return aRevitSettings;
        }

        /***************************************************/
    }
}
