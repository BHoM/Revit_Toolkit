using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Adapters.Revit.Elements;


namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Internal Methods                          ****/
        /***************************************************/

        internal static ViewSheet ToRevitViewSheet(this Sheet sheet, Document document, PushSettings pushSettings = null)
        {
            if (sheet == null)
                return null;

            ViewSheet aViewSheet = ViewSheet.Create(document, ElementId.InvalidElementId);

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aViewSheet, sheet, null, pushSettings.ConvertUnits);

            return aViewSheet;
        }

        /***************************************************/
    }
}