/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.Engine.Geometry;
using BH.oM.Geometry;
using BH.oM.Adapters.Revit.Settings;


namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static PolyCurve PolyCurve(this MeshTriangle meshTriangle, PullSettings pullSettings = null)
        {
            if (meshTriangle == null)
                return null;

            oM.Geometry.Point aPoint_1 = meshTriangle.get_Vertex(0).ToBHoM();
            oM.Geometry.Point aPoint_2 = meshTriangle.get_Vertex(1).ToBHoM();
            oM.Geometry.Point aPoint_3 = meshTriangle.get_Vertex(2).ToBHoM();

            return Create.PolyCurve(new ICurve[] { Create.Line(aPoint_1, aPoint_2), Create.Line(aPoint_2, aPoint_3), Create.Line(aPoint_3, aPoint_1) });
        }

        /***************************************************/

        public static PolyCurve PolyCurve(this FamilyInstance familyInstance, PullSettings pullSettings = null)
        {
            if (familyInstance == null)
                return null;

            HostObject aHostObject = familyInstance.Host as HostObject;
            if (aHostObject == null)
                return null;

            List<PolyCurve> aPolyCurveList = Profiles(aHostObject, pullSettings);
            if (aPolyCurveList == null || aPolyCurveList.Count == 0)
                return null;

            List<oM.Geometry.Plane> aPlaneList = new List<oM.Geometry.Plane>();
            foreach (PolyCurve aPolyCurve in aPolyCurveList)
            {
                oM.Geometry.Plane aPlane_Temp = BH.Engine.Adapters.Revit.Query.Plane(aPolyCurve);
                if (aPlane_Temp != null)
                    aPlaneList.Add(aPlane_Temp);
            }

            //TODO: Get geometry from Host
            List<ICurve> aCurveList = Query.Curves(familyInstance, new Options(), pullSettings);
            if (aCurveList == null || aCurveList.Count == 0)
            {
                if (aHostObject == null)
                    return null;

                List<Solid> aSolidList = aHostObject.Solids(new Options(), pullSettings);
                if (aSolidList == null || aSolidList.Count == 0)
                    return null;

                PolyCurve aPolyCurve = PolyCurve(familyInstance, aSolidList, aPlaneList, pullSettings);
                if (aPolyCurve != null)
                    return aPolyCurve;
            }

            if (aCurveList == null || aCurveList.Count == 0)
                return null;

            double aMin = double.MaxValue;
            oM.Geometry.Plane aPlane = null;
            foreach (ICurve aICurve in aCurveList)
            {
                List<oM.Geometry.Point> aPointList_Temp = BH.Engine.Geometry.Query.IControlPoints(aICurve);
                if (aPointList_Temp == null || aPointList_Temp.Count == 0)
                    continue;

                foreach (oM.Geometry.Plane aPlane_Temp in aPlaneList)
                {
                    double aMin_Temp = aPointList_Temp.ConvertAll(x => BH.Engine.Geometry.Query.Distance(x, aPlane_Temp)).Min();
                    if (aMin_Temp < aMin)
                    {
                        aPlane = aPlane_Temp;
                        aMin = aMin_Temp;
                    }
                }
            }

            BoundingBox aBoundingBox = null;
            List<oM.Geometry.Point> aPointList = new List<oM.Geometry.Point>();
            foreach (ICurve aICurve in aCurveList)
            {
                ICurve aICurve_Temp = null;
                try
                {
                    aICurve_Temp = BH.Engine.Geometry.Modify.IProject(aICurve, aPlane);
                }
                catch (System.Exception aException)
                {
                    //TODO: to be fixed in Geometry engine case when normal of arc is diferent to normal of a plane
                    //aICurve_Temp = BH.Engine.Geometry.Modify.IProject(aICurve, aPlane);
                }

                if (aICurve_Temp == null)
                    continue;

                if (aBoundingBox == null)
                    aBoundingBox = BH.Engine.Geometry.Query.IBounds(aICurve_Temp);
                else
                    aBoundingBox += BH.Engine.Geometry.Query.IBounds(aICurve_Temp);


                //TODO: Issue with projecting to proper type - workaround solution: recognise object and call ControlPoints instead IControlPoints
                if (aICurve_Temp is oM.Geometry.Arc)
                {
                    aPointList.AddRange(((oM.Geometry.Arc)aICurve_Temp).ControlPoints());
                }
                else if (aICurve_Temp is Polyline)
                {
                    aPointList.AddRange(((Polyline)aICurve_Temp).ControlPoints());
                }
                else
                {
                    aPointList.AddRange(aICurve_Temp.IControlPoints());
                }

            }

            XYZ aHandOrientation = familyInstance.HandOrientation;
            Vector aHandDirection = Create.Vector(aHandOrientation.X, aHandOrientation.Y, aHandOrientation.Z);
            aHandDirection = BH.Engine.Geometry.Modify.Project(aHandDirection, aPlane).Normalise();

            Vector aUpDirection = BH.Engine.Geometry.Query.CrossProduct(aHandDirection, aPlane.Normal).Normalise();

            double aMax_Up = double.MinValue;
            double aMax_Hand = double.MinValue;

            for (int i = 0; i < aPointList.Count; i++)
            {
                for (int j = i + 1; j < aPointList.Count; j++)
                {
                    double aDotProduct;

                    Vector aVector = Create.Vector(aPointList[i].X - aPointList[j].X, aPointList[i].Y - aPointList[j].Y, aPointList[i].Z - aPointList[j].Z);

                    aDotProduct = BH.Engine.Geometry.Query.DotProduct(aVector, aHandDirection);
                    if (aDotProduct > 0)
                    {
                        Vector aVector_Temp = aHandDirection * aDotProduct;
                        double aHand = BH.Engine.Geometry.Query.Length(aVector_Temp);
                        if (aHand > aMax_Hand)
                            aMax_Hand = aHand;
                    }

                    aDotProduct = BH.Engine.Geometry.Query.DotProduct(aVector, aUpDirection);
                    if (aDotProduct > 0)
                    {
                        Vector aVector_Temp = aUpDirection * aDotProduct;
                        double aUp = BH.Engine.Geometry.Query.Length(aVector_Temp);
                        if (aUp > aMax_Up)
                            aMax_Up = aUp;
                    }
                }
            }

            if (aMax_Up < 0 || aMax_Hand < 0)
                return null;

            aUpDirection = aUpDirection * aMax_Up / 2;
            aHandDirection = aHandDirection * aMax_Hand / 2;

            oM.Geometry.Point aCenter = BH.Engine.Geometry.Query.Centre(aBoundingBox);

            oM.Geometry.Point aPoint_1 = BH.Engine.Geometry.Modify.Translate(aCenter, aUpDirection);
            aPoint_1 = BH.Engine.Geometry.Modify.Translate(aPoint_1, aHandDirection);

            oM.Geometry.Point aPoint_2 = BH.Engine.Geometry.Modify.Translate(aCenter, aUpDirection);
            aPoint_2 = BH.Engine.Geometry.Modify.Translate(aPoint_2, -aHandDirection);

            oM.Geometry.Point aPoint_3 = BH.Engine.Geometry.Modify.Translate(aCenter, -aUpDirection);
            aPoint_3 = BH.Engine.Geometry.Modify.Translate(aPoint_3, -aHandDirection);

            oM.Geometry.Point aPoint_4 = BH.Engine.Geometry.Modify.Translate(aCenter, -aUpDirection);
            aPoint_4 = BH.Engine.Geometry.Modify.Translate(aPoint_4, aHandDirection);

            return Create.PolyCurve(new oM.Geometry.Line[] { Create.Line(aPoint_1, aPoint_2), Create.Line(aPoint_2, aPoint_3), Create.Line(aPoint_3, aPoint_4), Create.Line(aPoint_4, aPoint_1) });
        }

        /***************************************************/

        public static PolyCurve PolyCurve(this FamilyInstance familyInstance, HostObject hostObject, PullSettings pullSettings = null)
        {
            if (hostObject == null)
                return null;

            List<PolyCurve> aPolyCurveList = Profiles(hostObject, pullSettings);
            if (aPolyCurveList == null || aPolyCurveList.Count == 0)
                return null;

            List<oM.Geometry.Plane> aPlaneList = new List<oM.Geometry.Plane>();
            foreach (PolyCurve aPolyCurve in aPolyCurveList)
            {
                oM.Geometry.Plane aPlane_Temp = BH.Engine.Adapters.Revit.Query.Plane(aPolyCurve);
                if (aPlane_Temp != null)
                    aPlaneList.Add(aPlane_Temp);
            }

            List<Solid> aSolidList = hostObject.Solids(new Options(), pullSettings);
            if (aSolidList == null || aSolidList.Count == 0)
                return null;

            return PolyCurve(familyInstance, aSolidList, aPlaneList, pullSettings);
        }


        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        private static PolyCurve PolyCurve(this FamilyInstance familyInstance, IEnumerable<Solid> solids, IEnumerable<oM.Geometry.Plane> planes, PullSettings pullSettings = null)
        {
            if (familyInstance == null || solids == null || planes == null)
                return null;

            foreach (oM.Geometry.Plane aPlane in planes)
            {
                if (aPlane == null)
                    continue;

                Autodesk.Revit.DB.Plane aPlane_Revit = Convert.ToRevitPlane(aPlane);
                if (aPlane_Revit == null)
                    continue;

                XYZ aNormal_Temp = aPlane.Normal.ToRevitXYZ();
                aNormal_Temp = aNormal_Temp.Normalize();

                BoundingBoxXYZ aBoundingBoxXYZ = familyInstance.get_BoundingBox(null);

                foreach (Solid aSolid in solids)
                {
                    Solid aSolid_Temp = BooleanOperationsUtils.CutWithHalfSpace(aSolid, aPlane_Revit);
                    if (aSolid_Temp == null || aSolid_Temp.Faces == null || aSolid_Temp.Faces.Size == 0)
                        continue;

                    List<PlanarFace> aPlanarFaceList = new List<PlanarFace>();
                    foreach (Autodesk.Revit.DB.Face aFace in aSolid_Temp.Faces)
                    {
                        PlanarFace aPlanarFace = aFace as PlanarFace;

                        if (aPlanarFace == null)
                            continue;

                        if (aPlanarFace.FaceNormal.IsAlmostEqualTo(aNormal_Temp, Tolerance.Distance))
                            aPlanarFaceList.Add(aPlanarFace);
                    }

                    if (aPlanarFaceList == null || aPlanarFaceList.Count == 0)
                        continue;

                    List<ICurve> aCurveList_Temp = new List<ICurve>();
                    foreach (PlanarFace aPlanarFace in aPlanarFaceList)
                    {
                        foreach (EdgeArray aEdgeArray in aPlanarFace.EdgeLoops)
                        {
                            foreach (Edge aEdge in aEdgeArray)
                            {
                                Curve aCurve = aEdge.AsCurve();
                                if (aCurve == null)
                                    continue;

                                if (IsContaining(aBoundingBoxXYZ, aCurve.GetEndPoint(0), true) && IsContaining(aBoundingBoxXYZ, aCurve.GetEndPoint(1), true))
                                    aCurveList_Temp.Add(aCurve.ToBHoM());
                            }
                        }
                    }

                    if (aCurveList_Temp == null || aCurveList_Temp.Count == 0)
                        continue;

                    List<PolyCurve> aResult = BH.Engine.Geometry.Modify.IJoin(aCurveList_Temp);
                    if (aResult == null || aResult.Count == 0)
                        continue;

                    aResult.RemoveAll(x => x == null);
                    aResult.Sort((x, y) => y.Length().CompareTo(y.Length()));

                    PolyCurve aPolyCurve = aResult.First();

                    if (!BH.Engine.Geometry.Query.IsClosed(aPolyCurve) && aPolyCurve != null)
                    {
                        List<oM.Geometry.Point> aPoints = aPolyCurve.ControlPoints();
                        if (aPoints != null && aPoints.Count > 2)
                            aPolyCurve.Curves.Add(Create.Line(aPoints.Last(), aPoints.First()));
                    }

                    return aPolyCurve;
                }
            }

            return null;
        }

        /***************************************************/
    }
}