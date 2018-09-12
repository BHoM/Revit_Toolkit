using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static oM.Adapters.Revit.Elements.Viewport ToBHoMViewport(this Viewport viewport, PullSettings pullSettings = null)
        {
            if (viewport == null)
                return null;

            oM.Geometry.Point aLocation = viewport.GetBoxCenter().ToBHoM(pullSettings);
            string aViewName = viewport.get_Parameter(BuiltInParameter.VIEW_NAME).AsString();
            string aSheetNumber = viewport.get_Parameter(BuiltInParameter.VIEWPORT_SHEET_NUMBER).AsString();

            oM.Adapters.Revit.Elements.Viewport aViewport = BH.Engine.Adapters.Revit.Create.Viewport(aSheetNumber, aViewName, aLocation);

            aViewport.Name = viewport.Name;
            aViewport = Modify.SetIdentifiers(aViewport, viewport) as oM.Adapters.Revit.Elements.Viewport;
            aViewport = Modify.SetCustomData(aViewport, viewport, pullSettings.ConvertUnits) as oM.Adapters.Revit.Elements.Viewport;

            return aViewport;
        }

        /***************************************************/
    }
}
