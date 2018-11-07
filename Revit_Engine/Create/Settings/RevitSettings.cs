using System.ComponentModel;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Creates Revit Settings class which contols behaviour of Revit Adapter")]
        [Input("connectionSettings", "Connection Settings for Revit Adapter")]
        [Input("familyLoadSettings", "FamilyLoad Settings for Revit Adapter")]
        [Input("generalSettings", "General Settings for Revit Adapter")]
        [Output("RevitSettings")]
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
