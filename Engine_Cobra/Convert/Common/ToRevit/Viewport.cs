using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;


namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Internal Methods                          ****/
        /***************************************************/

        internal static Viewport ToRevit(this oM.Adapters.Revit.Elements.Viewport viewport, Document document, PushSettings pushSettings = null)
        {
            if (viewport == null)
                return null;

            if (viewport.Location == null)
                return null;

            string aViewName = BH.Engine.Adapters.Revit.Query.ViewName(viewport);
            if (string.IsNullOrEmpty(aViewName))
                return null;

            string aSheetNumber = BH.Engine.Adapters.Revit.Query.SheetNumber(viewport);
            if (string.IsNullOrEmpty(aSheetNumber))
                return null;

            List<View> aViewList = new FilteredElementCollector(document).OfClass(typeof(View)).Cast<View>().ToList();
            View aView = aViewList.Find(x => !x.IsTemplate && x.ViewName == aViewName);
            if(aView == null)
                return null;

            List<ViewSheet> aViewSheetList = new FilteredElementCollector(document).OfClass(typeof(ViewSheet)).Cast<ViewSheet>().ToList();
            ViewSheet aViewSheet = aViewSheetList.Find(x => !x.IsTemplate && !x.IsPlaceholder && x.SheetNumber == aSheetNumber);
            if (aViewSheet == null)
                return null;

            Viewport aViewport = Viewport.Create(document, aViewSheet.Id, aView.Id, ToRevit(viewport.Location, pushSettings));

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aViewport, viewport, null, pushSettings.ConvertUnits);

            return aViewport;
        }

        /***************************************************/
    }
}