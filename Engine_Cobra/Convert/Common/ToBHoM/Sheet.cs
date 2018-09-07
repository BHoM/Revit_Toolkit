using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static Sheet ToBHoM(this ViewSheet viewSheet, PullSettings pullSettings = null)
        {
            Sheet aSheet = BH.Engine.Adapters.Revit.Create.Sheet(viewSheet.Name, viewSheet.SheetNumber);

            aSheet.Name = viewSheet.Name;
            aSheet = Modify.SetIdentifiers(aSheet, viewSheet) as Sheet;
            aSheet = Modify.SetCustomData(aSheet, viewSheet, true) as Sheet;

            return aSheet;
        }

        /***************************************************/
    }
}
