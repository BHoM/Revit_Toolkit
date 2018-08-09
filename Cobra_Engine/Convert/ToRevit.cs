using System.Collections.Generic;

using BH.oM.Environment.Elements;
using BH.oM.Environment.Interface;

using Autodesk.Revit.DB;
using BH.oM.Base;
using BH.oM.Adapters.Revit;

namespace BH.Engine.Revit
{

    /// <summary>
    /// BHoM Revit Engine Convert Methods
    /// </summary>
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Element ToRevit(this BHoMObject bHoMObject, Document document, PushSettings pushSettings = null)
        {
            if (pushSettings == null)
                pushSettings = PushSettings.Default;

            return ToRevit(bHoMObject as dynamic, document, pushSettings);
        }
    }
}
