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
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Parameters;
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

        public static ISurface OpeningSurface(this FamilyInstance familyInstance, HostObject host = null, RevitSettings settings = null)
        {
            if (familyInstance == null)
                return null;

            BoundingBoxXYZ bbox = familyInstance.get_BoundingBox(null);
            if (bbox == null)
                return null;

            // Get parent element if this family instance is a nested instance
            Element parentElem = familyInstance.SuperComponent;
            int parentId = -1;
            if (parentElem != null)
                parentId = familyInstance.SuperComponent.Id.IntegerValue;

            settings = settings.DefaultIfNull();
            List<HostObject> hosts;

            if (host != null)
                hosts = new List<HostObject> { host };
            else
            {
                BoundingBoxIntersectsFilter bbif = new BoundingBoxIntersectsFilter(new Outline(bbox.Min, bbox.Max));
                hosts = new FilteredElementCollector(familyInstance.Document).OfClass(typeof(HostObject)).WherePasses(bbif).Cast<HostObject>().ToList();
                hosts = hosts.Where(x => x.FindInserts(true, true, true, true).Any(y => (y.IntegerValue == familyInstance.Id.IntegerValue) || (y.IntegerValue == parentId))).ToList();
            }

            List<ISurface> surfaces = new List<ISurface>();

            if (parentElem != null) // Indicates element is a nested instance and needs different approach to extract location
            {
                Document doc = familyInstance.Document;
                Transaction t = new Transaction(doc);

                t.Start("Temp Delete Inserts");
                List<ElementId> inserts = hosts.SelectMany(x => x.FindInserts(true, true, true, true)).Distinct().Where(x => x.IntegerValue != -1).ToList();

                // Create dummy instance of nested element with matching parameters
                LocationPoint locPt = familyInstance.Location as LocationPoint;
                Element dummyInstance = doc.Create.NewFamilyInstance(locPt.Point, familyInstance.Symbol, hosts.First() as Element, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                IEnumerable <BuiltInParameter> paramsToSkip= new List<BuiltInParameter> { BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM, BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM };
                familyInstance.CopyParameters(dummyInstance, paramsToSkip);

                doc.Delete(inserts);
                doc.Regenerate();

                surfaces = GetOpeningGeometry(t, doc, hosts, inserts, familyInstance, settings);
            }

            else if (hosts.Count == 0)
            {
                HostObject curtainHost = familyInstance.Host as HostObject;
                if (curtainHost == null)
                    return null;

                List<CurtainGrid> curtainGrids = curtainHost.ICurtainGrids();
                if (curtainGrids.Count != 0)
                {
                    foreach (CurtainGrid cg in curtainGrids)
                    {
                        List<ElementId> ids = cg.GetPanelIds().ToList();
                        List<CurtainCell> cells = cg.GetCurtainCells().ToList();
                        if (ids.Count != cells.Count)
                            return null;

                        for (int i = 0; i < ids.Count; i++)
                        {
                            if (ids[i].IntegerValue == familyInstance.Id.IntegerValue)
                            {
                                foreach (PolyCurve curve in cells[i].CurveLoops.FromRevit())
                                {
                                    PlanarSurface surface = new PlanarSurface(curve, null);
                                    if (surface == null)
                                        return null;

                                    surfaces.Add(surface);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Document doc = familyInstance.Document;
                Transaction t = new Transaction(doc);

                t.Start("Temp Delete Inserts");

                List<ElementId> inserts = hosts.SelectMany(x => x.FindInserts(true, true, true, true)).Distinct().Where(x => x.IntegerValue != familyInstance.Id.IntegerValue).ToList();
                doc.Delete(inserts);
                doc.Regenerate();

                inserts.Add(familyInstance.Id);
                surfaces = GetOpeningGeometry(t, doc, hosts, inserts, familyInstance, settings);
            }

            if (surfaces.Count == 0)
                return null;
            else if (surfaces.Count == 1)
                return surfaces[0];
            else
                return new PolySurface { Surfaces = surfaces };
        }


        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        private static List<ISurface> GetOpeningGeometry(Transaction t, Document doc, List<HostObject> hosts, List<ElementId> inserts, FamilyInstance familyInstance, RevitSettings settings = null)
        {
            List<List<Solid>> solidsWithOpening = new List<List<Solid>>();
            foreach (HostObject h in hosts)
            {
                solidsWithOpening.Add(h.Solids(new Options()).Select(x => SolidUtils.Clone(x)).ToList());
            }

            // Rollback and restart of the transaction is needed because otherwise the object, to which familyInstance is pointing can become invalidated.
            FailureHandlingOptions failureHandlingOptions = t.GetFailureHandlingOptions().SetClearAfterRollback(true);
            t.RollBack(failureHandlingOptions);
            t.Start();

            List<ISurface> surfaces= new List<ISurface> ();
            List<CurveLoop> loops = new List<CurveLoop>();
            try
            {
                doc.Delete(inserts);
                doc.Regenerate();

                for (int i = 0; i < hosts.Count; i++)
                {
                    HostObject h = hosts[i];

                    List<Autodesk.Revit.DB.Plane> planes = h.IPanelPlanes();
                    if (planes.Count == 0)
                        continue;

                    List<Solid> fullSolids = h.Solids(new Options()).SelectMany(x => SolidUtils.SplitVolumes(x)).ToList();
                    if (h is Wall)
                    {
                        fullSolids = fullSolids.Select(x => BooleanOperationsUtils.CutWithHalfSpace(x, planes[0])).ToList();
                        planes[0] = Autodesk.Revit.DB.Plane.CreateByNormalAndOrigin(-planes[0].Normal, planes[0].Origin);
                    }

                    foreach (Solid s in fullSolids)
                    {
                        foreach (Solid s2 in solidsWithOpening[i])
                        {
                            BooleanOperationsUtils.ExecuteBooleanOperationModifyingOriginalSolid(s, s2, BooleanOperationsType.Difference);
                        }

                        foreach (Autodesk.Revit.DB.Face f in s.Faces)
                        {
                            PlanarFace pf = f as PlanarFace;
                            if (pf == null)
                                continue;

                            if (planes.Any(x => Math.Abs(1 - pf.FaceNormal.DotProduct(x.Normal)) <= settings.DistanceTolerance && Math.Abs((pf.Origin - x.Origin).DotProduct(x.Normal)) <= settings.AngleTolerance))
                                loops.AddRange(pf.GetEdgesAsCurveLoops());
                        }
                    }
                }
            }
            catch
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("Geometrical processing of a Revit element failed due to an internal Revit error. Converted opening might be missing one or more of its surfaces. Revit ElementId: {0}", familyInstance.Id));
            }

            t.RollBack(failureHandlingOptions);
            surfaces.AddRange(loops.Select(x => new PlanarSurface(x.FromRevit(), null)));
            return surfaces;
        }

        /***************************************************/
    }
}