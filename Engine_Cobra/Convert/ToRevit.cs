using Autodesk.Revit.DB;
using BH.oM.Base;
using BH.oM.Revit;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Element ToRevit(this BHoMObject bHoMObject, Document document, PushSettings pushSettings = null)
        {
            pushSettings.DefaultIfNull();

            return ToRevit(bHoMObject as dynamic, document, pushSettings);
        }

        /***************************************************/
    }
}