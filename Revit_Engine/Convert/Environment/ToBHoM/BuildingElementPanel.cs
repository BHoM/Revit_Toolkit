using System;
using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.oM.Environment.Elements;
using BH.Engine.Environment;
using BH.oM.Adapters.Revit;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static List<BuildingElementPanel> ToBHoMBuildingElementPanels(this PlanarFace planarFace, bool convertUnits = true)
        {
            EdgeArrayArray aEdgeArrayArray = planarFace.EdgeLoops;
            if (aEdgeArrayArray != null && aEdgeArrayArray.Size > 0)
            {
                List<BuildingElementPanel> aResult = new List<BuildingElementPanel>();
                for (int i = 0; i < aEdgeArrayArray.Size; i++)
                {
                    EdgeArray aEdgeArray = aEdgeArrayArray.get_Item(i);
                    List<oM.Geometry.ICurve> aCurveList = new List<oM.Geometry.ICurve>();
                    foreach (Edge aEdge in aEdgeArray)
                    {
                        Curve aCurve = aEdge.AsCurve();
                        if (aCurve != null)
                            aCurveList.Add(aCurve.ToBHoM(convertUnits));
                    }

                    if (aCurveList != null && aCurveList.Count > 0)
                    {
                        BuildingElementPanel aBuildingElementPanel = new BuildingElementPanel();
                        aBuildingElementPanel = aBuildingElementPanel.SetGeometry(Geometry.Create.PolyCurve(aCurveList));
                        aResult.Add(aBuildingElementPanel);
                    }
                }
                return aResult;
            }
            return null;
        }

        /***************************************************/

        public static List<BuildingElementPanel> ToBHoMBuildingElementPanels(this Element element, bool convertUnits = true)
        {
            return ToBHoMBuildingElementPanels(element.get_Geometry(new Options()), convertUnits);
        }

        /***************************************************/

        public static List<BuildingElementPanel> ToBHoMBuildingElementPanels(this RoofBase roofBase, bool convertUnits = true)
        {
            return ToBHoMBuildingElementPanels(roofBase.get_Geometry(new Options()), convertUnits);
        }

        /***************************************************/

        public static List<BuildingElementPanel> ToBHoMBuildingElementPanels(this FamilyInstance familyInstance, bool convertUnits = true)
        {
            List<BuildingElementPanel> aResult = new List<BuildingElementPanel>();

            //TODO: Get more accurate shape. Currently, Windows and doors goes as rectangular panel
            BoundingBoxXYZ aBoundingBoxXYZ = familyInstance.get_BoundingBox(null);

            XYZ aVector = aBoundingBoxXYZ.Max - aBoundingBoxXYZ.Min;

            double aWidth = Math.Abs(aVector.Y);
            double aHeight = Math.Abs(aVector.Z);

            XYZ aVector_Y = (aBoundingBoxXYZ.Transform.BasisY * aWidth) / 2;
            XYZ aVector_Z = (aBoundingBoxXYZ.Transform.BasisZ * aHeight) / 2;

            XYZ aMiddle = (aBoundingBoxXYZ.Max + aBoundingBoxXYZ.Min) / 2;

            XYZ aXYZ_1 = aMiddle + aVector_Z - aVector_Y;
            XYZ aXYZ_2 = aMiddle + aVector_Z + aVector_Y;
            XYZ aXYZ_3 = aMiddle - aVector_Z + aVector_Y;
            XYZ aXYZ_4 = aMiddle - aVector_Z - aVector_Y;

            List<oM.Geometry.Point> aPointList = new List<oM.Geometry.Point>();
            aPointList.Add(aXYZ_1.ToBHoM(convertUnits));
            aPointList.Add(aXYZ_2.ToBHoM(convertUnits));
            aPointList.Add(aXYZ_3.ToBHoM(convertUnits));
            aPointList.Add(aXYZ_4.ToBHoM(convertUnits));
            aPointList.Add(aXYZ_1.ToBHoM(convertUnits));

            BuildingElementPanel aBuildingElementPanel = Create.BuildingElementPanel(new oM.Geometry.Polyline[] { Geometry.Create.Polyline(aPointList) });
            if (aBuildingElementPanel != null)
                aResult.Add(aBuildingElementPanel);

            return aResult;
        }

        /***************************************************/

        public static List<BuildingElementPanel> ToBHoMBuildingElementPanels(this GeometryElement geometryElement, bool convertUnits = true)
        {
            List<BuildingElementPanel> aResult = new List<BuildingElementPanel>();
            foreach (GeometryObject aGeometryObject in geometryElement)
            {
                Solid aSolid = aGeometryObject as Solid;
                if (aSolid == null)
                    continue;

                PlanarFace aPlanarFace = Query.Top(aSolid);
                if (aPlanarFace == null)
                    continue;

                List<BHoMObject> aBHoMObjectList = aPlanarFace.ToBHoM(Discipline.Environmental, convertUnits);
                if (aBHoMObjectList == null || aBHoMObjectList.Count < 1)
                    continue;

                List<BuildingElementPanel> aBuildingElementPanelList = aBHoMObjectList.Cast<BuildingElementPanel>().ToList();
                if (aBuildingElementPanelList != null && aBuildingElementPanelList.Count > 0)
                    aResult.AddRange(aBuildingElementPanelList);
            }

            return aResult;

        }

        /***************************************************/
    }
}
