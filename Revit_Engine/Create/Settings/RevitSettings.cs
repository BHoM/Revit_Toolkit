using BH.oM.Adapters.Revit.Settings;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static RevitSettings RevitSettings(ConnectionSettings connectionSettings = null, FamilyLoadSettings familyLoadSettings = null, GeneralSettings generalSettings = null)
        {
            RevitSettings aRevitSettings = new RevitSettings();

            if (connectionSettings != null)
                aRevitSettings.ConnectionSettings = connectionSettings;

            if (familyLoadSettings != null)
                aRevitSettings.FamilyLoadSettings = familyLoadSettings;

            if (generalSettings != null)
                aRevitSettings.GeneralSettings = generalSettings;

            return aRevitSettings;
        }

        /***************************************************/
    }
}
