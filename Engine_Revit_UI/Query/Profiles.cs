/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

            Document aDocument = hostObject.Document;

            

            IEnumerable<ElementId> aElementIds = null;
            using (Transaction aTransaction = new Transaction(aDocument, "Temp"))
            {
                FailureHandlingOptions aFailureHandlingOptions = aTransaction.GetFailureHandlingOptions().SetClearAfterRollback(true);

                //IMPORTANT: have to be two separate transactions othewise HostObject become Invalid

                aTransaction.Start();
                try
                {
                    aElementIds = aDocument.Delete(hostObject.Id);
                }
                catch(Exception aException)
                {
                    aElementIds = null;
                }

                aTransaction.RollBack(aFailureHandlingOptions);

                aTransaction.Start();
                try
                {
                    IList<ElementId> aElementId_Inserts = hostObject.FindInserts(true, true, true, true);
                    if (aElementId_Inserts != null && aElementId_Inserts.Count > 0)
                    {
                        IEnumerable<ElementId> aElementIds_Temp = aDocument.Delete(aElementId_Inserts);
                        if (aElementIds_Temp != null && aElementIds_Temp.Count() != 0)
                            aElementIds = aElementIds.ToList().FindAll(x => !aElementIds_Temp.Contains(x));
                    }
                }
                catch (Exception aException)
                {

                }
                aTransaction.RollBack(aFailureHandlingOptions);
            }

            List<PolyCurve> aResult = new List<PolyCurve>();
            if (aElementIds != null && aElementIds.Count() > 0)
            {
                Level aLevel = aDocument.GetElement(hostObject.LevelId) as Level;

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

                        List<PolyCurve> aPolyCurveList = Convert.ToBHoM(aSketch, pullSettings);
                        if (aPolyCurveList == null)
                            continue;

                        foreach (PolyCurve aPolyCurve in aPolyCurveList)
                            if (aPolyCurve != null)
                                aResult.Add(aPolyCurve);
                    }
                }

                //if (aLevel != null && aResult.Count > 0)
                //{
                //    //double aElevation = aLevel.Elevation;
                //    //if (pullSettings.ConvertUnits)
                //    //    aElevation = aElevation.ToSI(UnitType.UnitType.UT_Length);

                //    //Parameter aParameter = hostObject.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET);
                //    //if (aParameter != null)
                //    //{
                //    //    double aOffset = aParameter.AsDouble().ToSI(UnitType.UnitType.UT_Length);
                //    //    aElevation = aElevation + aOffset;
                //    //}

                //    //Vector aVector = BH.Engine.Geometry.Create.Vector(0, 0, aElevation);
                //    BoundingBoxXYZ aBoundingBoxXYZ = hostObject.get_BoundingBox(null);

                //    //Vector aVector = BH.Engine.Geometry.Create.Vector(0, 0, aBoundingBoxXYZ.Min.Z);
                //    //for (int i = 0; i < aResult.Count; i++)
                //    //    aResult[i] = BH.Engine.Geometry.Modify.Translate(aResult[i], -aVector);
                //}
            }

            if (hostObject is Wall && aResult.Count == 0)
                return Profiles_Wall((Wall)hostObject, pullSettings);

            return aResult;
        }

        public static List<PolyCurve> Profiles(this SpatialElement spatialElement, PullSettings pullSettings = null)
        {
            if (spatialElement == null)
                return null;
            IList<IList<BoundarySegment>> boundarySegments = spatialElement.GetBoundarySegments(new SpatialElementBoundaryOptions());
            if (boundarySegments == null)
                return null;

            List<PolyCurve> aResults = new List<PolyCurve>();

            foreach (IList<BoundarySegment> boundarySegments_List in boundarySegments)
            {
                if (boundarySegments_List == null)
                    continue;
                List<BH.oM.Geometry.ICurve> aCurveList = new List<ICurve>();
                foreach (BoundarySegment aboundarySegment in boundarySegments_List)
                {
                    aCurveList.Add(Convert.ToBHoM(aboundarySegment.GetCurve(), pullSettings));
                }
                aResults.Add(BH.Engine.Geometry.Create.PolyCurve(aCurveList));
            }

            return aResults;
        }


        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        public static List<PolyCurve> Profiles_Wall(this Wall wall, PullSettings pullSettings = null)
        {
            List<PolyCurve> aPolyCurveList = null;

            BoundingBoxXYZ aBoundingBoxXYZ = wall.get_BoundingBox(null);
            if (aBoundingBoxXYZ != null)
            {
                LocationCurve aLocationCurve = wall.Location as LocationCurve;
                if (aLocationCurve != null)
                {
                    ICurve aCurve = Convert.ToBHoM(aLocationCurve, pullSettings);
                    if (aCurve != null)
                    {
                        oM.Geometry.Plane aPlane = null;

                        double aMax = aBoundingBoxXYZ.Max.Z;
                        if (pullSettings != null && pullSettings.ConvertUnits)
                            aMax = aMax.ToSI(UnitType.UT_Length);

                        aPlane = BH.Engine.Geometry.Create.Plane(BH.Engine.Geometry.Create.Point(0, 0, aMax), BH.Engine.Geometry.Create.Vector(0, 0, 1));
                        ICurve aCurve_Max = BH.Engine.Geometry.Modify.IProject(aCurve, aPlane);

                        double aMin = aBoundingBoxXYZ.Min.Z;
                        if (pullSettings != null && pullSettings.ConvertUnits)
                            aMin = aMin.ToSI(UnitType.UT_Length);

                        aPlane = BH.Engine.Geometry.Create.Plane(BH.Engine.Geometry.Create.Point(0, 0, aMin), BH.Engine.Geometry.Create.Vector(0, 0, 1));
                        ICurve aCurve_Min = BH.Engine.Geometry.Modify.IProject(aCurve, aPlane);

                        oM.Geometry.Point aPoint_1;
                        oM.Geometry.Point aPoint_2;
                        oM.Geometry.Point aPoint_3;

                        aPoint_1 = BH.Engine.Geometry.Query.IEndPoint(aCurve_Min);
                        aPoint_2 = BH.Engine.Geometry.Query.IStartPoint(aCurve_Max);
                        aPoint_3 = BH.Engine.Geometry.Query.IEndPoint(aCurve_Max);
                        if (BH.Engine.Geometry.Query.Distance(aPoint_1, aPoint_3) < BH.Engine.Geometry.Query.Distance(aPoint_1, aPoint_2))
                        {
                            oM.Geometry.Point aPoint_Temp = aPoint_2;

                            aCurve_Max = BH.Engine.Geometry.Modify.IFlip(aCurve_Max);
                            aPoint_2 = aPoint_3;
                            aPoint_3 = aPoint_Temp;
                        }

                        oM.Geometry.Line aLine_1 = BH.Engine.Geometry.Create.Line(aPoint_1, aPoint_2);
                        oM.Geometry.Line aLine_2 = BH.Engine.Geometry.Create.Line(aPoint_3, BH.Engine.Geometry.Query.IStartPoint(aCurve_Min));

                        aPolyCurveList = new List<PolyCurve>();
                        aPolyCurveList.Add(BH.Engine.Geometry.Create.PolyCurve(new ICurve[] { aCurve_Min, aLine_1, aCurve_Max, aLine_2 }));
                        return aPolyCurveList;
                    }

                }
            }
            
            if (!ExporterIFCUtils.HasElevationProfile(wall))
                return null;

            IList<CurveLoop> aCurveLoopList = ExporterIFCUtils.GetElevationProfile(wall);
            if (aCurveLoopList == null)
                return null;

            aPolyCurveList = new List<PolyCurve>();
            foreach (CurveLoop aCurveLoop in aCurveLoopList)
            {
                PolyCurve aPolyCurve = Convert.ToBHoM(aCurveLoop, pullSettings);
                if (aPolyCurve != null)
                    aPolyCurveList.Add(aPolyCurve);
            }
            return aPolyCurveList;
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
