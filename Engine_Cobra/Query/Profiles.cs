using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit.Settings;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        static public List<oM.Geometry.PolyCurve> Profiles(this Element element, PullSettings pullSettings = null)
        {
            if (element == null || element.Document == null)
                return null;

            Document aDocument = element.Document;

            IEnumerable<ElementId> aElementIds = null;
            using (Transaction aTransaction = new Transaction(aDocument, "Temp"))
            {
                aTransaction.Start();
                aElementIds = aDocument.Delete(element.Id);
                aTransaction.RollBack();
            }

            if (aElementIds == null || aElementIds.Count() == 0)
                return null;

            List<oM.Geometry.PolyCurve> aResult = new List<oM.Geometry.PolyCurve>();
            foreach (ElementId aElementId in aElementIds)
            {
                Element aElement = aDocument.GetElement(aElementId);
                if (aElement == null)
                    continue;

                if (aElement is Sketch)
                {
                    Sketch aSketch = (Sketch)aElement;

                    if (aSketch.Profile == null)
                        continue;

                    List<List<oM.Geometry.ICurve>> aCurveListList = Convert.ToBHoM(aSketch.Profile, pullSettings);
                    if (aCurveListList == null)
                        continue;

                    foreach (List<oM.Geometry.ICurve> aCureList in aCurveListList)
                        if (aCureList != null)
                            aResult.Add(BH.Engine.Geometry.Create.PolyCurve(aCureList));
                }
            }
            return aResult;
        }

        /***************************************************/
    }
}
