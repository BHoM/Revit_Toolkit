using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.Engine.Environment;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        internal static List<oM.Environment.Elements.Panel> Panels(this GeometryElement geometryElement, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> aResult = new List<oM.Environment.Elements.Panel>();
            foreach (GeometryObject aGeometryObject in geometryElement)
            {
                Solid aSolid = aGeometryObject as Solid;
                if (aSolid == null)
                    continue;

                PlanarFace aPlanarFace = Query.Top(aSolid);
                if (aPlanarFace == null)
                    continue;

                List<BH.oM.Environment.Elements.Panel> aBuildingElementPanelList = aPlanarFace.Panels(pullSettings);
                if (aBuildingElementPanelList == null || aBuildingElementPanelList.Count < 1)
                    continue;

                aResult.AddRange(aBuildingElementPanelList);
            }

            return aResult;

        }

        /***************************************************/

        internal static List<oM.Environment.Elements.Panel> Panels(this PlanarFace planarFace, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> aResult = new List<oM.Environment.Elements.Panel>();

            List<PolyCurve> aPolyCurveList = planarFace.ToBHoMPolyCurveList(pullSettings);

            foreach (PolyCurve aPolyCurve in aPolyCurveList)
            {
                //Create the Panel
                oM.Environment.Elements.Panel aPanel = Create.Panel(aPolyCurve);
                aResult.Add(aPanel);
            }

            return aResult;
        }

        /***************************************************/
    }
}