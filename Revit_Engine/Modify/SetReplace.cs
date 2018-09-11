using System.Collections.Generic;
using BH.oM.Adapters.Revit;
using System.IO;
using BH.oM.Adapters.Revit.Settings;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static RevitSettings SetReplace(this RevitSettings RevitSettings, bool Replace)
        {
            if (RevitSettings == null)
                return null;

            RevitSettings aRevitSettings = RevitSettings.GetShallowClone() as RevitSettings;
            aRevitSettings.Replace = Replace;

            return aRevitSettings;
        }

        /***************************************************/
    }
}
