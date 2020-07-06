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
using BH.oM.Base;
using BH.oM.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static oM.Physical.Elements.Wall WallFromRevit(this Wall wall, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            oM.Physical.Elements.Wall bHoMWall = refObjects.GetValue<oM.Physical.Elements.Wall>(wall.Id);
            if (bHoMWall != null)
                return bHoMWall;

            if (wall.StackedWallOwnerId != null && wall.StackedWallOwnerId != ElementId.InvalidElementId)
                return null;

            //TODO: check if wall not curved! and return without geometry

            Document doc = wall.Document;

            if (wall.CurtainGrid != null)
            {
                CurtainGrid cg = wall.CurtainGrid;

                List<Element> panels = cg.GetPanelIds().Select(x => doc.GetElement(x)).ToList();
                List<CurtainCell> cells = cg.GetCurtainCells().ToList();

                //TODO: check if same counts and error if not!
                //TODO: SetIdentifiers and all that stuff, plus check if WindowFromPanel still useful?
                //TODO: why originContextFragment on physical convert? 

                List<BH.oM.Physical.Elements.IOpening> curtainPanels = new List<oM.Physical.Elements.IOpening>();
                for (int i = 0; i < panels.Count; i++)
                {
                    foreach (PolyCurve pc in cells[i].PlanarizedCurveLoops.FromRevit())
                    {
                        if (panels[i].Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors)
                            curtainPanels.Add(new BH.oM.Physical.Elements.Door { Location = new PlanarSurface(pc, null), Name = panels[i].FamilyTypeFullName() });
                        else
                            curtainPanels.Add(new BH.oM.Physical.Elements.Window { Location = new PlanarSurface(pc, null), Name = panels[i].FamilyTypeFullName() });
                    }
                }

                //TODO: name, identifiers etc.

                ISurface location;
                if (curtainPanels.Count == 0)
                {
                    //TODO: return without geometry?
                    location = null;
                }
                else if (curtainPanels.Count == 1)
                    location = curtainPanels[0].Location;
                else
                    location = new PolySurface { Surfaces = curtainPanels.Select(x => x.Location).ToList() };

                //TODO: some construction
                return new oM.Physical.Elements.Wall { Location = location, Openings = curtainPanels };
            }

            HostObjAttributes hostObjAttributes = doc.GetElement(wall.GetTypeId()) as HostObjAttributes;
            oM.Physical.Constructions.Construction construction = hostObjAttributes.ConstructionFromRevit(settings, refObjects);
            string materialGrade = wall.MaterialGrade(settings);
            construction = construction.UpdateMaterialProperties(hostObjAttributes, materialGrade, settings, refObjects);

            Autodesk.Revit.DB.Line l = (wall.Location as LocationCurve)?.Curve as Autodesk.Revit.DB.Line;
            XYZ normal = l.Direction.CrossProduct(XYZ.BasisZ).Normalize();
            Autodesk.Revit.DB.Plane plane = Autodesk.Revit.DB.Plane.CreateByNormalAndOrigin(normal, l.Origin);

            List<Element> inserts = (wall as HostObject).FindInserts(true, true, true, true).Select(x => doc.GetElement(x)).ToList();
            
            List<FamilyInstance> windows = inserts.Where(x => x is FamilyInstance && x.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Windows).Cast<FamilyInstance>().ToList();
            List<FamilyInstance> doors = inserts.Where(x => x is FamilyInstance && x.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors).Cast<FamilyInstance>().ToList();

            Dictionary<PlanarSurface, List<PlanarSurface>> surfs = wall.PanelSurfaces(plane, false, windows.Union(doors).Select(x => x.Id), settings);
            List<ISurface> surfaces = new List<ISurface>(surfs.Keys);
            List<BH.oM.Physical.Elements.IOpening> openings = new List<oM.Physical.Elements.IOpening>();
            foreach(List<PlanarSurface> ps in surfs.Values)
            {
                openings.AddRange(ps.Select(x => new BH.oM.Physical.Elements.Void { Location = x }));
            }
            
            foreach (FamilyInstance window in windows)
            {
                openings.Add(window.WindowFromRevit(settings, refObjects));
            }

            foreach (FamilyInstance door in doors)
            {
                openings.Add(door.DoorFromRevit(settings, refObjects));
            }

            ISurface surface;
            if (surfaces.Count == 1)
                surface = surfaces[0];
            else
                surface = new PolySurface { Surfaces = surfaces };

            bHoMWall = new oM.Physical.Elements.Wall { Location = surface, Openings = openings, Construction = construction };
            bHoMWall.Name = wall.FamilyTypeFullName();

            //Set identifiers, parameters & custom data
            bHoMWall.SetIdentifiers(wall);
            bHoMWall.CopyParameters(wall, settings.ParameterSettings);
            bHoMWall.SetProperties(wall, settings.ParameterSettings);

            refObjects.AddOrReplace(wall.Id, bHoMWall);
            return bHoMWall;
        }

        public static Autodesk.Revit.DB.Plane OpeningPlane(this FamilyInstance familyInstance, RevitSettings settings)
        {
            if (familyInstance == null || !(familyInstance.Location is LocationPoint))
                return null;

            HostObject host = familyInstance.Host as HostObject;
            if (host == null)
                return null;

            XYZ normal = familyInstance.GetTotalTransform().BasisY;
            Autodesk.Revit.DB.Plane plane = null;
            if (host is Wall && Math.Abs(normal.DotProduct(XYZ.BasisZ)) <= settings.AngleTolerance)
            {
               Autodesk.Revit.DB.Line line = (((Wall)host).Location as LocationCurve)?.Curve as Autodesk.Revit.DB.Line;
                if (line == null)
                    return null;

                plane = Autodesk.Revit.DB.Plane.CreateByNormalAndOrigin(normal, line.Origin);
            }
            else
            {
                if (normal.Z < 0)
                    normal = normal.Negate();

                XYZ pt = ((LocationPoint)familyInstance.Location).Point;
                foreach (Autodesk.Revit.DB.Face f in host.Faces(new Options(), settings))
                {
                    PlanarFace pf = f as PlanarFace;
                    if (pf == null)
                        continue;
                    
                    if (1- pf.FaceNormal.DotProduct(normal) <= settings.AngleTolerance && pf.Project(pt) != null)
                    {
                        plane = Autodesk.Revit.DB.Plane.CreateByNormalAndOrigin(normal, pf.Origin);
                        break;
                    }
                }
            }

            return plane;
        }

        public static PlanarSurface OpeningSurface(this FamilyInstance familyInstance, RevitSettings settings)
        {
            if (familyInstance == null || !(familyInstance.Location is LocationPoint))
                return null;

            HostObject host = familyInstance.Host as HostObject;
            if (host == null)
                return null;

            Autodesk.Revit.DB.Plane openingPlane = familyInstance.OpeningPlane(settings);
            if (openingPlane == null)
                return null;

            //TODO: this should be migrated out? here it forces on centerline of the wall and on face of the floor?
            bool planeOnFace = Math.Abs(openingPlane.Normal.DotProduct(XYZ.BasisZ)) > settings.AngleTolerance;

            Document doc = familyInstance.Document;
            Dictionary<PlanarSurface, List<PlanarSurface>> result = new Dictionary<PlanarSurface, List<PlanarSurface>>();

            List<ElementId> inserts = host.FindInserts(true, true, true, true).Where(x => x.IntegerValue != familyInstance.Id.IntegerValue).ToList();

            Transaction t = new Transaction(doc);
            FailureHandlingOptions failureHandlingOptions = t.GetFailureHandlingOptions().SetClearAfterRollback(true);
            t.Start("Temp Delete Inserts");

            foreach (ElementId id in JoinGeometryUtils.GetJoinedElements(doc, host))
            {
                JoinGeometryUtils.UnjoinGeometry(doc, host, doc.GetElement(id));
            }

            doc.Delete(inserts);
            doc.Regenerate();

            List<Solid> solidsWithOpening = host.Solids(new Options());
            if (!planeOnFace)
                solidsWithOpening = solidsWithOpening.Select(x => BooleanOperationsUtils.CutWithHalfSpace(x, openingPlane)).ToList();

            // Rollback and restart of the transaction is needed because otherwise the object, to which familyInstance is pointing can become invalidated.
            t.RollBack(failureHandlingOptions);
            t.Start();

            inserts.Add(familyInstance.Id);
            doc.Delete(inserts);
            doc.Regenerate();

            List<Solid> fullSolids = host.Solids(new Options()).SelectMany(x => SolidUtils.SplitVolumes(x)).ToList();
            if (!planeOnFace)
                fullSolids = fullSolids.Select(x => BooleanOperationsUtils.CutWithHalfSpace(x, openingPlane)).ToList();

            XYZ normal = openingPlane.Normal;
            if (!planeOnFace)
                normal = -normal;

            List<CurveLoop> loops = new List<CurveLoop>();
            foreach (Solid s in fullSolids)
            {
                foreach (Solid s2 in solidsWithOpening)
                {
                    BooleanOperationsUtils.ExecuteBooleanOperationModifyingOriginalSolid(s, s2, BooleanOperationsType.Difference);
                }

                foreach (Autodesk.Revit.DB.Face f in s.Faces)
                {
                    PlanarFace pf = f as PlanarFace;
                    if (pf == null)
                        continue;

                    if (Math.Abs(1 - pf.FaceNormal.DotProduct(normal)) <= settings.DistanceTolerance)
                        loops.AddRange(pf.GetEdgesAsCurveLoops());
                }
            }
            
            t.RollBack(failureHandlingOptions);

            if (loops.Count == 1)
                return new PlanarSurface(loops[0].FromRevit(), null);
            else
                return null;
        }
        
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
            if (!planeOnFace)
                solidsWithOpenings = solidsWithOpenings.Select(x => BooleanOperationsUtils.CutWithHalfSpace(x, plane)).ToList();
            
            //TODO: not needed if no insterts!
            doc.Delete(inserts);
            doc.Regenerate();

            List<Solid> fullSolids = hostObject.Solids(new Options()).SelectMany(x => SolidUtils.SplitVolumes(x)).ToList();
            if (!planeOnFace)
                fullSolids = fullSolids.Select(x => BooleanOperationsUtils.CutWithHalfSpace(x, plane)).ToList();

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

                result.Add(surface, openings);
            }
            
            t.RollBack();
            return result;
        }

        /***************************************************/
    }
}
