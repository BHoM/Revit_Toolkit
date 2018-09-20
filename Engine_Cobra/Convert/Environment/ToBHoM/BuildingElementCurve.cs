using Autodesk.Revit.DB;

using BH.oM.Environment.Elements;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;

using System;
using System.Collections.Generic;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static ICurve ToBHoMCurve(this Wall wall, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            return (wall.Location as LocationCurve).ToBHoM(pullSettings);
        }

        internal static ICurve ToBHoMCurve(this FamilyInstance familyInstance, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

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
            aPointList.Add(aXYZ_1.ToBHoM(pullSettings));
            aPointList.Add(aXYZ_2.ToBHoM(pullSettings));
            aPointList.Add(aXYZ_3.ToBHoM(pullSettings));
            aPointList.Add(aXYZ_4.ToBHoM(pullSettings));
            aPointList.Add(aXYZ_1.ToBHoM(pullSettings));

            return BH.Engine.Geometry.Create.Polyline(aPointList);
        }

        internal static List<ICurve> ToBHoMCurve(this Element element, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            return ToBHoMCurve(element.get_Geometry(new Options()), pullSettings);
        }

        internal static List<ICurve> ToBHoMCurve(this GeometryElement geometryElement, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<ICurve> aResult = new List<ICurve>();
            foreach (GeometryObject aGeometryObject in geometryElement)
            {
                Solid aSolid = aGeometryObject as Solid;
                if (aSolid == null)
                    continue;

                PlanarFace aPlanarFace = Query.Top(aSolid);
                if (aPlanarFace == null)
                    continue;

                EdgeArrayArray aEdgeArrayArray = aPlanarFace.EdgeLoops;
                if (aEdgeArrayArray == null && aEdgeArrayArray.Size == 0)
                    continue;

                for (int i = 0; i < aEdgeArrayArray.Size; i++)
                {
                    EdgeArray aEdgeArray = aEdgeArrayArray.get_Item(i);
                    List<oM.Geometry.ICurve> aCurveList = new List<oM.Geometry.ICurve>();
                    foreach (Edge aEdge in aEdgeArray)
                    {
                        Curve aCurve = aEdge.AsCurve();
                        if (aCurve != null)
                            aCurveList.Add(aCurve.ToBHoM(pullSettings));
                    }

                    if (aCurveList != null && aCurveList.Count > 0)
                        aResult.AddRange(aCurveList);
                }
            }

            return aResult;
        }

        /***************************************************/
    }
}