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
using BH.Engine.Adapters.Revit;
using BH.Engine.Geometry;
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
            settings = settings.DefaultIfNull();

            if (!hostObject.Document.IsLinked)
                return hostObject.PanelSurfaces_HostDocument(insertsToIgnore, settings);
            else
            {
                BH.Engine.Reflection.Compute.RecordWarning("Pulling panels and openings from Revit link documents is simplified compared to pulling directly from the host document, therefore it may result in degraded output.\n" +
                                                           "In case of requirement for best possible outcome, it is recommended to open the link document in Revit and pull the elements directly from there.");

                return hostObject.PanelSurfaces_LinkDocument(insertsToIgnore, settings);
            }
        }


        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        private static Dictionary<PlanarSurface, List<PlanarSurface>> PanelSurfaces_HostDocument(this HostObject hostObject, IEnumerable<ElementId> insertsToIgnore = null, RevitSettings settings = null)
        {
            List<Autodesk.Revit.DB.Plane> planes = hostObject.IPanelPlanes();
            if (planes.Count == 0)
                return null;

            Document doc = hostObject.Document;
            Dictionary<PlanarSurface, List<PlanarSurface>> result = new Dictionary<PlanarSurface, List<PlanarSurface>>();

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

        private static Dictionary<PlanarSurface, List<PlanarSurface>> PanelSurfaces_LinkDocument(this HostObject hostObject, IEnumerable<ElementId> insertsToIgnore = null, RevitSettings settings = null)
        {
            List<Autodesk.Revit.DB.Face> faces = hostObject.ILinkPanelFaces(settings);
            List<PlanarFace> planarFaces = faces.Where(x => x is PlanarFace).Cast<PlanarFace>().ToList();
            if (faces.Count != planarFaces.Count)
                BH.Engine.Reflection.Compute.RecordWarning($"Some faces of the link element were not planar and could not be retrieved.\n ElementId: {hostObject.Id} Document: {hostObject.Document.PathName}");

            Dictionary<PlanarSurface, List<PlanarSurface>> result = new Dictionary<PlanarSurface, List<PlanarSurface>>();
            List<Autodesk.Revit.DB.Face> edgeFaces = null;
            if (insertsToIgnore != null && insertsToIgnore.Any())
                edgeFaces = hostObject.Faces(new Options(), settings).Where(x => hostObject.GetGeneratingElementIds(x).Any(y => insertsToIgnore.Any(z => y.IntegerValue == z.IntegerValue))).ToList();

            foreach (PlanarFace pf in planarFaces)
            {
                ICurve outline = null;
                List<ICurve> openings = new List<ICurve>();
                int k = 0;
                foreach (EdgeArray ea in pf.EdgeLoops)
                {
                    k++;
                    PolyCurve loop = new PolyCurve();
                    if (edgeFaces != null)
                    {
                        List<ICurve> edges = new List<ICurve>();
                        foreach (Edge e in ea)
                        {
                            Curve crv = e.AsCurve();
                            if (!crv.IsEdge(edgeFaces, settings))
                                edges.Add(crv.IFromRevit());
                        }
                        
                        for (int i = edges.Count - 1; i > 0; i--)
                        {
                            BH.oM.Geometry.Point pt1 = edges[i - 1].IEndPoint();
                            BH.oM.Geometry.Point pt2 = edges[i].IStartPoint();
                            if (pt1.Distance(pt2) > settings.DistanceTolerance)
                                edges.Insert(i, new BH.oM.Geometry.Line { Start = pt1, End = pt2 });
                        }

                        if (edges.Count != 0)
                        {
                            BH.oM.Geometry.Point end = edges.Last().IEndPoint();
                            BH.oM.Geometry.Point start = edges[0].IStartPoint();
                            if (end.Distance(start) > settings.DistanceTolerance)
                                edges.Add(new BH.oM.Geometry.Line { Start = end, End = start });

                            loop.Curves = edges;
                        }
                    }
                    else
                    {
                        foreach(Edge e in ea)
                        {
                            loop.Curves.Add(e.AsCurve().IFromRevit());
                        }
                    }

                    if (loop.Curves.Count == 0)
                        continue;

                    if (k == 1)
                        outline = loop;
                    else
                        openings.Add(loop);

                }

                if (outline != null)
                    result.Add(new PlanarSurface(outline, new List<ICurve>()), openings.Select(x => new PlanarSurface(x, new List<ICurve>())).ToList());
            }

            return result;
        }

        /***************************************************/
    }
}
