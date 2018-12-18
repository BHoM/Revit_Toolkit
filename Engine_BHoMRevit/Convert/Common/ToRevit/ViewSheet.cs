using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Adapters.Revit.Elements;


namespace BH.UI.Revit.Engine
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

            ViewSheet aViewSheet = pushSettings.FindRefObject<ViewSheet>(document, sheet.BHoM_Guid);
            if (aViewSheet != null)
                return aViewSheet;

            pushSettings.DefaultIfNull();

            aViewSheet = ViewSheet.Create(document, ElementId.InvalidElementId);

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aViewSheet, sheet, null, pushSettings.ConvertUnits);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(sheet, aViewSheet);

            return aViewSheet;
        }

        /***************************************************/
    }
}