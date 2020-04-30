/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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
using BH.Engine.Adapters.Revit;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static PolyCurve PolyCurve(this MeshTriangle meshTriangle, RevitSettings settings = null)
        {
            if (meshTriangle == null)
                return null;

            oM.Geometry.Point point1 = meshTriangle.get_Vertex(0).PointFromRevit();
            oM.Geometry.Point point2 = meshTriangle.get_Vertex(1).PointFromRevit();
            oM.Geometry.Point point3 = meshTriangle.get_Vertex(2).PointFromRevit();

            return new PolyCurve { Curves = new List<ICurve> { new BH.oM.Geometry.Line { Start = point1, End = point2 }, new BH.oM.Geometry.Line { Start = point2, End = point3 }, new BH.oM.Geometry.Line { Start = point3, End = point1 } } };
        }

        /***************************************************/

        public static PolyCurve PolyCurve(this FamilyInstance familyInstance, RevitSettings settings = null)
        {
            if (familyInstance == null)
                return null;

            HostObject hostObject = familyInstance.Host as HostObject;
            if (hostObject == null)
                return null;

            List<PolyCurve> polycurves = hostObject.Profiles(settings);
            if (polycurves == null || polycurves.Count == 0)
                return null;

            List<oM.Geometry.Plane> planes = new List<oM.Geometry.Plane>();
            foreach (PolyCurve polycurve in polycurves)
            {
                oM.Geometry.Plane tempPlane = polycurve.Plane();
                if (tempPlane != null)
                    planes.Add(tempPlane);
            }

            //TODO: Get geometry from Host
            List<ICurve> curves = familyInstance.Curves(new Options(), settings);
            if (curves == null || curves.Count == 0)
            {
                if (hostObject == null)
                    return null;

                List<Solid> solids = hostObject.Solids(new Options(), settings);
                if (solids == null || solids.Count == 0)
                    return null;

                PolyCurve polycurve = familyInstance.PolyCurve(solids, planes, settings);
                if (polycurve != null)
                    return polycurve;
            }

            if (curves == null || curves.Count == 0)
                return null;

            double min = double.MaxValue;
            oM.Geometry.Plane plane = null;
            foreach (ICurve curve in curves)
            {
                List<oM.Geometry.Point> tempPoints = curve.IControlPoints();
                if (tempPoints == null || tempPoints.Count == 0)
                    continue;

                foreach (oM.Geometry.Plane tempPlane in planes)
                {
                    double tempMin = tempPoints.ConvertAll(x => x.Distance(tempPlane)).Min();
                    if (tempMin < min)
                    {
                        plane = tempPlane;
                        min = tempMin;
                    }
                }
            }

            BoundingBox bbox = null;
            List<oM.Geometry.Point> points = new List<oM.Geometry.Point>();
            foreach (ICurve curve in curves)
            {
                ICurve tempCurve = null;
                try
                {
                    tempCurve = curve.IProject(plane);
                }
                catch
                {
                    //TODO: to be fixed in Geometry engine case when normal of arc is diferent to normal of a plane
                    //aICurve_Temp = BH.Engine.Geometry.Modify.IProject(aICurve, aPlane);
                }

                if (tempCurve == null)
                    continue;

                if (bbox == null)
                    bbox = tempCurve.IBounds();
                else
                    bbox += tempCurve.IBounds();
               
                //TODO: Issue with projecting to proper type - workaround solution: recognise object and call ControlPoints instead IControlPoints
                if (tempCurve is oM.Geometry.Arc)
                {
                    points.AddRange(((oM.Geometry.Arc)tempCurve).ControlPoints());
                }
                else if (tempCurve is Polyline)
                {
                    points.AddRange(((Polyline)tempCurve).ControlPoints());
                }
                else
                {
                    points.AddRange(tempCurve.IControlPoints());
                }

            }

            XYZ handOrientation = familyInstance.HandOrientation;
            Vector handDirection = new Vector { X = handOrientation.X, Y = handOrientation.Y, Z = handOrientation.Z };
            handDirection = handDirection.Project(plane).Normalise();

            Vector upDirection = handDirection.CrossProduct(plane.Normal).Normalise();

            double maxUp = double.MinValue;
            double maxHand = double.MinValue;

            for (int i = 0; i < points.Count; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    double dotProduct;

                    Vector vector = new Vector { X = points[i].X - points[j].X, Y = points[i].Y - points[j].Y, Z = points[i].Z - points[j].Z };

                    dotProduct = vector.DotProduct(handDirection);
                    if (dotProduct > 0)
                    {
                        Vector tempVector = handDirection * dotProduct;
                        double hand = tempVector.Length();
                        if (hand > maxHand)
                            maxHand = hand;
                    }

                    dotProduct = vector.DotProduct(upDirection);
                    if (dotProduct > 0)
                    {
                        Vector tempVector = upDirection * dotProduct;
                        double up = tempVector.Length();
                        if (up > maxUp)
                            maxUp = up;
                    }
                }
            }

            if (maxUp < 0 || maxHand < 0)
                return null;

            upDirection = upDirection * maxUp / 2;
            handDirection = handDirection * maxHand / 2;

            oM.Geometry.Point centre = bbox.Centre();
            oM.Geometry.Point point1 = centre + upDirection + handDirection;
            oM.Geometry.Point point2 = centre + upDirection - handDirection;
            oM.Geometry.Point point3 = centre - upDirection - handDirection;
            oM.Geometry.Point point4 = centre - upDirection + handDirection;

            return new PolyCurve { Curves = new List<ICurve> { new BH.oM.Geometry.Line { Start = point1, End = point2 }, new BH.oM.Geometry.Line { Start = point2, End = point3 }, new BH.oM.Geometry.Line { Start = point3, End = point4 }, new BH.oM.Geometry.Line { Start = point4, End = point1 } } };
        }

        /***************************************************/

        public static PolyCurve PolyCurve(this FamilyInstance familyInstance, HostObject hostObject, RevitSettings settings = null)
        {
            if (hostObject == null)
                return null;

            List<PolyCurve> polycurves = hostObject.Profiles(settings);
            if (polycurves == null || polycurves.Count == 0)
                return null;

            List<oM.Geometry.Plane> planes = new List<oM.Geometry.Plane>();
            foreach (PolyCurve polycurve in polycurves)
            {
                oM.Geometry.Plane tempPlane = polycurve.Plane();
                if (tempPlane != null)
                    planes.Add(tempPlane);
            }

            List<Solid> solids = hostObject.Solids(new Options(), settings);
            if (solids == null || solids.Count == 0)
                return null;

            return familyInstance.PolyCurve(solids, planes, settings);
        }


        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        private static PolyCurve PolyCurve(this FamilyInstance familyInstance, IEnumerable<Solid> solids, IEnumerable<oM.Geometry.Plane> planes, RevitSettings settings = null)
        {
            if (familyInstance == null || solids == null || planes == null)
                return null;

            foreach (oM.Geometry.Plane plane in planes)
            {
                if (plane == null)
                    continue;

                Autodesk.Revit.DB.Plane revitPlane = plane.ToRevit();
                if (revitPlane == null)
                    continue;

                XYZ tempNormal = plane.Normal.ToRevit();
                tempNormal = tempNormal.Normalize();

                BoundingBoxXYZ bboxXYZ = familyInstance.get_BoundingBox(null);

                foreach (Solid solid in solids)
                {
                    Solid tempSolid = BooleanOperationsUtils.CutWithHalfSpace(solid, revitPlane);
                    if (tempSolid == null || tempSolid.Faces == null || tempSolid.Faces.Size == 0)
                        continue;

                    List<PlanarFace> planarFaces = new List<PlanarFace>();
                    foreach (Autodesk.Revit.DB.Face face in tempSolid.Faces)
                    {
                        PlanarFace planarFace = face as PlanarFace;

                        if (planarFace == null)
                            continue;

                        if (planarFace.FaceNormal.IsAlmostEqualTo(tempNormal, Tolerance.Distance))
                            planarFaces.Add(planarFace);
                    }

                    if (planarFaces == null || planarFaces.Count == 0)
                        continue;

                    List<ICurve> tempCurves = new List<ICurve>();
                    foreach (PlanarFace planarFace in planarFaces)
                    {
                        foreach (EdgeArray edges in planarFace.EdgeLoops)
                        {
                            foreach (Edge edge in edges)
                            {
                                Curve curve = edge.AsCurve();
                                if (curve == null)
                                    continue;

                                if (bboxXYZ.IsContaining(curve.GetEndPoint(0), true) && bboxXYZ.IsContaining(curve.GetEndPoint(1), true))
                                    tempCurves.Add(curve.IFromRevit());
                            }
                        }
                    }

                    if (tempCurves == null || tempCurves.Count == 0)
                        continue;

                    List<PolyCurve> result = BH.Engine.Geometry.Compute.IJoin(tempCurves);
                    if (result == null || result.Count == 0)
                        continue;

                    result.RemoveAll(x => x == null);
                    result.Sort((x, y) => y.Length().CompareTo(y.Length()));

                    PolyCurve pcurve = result.First();

                    if (!pcurve.IsClosed() && pcurve != null)
                    {
                        List<oM.Geometry.Point> points = pcurve.ControlPoints();
                        if (points != null && points.Count > 2)
                            pcurve.Curves.Add(new BH.oM.Geometry.Line { Start = points.Last(), End = points.First() });
                    }

                    return pcurve;
                }
            }

            return null;
        }

        /***************************************************/
    }
}
