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

using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        static public Dictionary<PlanarSurface, List<oM.Physical.Elements.IOpening>> PlanarSurfaceDictionary(this HostObject hostObject, bool pullOpenings = true, PullSettings pullSettings = null)
        {
            List<PolyCurve> aPolyCurveList = Query.Profiles(hostObject, pullSettings);

            List<PolyCurve> aPolyCurveList_Outer = null;
            try
            {
                aPolyCurveList_Outer = BH.Engine.Adapters.Revit.Query.OuterPolyCurves(aPolyCurveList);
            }
            catch (Exception aException)
            {
                aPolyCurveList_Outer = aPolyCurveList;
            }

            if (aPolyCurveList_Outer == null)
                return null;

            List<PlanarSurface> aPlanarSurfaceList_NotPlanar = new List<PlanarSurface>();
            List<PlanarSurface> aPlanarSurfaceList_Planar = new List<PlanarSurface>();
            foreach (PolyCurve aPolyCurve in aPolyCurveList_Outer)
            {
                List<PolyCurve> aPolyCurveList_Inner = null;
                try
                {
                    aPolyCurveList_Inner = BH.Engine.Adapters.Revit.Query.InnerPolyCurves(aPolyCurve, aPolyCurveList);
                }
                catch (Exception aException)
                {

                }

                List<ICurve> aICurveList = new List<ICurve>();
                if (aPolyCurveList_Inner != null && aPolyCurveList_Inner.Count > 0)
                    aICurveList = aPolyCurveList_Inner.ConvertAll(x => (ICurve)x);

                //TODO: Create method in Geometry Engine shall be used however IsClosed method returns false for some of the PolyCurves pulled from Revit
                //PlanarSurface aPlanarSurface = BH.Engine.Geometry.Create.PlanarSurface(aPolyCurve, aPolyCurveList_Inner.ConvertAll(x => (ICurve)x)
                PlanarSurface aPlanarSurface = new PlanarSurface()
                {
                    ExternalBoundary = aPolyCurve,
                    InternalBoundaries = aICurveList
                };

                if (!BH.Engine.Geometry.Query.IIsPlanar(aPlanarSurface.ExternalBoundary))
                {
                    BH.Engine.Reflection.Compute.RecordWarning("Invalid Geometry of ISurface. External Boundary of ISurface is not planar and Openings cannot be pulled.");
                    aPlanarSurfaceList_NotPlanar.Add(aPlanarSurface);
                }
                else
                {
                    aPlanarSurfaceList_Planar.Add(aPlanarSurface);
                }
                    
            }

            if (aPlanarSurfaceList_Planar == null && aPlanarSurfaceList_NotPlanar == null)
                return null;

            Dictionary<PlanarSurface, List<oM.Physical.Elements.IOpening>> aResult = new Dictionary<PlanarSurface, List<oM.Physical.Elements.IOpening>>();

            if(aPlanarSurfaceList_Planar != null && aPlanarSurfaceList_Planar.Count > 0)
            {
                foreach (PlanarSurface aPlanarSurface in aPlanarSurfaceList_Planar)
                    aResult[aPlanarSurface] = null;

                if(pullOpenings)
                {
                    IEnumerable<ElementId> aElementIds_Hosted = hostObject.FindInserts(false, false, false, false);
                    if (aElementIds_Hosted != null && aElementIds_Hosted.Count() > 0)
                    {
                        List<oM.Physical.Elements.IOpening> aOpeningList = new List<oM.Physical.Elements.IOpening>();
                        List<PolyCurve> aPolyCurveList_Internal = new List<PolyCurve>(); 
                        foreach (ElementId aElementId in aElementIds_Hosted)
                        {
                            FamilyInstance aFamilyInstance = hostObject.Document.GetElement(aElementId) as FamilyInstance;
                            if (aFamilyInstance == null)
                                continue;

                            if (aFamilyInstance.Category == null)
                                continue;

                            switch ((BuiltInCategory)(aFamilyInstance.Category.Id.IntegerValue))
                            {
                                case Autodesk.Revit.DB.BuiltInCategory.OST_Windows:
                                    aOpeningList.Add(aFamilyInstance.ToBHoMWindow(pullSettings));
                                    break;
                                case Autodesk.Revit.DB.BuiltInCategory.OST_Doors:
                                    aOpeningList.Add(aFamilyInstance.ToBHoMDoor(pullSettings));
                                    break;
                                default:
                                    PolyCurve aPolyCurve = aFamilyInstance.PolyCurve(hostObject, pullSettings);
                                    if (aPolyCurve != null)
                                        aPolyCurveList_Internal.Add(aPolyCurve);
                                    break;
                            }
                        }

                        if (aOpeningList != null && aOpeningList.Count > 0)
                        {
                            aOpeningList.RemoveAll(x => x == null);
                            foreach (oM.Physical.Elements.IOpening aOpening in aOpeningList)
                            {
                                if (aOpening.Location == null)
                                    continue;

                                PlanarSurface aPlanarSurface = aPlanarSurfaceList_Planar.Find(x => x.IsContaining(aOpening.Location as PlanarSurface));
                                if (aPlanarSurface == null)
                                    continue;

                                List<oM.Physical.Elements.IOpening> aOpeningList_Temp = aResult[aPlanarSurface];
                                if (aOpeningList_Temp == null)
                                {
                                    aOpeningList_Temp = new List<oM.Physical.Elements.IOpening>();
                                    aResult[aPlanarSurface] = aOpeningList_Temp;
                                }

                                aOpeningList_Temp.Add(aOpening);
                            }
                        }

                        if (aPolyCurveList_Internal != null && aPolyCurveList_Internal.Count > 0)
                        {
                            aPolyCurveList_Internal.RemoveAll(x => x == null);
                            foreach (PolyCurve aPolyCurve in aPolyCurveList_Internal)
                            {
                                PlanarSurface aPlanarSurface = aPlanarSurfaceList_Planar.Find(x => x.IsContaining(BH.Engine.Geometry.Query.IControlPoints(aPolyCurve)));
                                if (aPlanarSurface == null)
                                    continue;

                                aPlanarSurface.InternalBoundaries.Add(aPolyCurve);
                            }
                        }
                    }
                }
            }


            foreach (PlanarSurface aPlanarSurface in aPlanarSurfaceList_NotPlanar)
                aResult[aPlanarSurface] = null;

            return aResult;
        }

        /***************************************************/
    }
}