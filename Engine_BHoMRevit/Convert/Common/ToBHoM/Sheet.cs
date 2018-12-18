using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static Sheet ToBHoMSheet(this ViewSheet viewSheet, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            Sheet aSheet = pullSettings.FindRefObject<Sheet>(viewSheet.Id.IntegerValue);
            if (aSheet != null)
                return aSheet;

            aSheet = BH.Engine.Adapters.Revit.Create.Sheet(viewSheet.Name, viewSheet.SheetNumber);

            aSheet.Name = viewSheet.Name;

            aSheet = Modify.SetIdentifiers(aSheet, viewSheet) as Sheet;
            if (pullSettings.CopyCustomData)
                aSheet = Modify.SetCustomData(aSheet, viewSheet, pullSettings.ConvertUnits) as Sheet;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aSheet);

            return aSheet;
        }

        /***************************************************/
    }
}
