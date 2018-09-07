using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Element ToRevit(this IBHoMObject bHoMObject, Document document, PushSettings pushSettings = null)
        {
             pushSettings = pushSettings.DefaultIfNull();

            return ToRevit(bHoMObject as dynamic, document, pushSettings);
        }

        /***************************************************/
    }
}