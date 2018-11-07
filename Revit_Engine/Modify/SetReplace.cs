using System.ComponentModel;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Sets Replace property for GeneralSettings stored in RevitSettings.")]
        [Input("revitSettings", "RevitSettings")]
        [Input("replace", "Replace existing elements in the model for push method. Update parameters (CustomData) only if set to false.")]
        [Output("RevitSettings")]
        public static RevitSettings SetReplace(this RevitSettings revitSettings, bool replace)
        {
            if (revitSettings == null || revitSettings.GeneralSettings == null)
                return null;

            RevitSettings aRevitSettings = revitSettings.GetShallowClone() as RevitSettings;
            aRevitSettings.GeneralSettings.Replace = replace;

            return aRevitSettings;
        }

        /***************************************************/
    }
}
