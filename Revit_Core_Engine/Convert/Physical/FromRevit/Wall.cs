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

            if (wall.CurtainGrid != null)
                bHoMWall = wall.CurtainWallFromRevit(settings, refObjects);
            else
                bHoMWall = wall.SolidWallFromRevit(settings, refObjects);

            if (bHoMWall == null)
                return null;
            
            HostObjAttributes hostObjAttributes = wall.Document.GetElement(wall.GetTypeId()) as HostObjAttributes;
            oM.Physical.Constructions.Construction construction = hostObjAttributes.ConstructionFromRevit(settings, refObjects);
            string materialGrade = wall.MaterialGrade(settings);
            construction = construction.UpdateMaterialProperties(hostObjAttributes, materialGrade, settings, refObjects);
            bHoMWall.Construction = construction;

            bHoMWall.Name = wall.FamilyTypeFullName();

            //Set identifiers, parameters & custom data
            bHoMWall.SetIdentifiers(wall);
            bHoMWall.CopyParameters(wall, settings.ParameterSettings);
            bHoMWall.SetProperties(wall, settings.ParameterSettings);

            refObjects.AddOrReplace(wall.Id, bHoMWall);
            return bHoMWall;
        }


        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        private static oM.Physical.Elements.Wall SolidWallFromRevit(this Wall wall, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            ISurface location = null;
            List<BH.oM.Physical.Elements.IOpening> openings = new List<oM.Physical.Elements.IOpening>();

            Autodesk.Revit.DB.Line wallLine = (wall.Location as LocationCurve)?.Curve as Autodesk.Revit.DB.Line;
            if (wallLine != null)
            {
                XYZ normal = wallLine.Direction.CrossProduct(XYZ.BasisZ).Normalize();
                Autodesk.Revit.DB.Plane plane = Autodesk.Revit.DB.Plane.CreateByNormalAndOrigin(normal, wallLine.Origin);

                List<Element> inserts = (wall as HostObject).FindInserts(true, true, true, true).Select(x => wall.Document.GetElement(x)).ToList();
                List<FamilyInstance> windows = inserts.Where(x => x is FamilyInstance && x.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Windows).Cast<FamilyInstance>().ToList();
                List<FamilyInstance> doors = inserts.Where(x => x is FamilyInstance && x.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors).Cast<FamilyInstance>().ToList();

                Dictionary<PlanarSurface, List<PlanarSurface>> surfaces = wall.PanelSurfaces(plane, false, windows.Union(doors).Select(x => x.Id), settings);
                List<ISurface> locations = new List<ISurface>(surfaces.Keys);
                if (locations.Count != 0)
                {
                    foreach (List<PlanarSurface> ps in surfaces.Values)
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

                    if (locations.Count == 1)
                        location = locations[0];
                    else
                        location = new PolySurface { Surfaces = locations };
                }
            }
            else
                BH.Engine.Reflection.Compute.RecordError(String.Format("Conversion of curved walls to BHoM is currently not supported. A BHoM wall without location has been returned. Revit ElementId: {0}", wall.Id.IntegerValue));

            return new oM.Physical.Elements.Wall { Location = location, Openings = openings };
        }

        /***************************************************/

        private static oM.Physical.Elements.Wall CurtainWallFromRevit(this Wall wall, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            CurtainGrid cg = wall.CurtainGrid;

            List<BH.oM.Physical.Elements.IOpening> curtainPanels = new List<oM.Physical.Elements.IOpening>();

            List<Element> panels = cg.GetPanelIds().Select(x => wall.Document.GetElement(x)).ToList();
            List<CurtainCell> cells = cg.GetCurtainCells().ToList();
            if (panels.Count == cells.Count)
            {
                for (int i = 0; i < panels.Count; i++)
                {
                    if (panels[i].get_BoundingBox(null) == null)
                        continue;

                    foreach (PolyCurve pc in cells[i].CurveLoops.FromRevit())
                    {
                        if (panels[i].Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors)
                            curtainPanels.Add(new BH.oM.Physical.Elements.Door { Location = new PlanarSurface(pc, null), Name = panels[i].FamilyTypeFullName() });
                        else
                            curtainPanels.Add(new BH.oM.Physical.Elements.Window { Location = new PlanarSurface(pc, null), Name = panels[i].FamilyTypeFullName() });
                    }
                }
            }
            else
                BH.Engine.Reflection.Compute.RecordError(String.Format("Processing of panels of Revit curtain wall failed. BHoM wall without location has been returned. Revit ElementId: {0}", wall.Id.IntegerValue));
            
            ISurface location;
            if (curtainPanels.Count == 0)
                location = null;
            else if (curtainPanels.Count == 1)
                location = curtainPanels[0].Location;
            else
                location = new PolySurface { Surfaces = curtainPanels.Select(x => x.Location).ToList() };
            
            return new oM.Physical.Elements.Wall { Location = location, Openings = curtainPanels };
        }

        /***************************************************/
    }
}
