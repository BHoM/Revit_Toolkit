/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BH.oM.Adapters.Revit.Enums;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Extracts the BHoM-representative location surfaces from a given collection of Revit host objects." +
                     "\nOptimised to regenerate the document only twice for all input elements, not twice per each.")]
        [Input("hostObjects", "Revit host objects to extract the location surfaces from.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("surfaces", "BHoM-representative location surfaces extracted from the input Revit host objects.")]
        public static Dictionary<ElementId, Dictionary<PlanarSurface, List<PlanarSurface>>> PanelSurfaces(this List<HostObject> hostObjects, Discipline discipline, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            var result = new Dictionary<ElementId, Dictionary<PlanarSurface, List<PlanarSurface>>>();

            List<HostObject> linked = hostObjects.Where(x => x.Document.IsLinked).ToList();
            List<HostObject> curtains = hostObjects.Where(x => !x.Document.IsLinked && x.ICurtainGrids().Count != 0).ToList();
            List<HostObject> solids = hostObjects.Except(linked.Union(curtains)).ToList();

            // Process linked elements first
            foreach (HostObject hostObject in linked)
            {
                if (hostObject.ICurtainGrids().Count != 0)
                    result.Add(hostObject.Id, null);
                else
                {
                    IList<ElementId> insertsToIgnore = hostObject.InsertsToIgnore(discipline);
                    result.Add(hostObject.Id, hostObject.PanelSurfaces_LinkDocument(insertsToIgnore, settings));
                }
            }

            if (result.Count(x => x.Value != null) != 0)
                BH.Engine.Base.Compute.RecordWarning("Pulling panels and openings from Revit link documents is simplified compared to pulling directly from the host document, therefore it may result in degraded output.\n" +
                                                     "In case of requirement for best possible outcome, it is recommended to open the link document in Revit and pull the elements directly from there.");

            // Return null for curtain panels because their location is extracted differently
            foreach (HostObject hostObject in curtains)
            {
                result.Add(hostObject.Id, null);
            }

            // Extract location surfaces for all panels at once
            foreach (var group in solids.GroupBy(x => x.Document)) 
            {
                var surfs = group.Key.PanelSurfaces_HostDocument(group.Select(x => (x.Id, x.InsertsToIgnore(discipline) as IEnumerable<ElementId>)).ToList(), settings);
                foreach (var kvp in surfs)
                {
                    result.Add(kvp.Key, kvp.Value);
                }
            }

            return result;
        }

        /***************************************************/

        [Description("Extracts the BHoM-representative location surfaces from a given Revit host object.")]
        [Input("hostObject", "Revit host object to extract the location surfaces from.")]
        [Input("insertsToIgnore", "Revit inserts (doors, windows etc.) to ignore when extracting the location surfaces.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("surfaces", "BHoM-representative location surfaces extracted from the input Revit host object.")]
        public static Dictionary<PlanarSurface, List<PlanarSurface>> PanelSurfaces(this HostObject hostObject, IEnumerable<ElementId> insertsToIgnore = null, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            if (!hostObject.Document.IsLinked)
                return hostObject.Document.PanelSurfaces_HostDocument(new List<(ElementId, IEnumerable<ElementId>)> { (hostObject.Id, insertsToIgnore) }, settings)[hostObject.Id];
            else
            {
                BH.Engine.Base.Compute.RecordWarning("Pulling panels and openings from Revit link documents is simplified compared to pulling directly from the host document, therefore it may result in degraded output.\n" +
                                                     "In case of requirement for best possible outcome, it is recommended to open the link document in Revit and pull the elements directly from there.");

                return hostObject.PanelSurfaces_LinkDocument(insertsToIgnore, settings);
            }
        }


        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        private static Dictionary<ElementId, Dictionary<PlanarSurface, List<PlanarSurface>>> PanelSurfaces_HostDocument(this Document doc, List<(ElementId, IEnumerable<ElementId>)> hostsWithInsertsToIgnore, RevitSettings settings = null)
        {
            var result = new Dictionary<ElementId, Dictionary<PlanarSurface, List<PlanarSurface>>>();

            // query panel surfaces from each of the panels and rule out the ones that do not have any
            var planes = new Dictionary<ElementId, List<Autodesk.Revit.DB.Plane>>();
            var toProcess = new List<(ElementId, IEnumerable<ElementId>)>();
            foreach ((ElementId, IEnumerable<ElementId>) tuple in hostsWithInsertsToIgnore)
            {
                ElementId id = tuple.Item1;
                List<Autodesk.Revit.DB.Plane> hostPlanes = (doc.GetElement(id) as HostObject)?.IPanelPlanes(settings);
                if (hostPlanes == null || hostPlanes.Count == 0)
                    result.Add(id, null);
                else
                {
                    toProcess.Add(tuple);
                    planes.Add(id, hostPlanes);
                }
            }

            using (Transaction t = new Transaction(doc, "Temp Delete Inserts And Unjoin Geometry"))
            {
                FailureHandlingOptions failureHandlingOptions = t.GetFailureHandlingOptions().SetClearAfterRollback(true);
                t.Start();

                // Do the preprocessing by unjoining the elements from each other and deleting the openings meant to be ignored
                HashSet<ElementId> insertsToDelete = new HashSet<ElementId>();
                foreach ((ElementId, IEnumerable<ElementId>) tuple in toProcess)
                {
                    HostObject hostObject = doc.GetElement(tuple.Item1) as HostObject;
                    IEnumerable<ElementId> insertsToIgnore = tuple.Item2;

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
                    {
                        foreach (ElementId id in insertsToIgnore)
                        {
                            insertsToDelete.Add(id);
                        }
                    }
                }

                if (insertsToDelete.Count != 0)
                    doc.Delete(insertsToDelete);
                
                // 1st regenerate
                doc.Regenerate();

                // Get the solid representations of each of the panels with openings
                Dictionary<ElementId, List<Solid>> withOpenings = new Dictionary<ElementId, List<Solid>>();
                foreach ((ElementId, IEnumerable<ElementId>) tuple in toProcess)
                {
                    HostObject hostObject = doc.GetElement(tuple.Item1) as HostObject;
                    withOpenings.Add(hostObject.Id, hostObject.Solids(new Options()).Select(x => SolidUtils.Clone(x)).ToList());
                }

                // Delete remaining openings
                insertsToDelete = new HashSet<ElementId>();
                Dictionary<ElementId, bool> insertsDeleted = new Dictionary<ElementId, bool>();
                foreach ((ElementId, IEnumerable<ElementId>) tuple in toProcess)
                {
                    ElementId id = tuple.Item1;
                    if (withOpenings[id] == null)
                    {
                        insertsDeleted.Add(id, false);
                        continue;
                    }

                    HostObject hostObject = doc.GetElement(id) as HostObject;
                    IEnumerable<ElementId> insertsToIgnore = tuple.Item2;

                    IList<ElementId> inserts = hostObject.FindInserts(true, true, true, true);
                    if (insertsToIgnore != null)
                        inserts = inserts.Where(x => insertsToIgnore.All(y => x.IntegerValue != y.IntegerValue)).ToList();

                    insertsDeleted.Add(id, inserts.Count != 0);
                    foreach (ElementId insert in inserts)
                    {
                        insertsToDelete.Add(insert);
                    }
                }

                if (insertsToDelete.Count != 0)
                {
                    doc.Delete(insertsToDelete);
                    doc.Regenerate();
                }

                // Get the solid representations of each of the panels without openings
                Dictionary<ElementId, List<Solid>> withoutOpenings = new Dictionary<ElementId, List<Solid>>();
                foreach ((ElementId, IEnumerable<ElementId>) tuple in toProcess)
                {
                    ElementId id = tuple.Item1;
                    List<Solid> solidsWithOpenings = withOpenings[id];
                    if (solidsWithOpenings == null)
                    {
                        withoutOpenings.Add(id, null);
                        continue;
                    }

                    if (insertsDeleted[id])
                    {
                        HostObject hostObject = doc.GetElement(id) as HostObject;
                        withoutOpenings.Add(id, hostObject.Solids(new Options()).Select(x => SolidUtils.Clone(x)).ToList());
                    }
                    else
                        withoutOpenings.Add(id, solidsWithOpenings);
                }

                // Extract the location of the panel and its not ignored openings
                foreach ((ElementId, IEnumerable<ElementId>) tuple in toProcess)
                {
                    ElementId id = tuple.Item1;
                    HostObject hostObject = doc.GetElement(id) as HostObject;

                    List<Solid> solidsWithOpenings = withOpenings[id];
                    List<Solid> fullSolids = withoutOpenings[id];
                    if (solidsWithOpenings == null || fullSolids == null)
                    {
                        result.Add(id, null);
                        continue;
                    }

                    List<Autodesk.Revit.DB.Plane> objectPlanes = planes[id];

                    try
                    {
                        Dictionary<PlanarSurface, List<PlanarSurface>> subResult = new Dictionary<PlanarSurface, List<PlanarSurface>>();

                        fullSolids = fullSolids.SelectMany(x => SolidUtils.SplitVolumes(x)).Where(x => x != null).ToList();
                        if (hostObject is Wall)
                        {
                            fullSolids.ForEach(x => BooleanOperationsUtils.CutWithHalfSpaceModifyingOriginalSolid(x, objectPlanes[0]));
                            fullSolids = fullSolids.Where(x => x != null).ToList();
                            Autodesk.Revit.DB.Plane flippedPlane = Autodesk.Revit.DB.Plane.CreateByNormalAndOrigin(-objectPlanes[0].Normal, objectPlanes[0].Origin + objectPlanes[0].Normal * 1e-3);
                            fullSolids.ForEach(x => BooleanOperationsUtils.CutWithHalfSpaceModifyingOriginalSolid(x, flippedPlane));
                            fullSolids = fullSolids.Where(x => x != null).SelectMany(x => SolidUtils.SplitVolumes(x)).ToList();
                            objectPlanes[0] = Autodesk.Revit.DB.Plane.CreateByNormalAndOrigin(-objectPlanes[0].Normal, objectPlanes[0].Origin);
                        }

                        foreach (Autodesk.Revit.DB.Plane plane in objectPlanes)
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

                                if (insertsDeleted[id])
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

                                subResult.Add(surface, openings);
                            }
                        }

                        result.Add(id, subResult);
                    }
                    catch
                    {
                        BH.Engine.Base.Compute.RecordError(String.Format("Geometrical processing of a Revit element failed due to an internal Revit error. Converted panel might be missing one or more of its surfaces. Revit ElementId: {0}", hostObject.Id));
                        result.Add(id, null);
                    }
                }

                t.RollBack(failureHandlingOptions);
            }

            return result;
        }

        /***************************************************/

        private static Dictionary<PlanarSurface, List<PlanarSurface>> PanelSurfaces_LinkDocument(this HostObject hostObject, IEnumerable<ElementId> insertsToIgnore = null, RevitSettings settings = null)
        {
            List<Autodesk.Revit.DB.Face> faces = hostObject.ILinkPanelFaces(settings);
            if (faces == null)
                return null;

            List<PlanarFace> planarFaces = faces.Where(x => x is PlanarFace).Cast<PlanarFace>().ToList();
            if (faces.Count != planarFaces.Count)
                BH.Engine.Base.Compute.RecordWarning($"Some faces of the link element were not planar and could not be retrieved.\n ElementId: {hostObject.Id} Document: {hostObject.Document.PathName}");

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
                            Curve crv = e.AsCurveFollowingFace(pf);
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
                            loop.Curves.Add(e.AsCurveFollowingFace(pf).IFromRevit());
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

