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
            pullSettings = pullSettings.DefaultIfNull();

            oM.Adapters.Revit.Elements.Viewport aViewport = pullSettings.FindRefObject<oM.Adapters.Revit.Elements.Viewport>(viewport.Id.IntegerValue);
            if (aViewport != null)
                return aViewport;

            oM.Geometry.Point aLocation = viewport.GetBoxCenter().ToBHoM(pullSettings);
            string aViewName = viewport.get_Parameter(BuiltInParameter.VIEW_NAME).AsString();
            string aSheetNumber = viewport.get_Parameter(BuiltInParameter.VIEWPORT_SHEET_NUMBER).AsString();

            aViewport = BH.Engine.Adapters.Revit.Create.Viewport(aSheetNumber, aViewName, aLocation);

            aViewport.Name = viewport.Name;

            aViewport = Modify.SetIdentifiers(aViewport, viewport) as oM.Adapters.Revit.Elements.Viewport;
            if (pullSettings.CopyCustomData)
                aViewport = Modify.SetCustomData(aViewport, viewport, pullSettings.ConvertUnits) as oM.Adapters.Revit.Elements.Viewport;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aViewport);

            return aViewport;
        }

        /***************************************************/
    }
}
