using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.Engine.Environment;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static List<oM.Environment.Elements.Panel> ToBHoMBuildingElementPanels(this PlanarFace planarFace, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> aResult = new List<oM.Environment.Elements.Panel>();

            List<List<oM.Geometry.ICurve>> crvs = planarFace.ToBHoMCurve(pullSettings);

            foreach(List<oM.Geometry.ICurve> lst in crvs)
            {
                //Create the Panel
                oM.Environment.Elements.Panel aPanel = Create.Panel(BH.Engine.Geometry.Create.PolyCurve(lst));
                aResult.Add(aPanel);
            }

            return aResult;
        }

        /***************************************************/

        internal static List<BH.oM.Environment.Elements.Panel> ToBHoMBuildingElementPanels(this Element element, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            return ToBHoMBuildingElementPanels(element.get_Geometry(new Options()), pullSettings);
        }

        /***************************************************/

        internal static List<BH.oM.Environment.Elements.Panel> ToBHoMBuildingElementPanels(this RoofBase roofBase, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            return ToBHoMBuildingElementPanels(roofBase.get_Geometry(new Options()), pullSettings);
        }

        /***************************************************/

        internal static List<BH.oM.Environment.Elements.Panel> ToBHoMBuildingElementPanels(this FamilyInstance familyInstance, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<BH.oM.Environment.Elements.Panel> aResult = new List<BH.oM.Environment.Elements.Panel>();

            BH.oM.Environment.Elements.Panel aBuildingElementPanel = Create.Panel(new oM.Geometry.Polyline[] { familyInstance.ToBHoMCurve(pullSettings) as BH.oM.Geometry.Polyline });
            if (aBuildingElementPanel != null)
                aResult.Add(aBuildingElementPanel);

            return aResult;
        }

        /***************************************************/

        internal static List<BH.oM.Environment.Elements.Panel> ToBHoMBuildingElementPanels(this GeometryElement geometryElement, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<BH.oM.Environment.Elements.Panel> aResult = new List<BH.oM.Environment.Elements.Panel>();
            foreach (GeometryObject aGeometryObject in geometryElement)
            {
                Solid aSolid = aGeometryObject as Solid;
                if (aSolid == null)
                    continue;

                PlanarFace aPlanarFace = Query.Top(aSolid);
                if (aPlanarFace == null)
                    continue;

                List<BH.oM.Environment.Elements.Panel> aBuildingElementPanelList = aPlanarFace.ToBHoMBuildingElementPanels(pullSettings);
                if (aBuildingElementPanelList == null || aBuildingElementPanelList.Count < 1)
                    continue;

                aResult.AddRange(aBuildingElementPanelList);
            }

            return aResult;

        }

        /***************************************************/
    }
}