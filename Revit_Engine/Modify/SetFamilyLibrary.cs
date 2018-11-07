using BH.oM.Adapters.Revit.Generic;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Sets Pull FamilyLibrary for RevitSettings.")]
        [Input("revitSettings", "RevitSettings")]
        [Input("familyLibrary", "FamilyLibrary to be set")]
        [Output("RevitSettings")]
        public static RevitSettings SetFamilyLibrary(this RevitSettings revitSettings, FamilyLibrary familyLibrary)
        {
            if (revitSettings == null || revitSettings.FamilyLoadSettings == null)
                return null;

            RevitSettings aRevitSettings = revitSettings.GetShallowClone() as RevitSettings;

            aRevitSettings.FamilyLoadSettings = SetFamilyLibrary(revitSettings.FamilyLoadSettings, familyLibrary);

            return aRevitSettings;
        }

        /***************************************************/

        [Description("Sets Pull FamilyLibrary for FamilyLoadSettings.")]
        [Input("familyLoadSettings", "FamilyLoadSettings")]
        [Input("familyLibrary", "FamilyLibrary to be set")]
        [Output("FamilyLoadSettings")]
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
