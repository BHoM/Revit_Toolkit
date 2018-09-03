using BH.oM.Revit;

namespace BH.Engine.Revit
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

            revitSettings.FamilyLibrary = familyLibrary;

            return aRevitSettings;
        }

        /***************************************************/
    }
}
