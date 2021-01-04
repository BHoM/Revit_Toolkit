/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using Autodesk.Revit.DB.IFC;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Dictionary<PlanarSurface, List<PlanarSurface>> PanelSurfaces(this HostObject hostObject, IEnumerable<ElementId> insertsToIgnore = null, RevitSettings settings = null)
        {
            Document doc = hostObject.Document;
            settings = settings.DefaultIfNull();
            
            Dictionary<PlanarSurface, List<PlanarSurface>> result = new Dictionary<PlanarSurface, List<PlanarSurface>>();

            List <Autodesk.Revit.DB.Plane> planes = hostObject.IPanelPlanes();
            if (planes.Count == 0)
                return null;

            IList<ElementId> inserts = hostObject.FindInserts(true, true, true, true);
            if (insertsToIgnore != null)
                inserts = inserts.Where(x => insertsToIgnore.All(y => x.IntegerValue != y.IntegerValue)).ToList();

            Transaction t = new Transaction(doc);
            FailureHandlingOptions failureHandlingOptions = t.GetFailureHandlingOptions().SetClearAfterRollback(true);
            t.Start("Temp Delete Inserts And Unjoin Geometry");

            try
            {
                foreach (ElementId id in JoinGeometryUtils.GetJoinedElements(doc, hostObject))
                {
                    JoinGeometryUtils.UnjoinGeometry(doc, hostObject, doc.GetElement(id));
                }

                if (hostObject is Wall)
                {
                    WallUtils.DisallowWallJoinAtEnd((Wall)hostObject, 0);
                    WallUtils.DisallowWallJoinAtEnd((Wall)hostObject, 1);
                }

                if (insertsToIgnore != null)
                    doc.Delete(insertsToIgnore.ToList());

                doc.Regenerate();

                List<Solid> solidsWithOpenings = hostObject.Solids(new Options());
                List<Solid> fullSolids;

                if (inserts.Count != 0)
                {
                    solidsWithOpenings = solidsWithOpenings.Select(x => SolidUtils.Clone(x)).ToList();

                    doc.Delete(inserts);
                    doc.Regenerate();

                    fullSolids = hostObject.Solids(new Options());
                }
                else
                    fullSolids = solidsWithOpenings;

                fullSolids = fullSolids.SelectMany(x => SolidUtils.SplitVolumes(x)).ToList();
                if (hostObject is Wall)
                {
                    fullSolids.ForEach(x => BooleanOperationsUtils.CutWithHalfSpaceModifyingOriginalSolid(x, planes[0]));
                    Autodesk.Revit.DB.Plane flippedPlane = Autodesk.Revit.DB.Plane.CreateByNormalAndOrigin(-planes[0].Normal, planes[0].Origin + planes[0].Normal * 1e-3);
                    fullSolids.ForEach(x => BooleanOperationsUtils.CutWithHalfSpaceModifyingOriginalSolid(x, flippedPlane));
                    fullSolids = fullSolids.SelectMany(x => SolidUtils.SplitVolumes(x)).ToList();
                    planes[0] = Autodesk.Revit.DB.Plane.CreateByNormalAndOrigin(-planes[0].Normal, planes[0].Origin);
                }


                foreach (Autodesk.Revit.DB.Plane plane in planes)
                {
                    foreach (Solid s in fullSolids)
                    {
                        List<CurveLoop> loops = new List<CurveLoop>();
                        foreach (Autodesk.Revit.DB.Face f in s.Faces)
                        {
                            PlanarFace pf = f as PlanarFace;
                            if (pf == null)
                                continue;

                            if (Math.Abs(1 - pf.FaceNormal.DotProduct(plane.Normal)) <= settings.DistanceTolerance && Math.Abs((pf.Origin - plane.Origin).DotProduct(plane.Normal)) <= settings.AngleTolerance)
                                loops.AddRange(pf.GetEdgesAsCurveLoops());
                        }

                        CurveLoop outline = loops.FirstOrDefault(x => x.IsCounterclockwise(plane.Normal));
                        PlanarSurface surface = new PlanarSurface(outline.FromRevit(), null);
                        List<PlanarSurface> openings = new List<PlanarSurface>();
                        foreach (CurveLoop loop in loops.Where(x => x != outline))
                        {
                            openings.Add(new PlanarSurface(loop.FromRevit(), null));
                        }

                        if (inserts.Count != 0)
                        {
                            List<Solid> openingVolumes = new List<Solid>();
                            foreach (Solid s2 in solidsWithOpenings)
                            {
                                openingVolumes.Add(BooleanOperationsUtils.ExecuteBooleanOperation(s, s2, BooleanOperationsType.Difference));
                            }

                            foreach (Solid s2 in openingVolumes)
                            {
                                foreach (Autodesk.Revit.DB.Face f in s2.Faces)
                                {
                                    PlanarFace pf = f as PlanarFace;
                                    if (pf == null)
                                        continue;

                                    if (Math.Abs(1 - pf.FaceNormal.DotProduct(plane.Normal)) <= settings.DistanceTolerance && Math.Abs((pf.Origin - plane.Origin).DotProduct(plane.Normal)) <= settings.AngleTolerance)
                                    {
                                        foreach (CurveLoop cl in pf.GetEdgesAsCurveLoops())
                                        {
                                            openings.Add(new PlanarSurface(cl.FromRevit(), null));
                                        }
                                    }
                                }
                            }
                        }

                        result.Add(surface, openings);
                    }
                }
            }
            catch
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("Geometrical processing of a Revit element failed due to an internal Revit error. Converted panel might be missing one or more of its surfaces. Revit ElementId: {0}", hostObject.Id));
            }

            t.RollBack(failureHandlingOptions);

            return result;
        }

        /***************************************************/
    }
}
