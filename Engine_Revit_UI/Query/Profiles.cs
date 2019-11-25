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

using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit.Settings;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB.IFC;
using BH.oM.Geometry;
using System;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static List<PolyCurve> Profiles(this HostObject hostObject, PullSettings pullSettings = null)
        {
            if (hostObject == null || hostObject.Document == null)
                return null;

            if (hostObject is Floor)
                return Profiles_Floor((Floor)hostObject, pullSettings);

            if (hostObject is RoofBase)
                return Profiles_RoofBase((RoofBase)hostObject, pullSettings);


            if (hostObject is Ceiling)
                return Profiles_Ceiling((Ceiling)hostObject, pullSettings);

            Document document = hostObject.Document;

            IEnumerable<ElementId> elementIDs = null;
            using (Transaction transaction = new Transaction(document, "Temp"))
            {
                FailureHandlingOptions aFailureHandlingOptions = transaction.GetFailureHandlingOptions().SetClearAfterRollback(true);

                //IMPORTANT: have to be two separate transactions othewise HostObject become Invalid

                transaction.Start();
                try
                {
                    elementIDs = document.Delete(hostObject.Id);
                }
                catch
                {
                    elementIDs = null;
                }

                transaction.RollBack(aFailureHandlingOptions);

                transaction.Start();
                try
                {
                    IList<ElementId> insertElementIDs = hostObject.FindInserts(true, true, true, true);
                    if (insertElementIDs != null && insertElementIDs.Count > 0)
                    {
                        IEnumerable<ElementId> tempElementIDs = document.Delete(insertElementIDs);
                        if (tempElementIDs != null && tempElementIDs.Count() != 0)
                            elementIDs = elementIDs.ToList().FindAll(x => !tempElementIDs.Contains(x));
                    }
                }
                catch
                {

                }
                transaction.RollBack(aFailureHandlingOptions);
            }

            List<PolyCurve> result = new List<PolyCurve>();
            if (elementIDs != null && elementIDs.Count() > 0)
            {
                Level level = document.GetElement(hostObject.LevelId) as Level;

                foreach (ElementId id in elementIDs)
                {
                    Element element = document.GetElement(id);
                    if (element == null)
                        continue;

                    if (element is Sketch)
                    {
                        Sketch sketch = (Sketch)element;

                        if (sketch.Profile == null)
                            continue;

                        List<PolyCurve> polycurves = Convert.ToBHoM(sketch);
                        if (polycurves == null)
                            continue;

                        foreach (PolyCurve polycurve in polycurves)
                            if (polycurve != null)
                                result.Add(polycurve);
                    }
                }
            }

            if (hostObject is Wall && result.Count == 0)
                return Profiles_Wall((Wall)hostObject, pullSettings);

            return result;
        }

        public static List<PolyCurve> Profiles(this SpatialElement spatialElement, PullSettings pullSettings = null)
        {
            if (spatialElement == null)
                return null;
            IList<IList<BoundarySegment>> boundarySegments = spatialElement.GetBoundarySegments(new SpatialElementBoundaryOptions());
            if (boundarySegments == null)
                return null;

            List<PolyCurve> results = new List<PolyCurve>();

            foreach (IList<BoundarySegment> boundarySegmentList in boundarySegments)
            {
                if (boundarySegmentList == null)
                    continue;
                List<BH.oM.Geometry.ICurve> curves = new List<ICurve>();
                foreach (BoundarySegment boundarySegment in boundarySegmentList)
                {
                    curves.Add(Convert.ToBHoM(boundarySegment.GetCurve()));
                }
                results.Add(BH.Engine.Geometry.Create.PolyCurve(curves));
            }

            return results;
        }


        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        public static List<PolyCurve> Profiles_Wall(this Wall wall, PullSettings pullSettings = null)
        {
            List<PolyCurve> polycurves = null;

            BoundingBoxXYZ bboxXYZ = wall.get_BoundingBox(null);
            if (bboxXYZ != null)
            {
                LocationCurve locationCurve = wall.Location as LocationCurve;
                if (locationCurve != null)
                {
                    ICurve curve = Convert.ToBHoM(locationCurve);
                    if (curve != null)
                    {
                        oM.Geometry.Plane plane = null;

                        double max = bboxXYZ.Max.Z.ToSI(UnitType.UT_Length);
                        plane = BH.Engine.Geometry.Create.Plane(BH.Engine.Geometry.Create.Point(0, 0, max), BH.Engine.Geometry.Create.Vector(0, 0, 1));
                        ICurve maxCurve = BH.Engine.Geometry.Modify.IProject(curve, plane);

                        double min = bboxXYZ.Min.Z.ToSI(UnitType.UT_Length);
                        plane = BH.Engine.Geometry.Create.Plane(BH.Engine.Geometry.Create.Point(0, 0, min), BH.Engine.Geometry.Create.Vector(0, 0, 1));
                        ICurve minCurve = BH.Engine.Geometry.Modify.IProject(curve, plane);

                        oM.Geometry.Point point1;
                        oM.Geometry.Point point2;
                        oM.Geometry.Point point3;

                        point1 = BH.Engine.Geometry.Query.IEndPoint(minCurve);
                        point2 = BH.Engine.Geometry.Query.IStartPoint(maxCurve);
                        point3 = BH.Engine.Geometry.Query.IEndPoint(maxCurve);
                        if (BH.Engine.Geometry.Query.Distance(point1, point3) < BH.Engine.Geometry.Query.Distance(point1, point2))
                        {
                            oM.Geometry.Point tempPoint = point2;

                            maxCurve = BH.Engine.Geometry.Modify.IFlip(maxCurve);
                            point2 = point3;
                            point3 = tempPoint;
                        }

                        oM.Geometry.Line line1 = BH.Engine.Geometry.Create.Line(point1, point2);
                        oM.Geometry.Line line2 = BH.Engine.Geometry.Create.Line(point3, BH.Engine.Geometry.Query.IStartPoint(minCurve));

                        polycurves = new List<PolyCurve>();
                        polycurves.Add(BH.Engine.Geometry.Create.PolyCurve(new ICurve[] { minCurve, line1, maxCurve, line2 }));
                        return polycurves;
                    }
                }
            }
            
            if (!ExporterIFCUtils.HasElevationProfile(wall))
                return null;

            IList<CurveLoop> curveLoops = ExporterIFCUtils.GetElevationProfile(wall);
            if (curveLoops == null)
                return null;

            polycurves = new List<PolyCurve>();
            foreach (CurveLoop curveLoop in curveLoops)
            {
                PolyCurve polycurve = Convert.ToBHoM(curveLoop);
                if (polycurve != null)
                    polycurves.Add(polycurve);
            }

            return polycurves;
        }

        /***************************************************/

        private static List<PolyCurve> Profiles_Floor(this Floor floor, PullSettings pullSettings = null)
        {
            return TopFacesPolyCurves(floor, pullSettings);
        }

        /***************************************************/

        private static List<PolyCurve> Profiles_RoofBase(this RoofBase roofBase, PullSettings pullSettings = null)
        {
            return TopFacesPolyCurves(roofBase, pullSettings);
        }

        /***************************************************/

        private static List<PolyCurve> Profiles_Ceiling(this Ceiling ceiling, PullSettings pullSettings = null)
        {
            return BottomFacesPolyCurves(ceiling, pullSettings);
        }

        /***************************************************/
    }
}
