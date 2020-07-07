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

        public static Dictionary<PlanarSurface, List<PlanarSurface>> PanelSurfaces(this HostObject hostObject, Autodesk.Revit.DB.Plane plane, bool planeOnFace, IEnumerable<ElementId> insertsToIgnore = null, RevitSettings settings = null)
        {
            Document doc = hostObject.Document;

            settings = settings.DefaultIfNull();


            Dictionary<PlanarSurface, List<PlanarSurface>> result = new Dictionary<PlanarSurface, List<PlanarSurface>>();

            IList<ElementId> inserts = hostObject.FindInserts(true, true, true, true);
            if (insertsToIgnore != null)
                inserts = inserts.Where(x => insertsToIgnore.All(y => x.IntegerValue != y.IntegerValue)).ToList();

            Transaction t = new Transaction(doc);
            t.Start("Temp Delete Inserts And Unjoin Geometry");

            foreach (ElementId id in JoinGeometryUtils.GetJoinedElements(doc, hostObject))
            {
                JoinGeometryUtils.UnjoinGeometry(doc, hostObject, doc.GetElement(id));
            }

            if (hostObject is Wall)
            {
                WallUtils.DisallowWallJoinAtEnd((Wall)hostObject, 0);
                WallUtils.DisallowWallJoinAtEnd((Wall)hostObject, 1);
            }

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
            if (!planeOnFace)
            {
                fullSolids.ForEach(x => BooleanOperationsUtils.CutWithHalfSpaceModifyingOriginalSolid(x, plane));
                Autodesk.Revit.DB.Plane flippedPlane = Autodesk.Revit.DB.Plane.CreateByNormalAndOrigin(-plane.Normal, plane.Origin + plane.Normal * 1e-3);
                fullSolids.ForEach(x => BooleanOperationsUtils.CutWithHalfSpaceModifyingOriginalSolid(x, flippedPlane));
                fullSolids = fullSolids.SelectMany(x => SolidUtils.SplitVolumes(x)).ToList();
            }

            XYZ normal = plane.Normal;
            if (!planeOnFace)
                normal = -normal;

            foreach (Solid s in fullSolids)
            {
                PlanarSurface surface = null;
                List<PlanarSurface> openings = new List<PlanarSurface>();

                foreach (Autodesk.Revit.DB.Face f in s.Faces)
                {
                    PlanarFace pf = f as PlanarFace;
                    if (pf == null)
                        continue;

                    if (Math.Abs(1 - pf.FaceNormal.DotProduct(normal)) <= settings.AngleTolerance && Math.Abs((pf.Origin - plane.Origin).DotProduct(normal)) <= settings.AngleTolerance)
                    {
                        foreach (IList<CurveLoop> loops in ExporterIFCUtils.SortCurveLoops(pf.GetEdgesAsCurveLoops()))
                        {
                            surface = new PlanarSurface(loops[0].FromRevit(), null);
                            foreach (CurveLoop loop in loops.Skip(1))
                            {
                                openings.Add(new PlanarSurface(loop.FromRevit(), null));
                            }
                        }
                    }
                }

                if (inserts.Count != 0)
                {
                    foreach (Solid s2 in solidsWithOpenings)
                    {
                        BooleanOperationsUtils.ExecuteBooleanOperationModifyingOriginalSolid(s, s2, BooleanOperationsType.Difference);
                    }

                    foreach (Autodesk.Revit.DB.Face f in BooleanOperationsUtils.CutWithHalfSpace(s, plane).Faces)
                    {
                        PlanarFace pf = f as PlanarFace;
                        if (pf == null)
                            continue;

                        if (Math.Abs(1 - pf.FaceNormal.DotProduct(normal)) <= settings.AngleTolerance && Math.Abs((pf.Origin - plane.Origin).DotProduct(normal)) <= settings.AngleTolerance)
                        {
                            foreach (CurveLoop loop in pf.GetEdgesAsCurveLoops())
                            {
                                openings.Add(new PlanarSurface(loop.FromRevit(), null));
                            }
                        }
                    }
                }

                result.Add(surface, openings);
            }

            t.RollBack();
            return result;
        }

        /***************************************************/
    }
}