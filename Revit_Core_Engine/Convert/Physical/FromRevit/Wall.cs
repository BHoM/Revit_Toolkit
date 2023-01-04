/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BH.Revit.Engine.Core.Objects;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit Wall to BH.oM.Physical.Elements.Wall.")]
        [Input("wall", "Revit Wall to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("wall", "BH.oM.Physical.Elements.Wall resulting from converting the input Revit Wall.")]
        public static oM.Physical.Elements.Wall WallFromRevit(this Wall wall, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (wall == null)
                return null;

            settings = settings.DefaultIfNull();

            oM.Physical.Elements.Wall bHoMWall = refObjects.GetValue<oM.Physical.Elements.Wall>(wall.Id);
            if (bHoMWall != null)
                return bHoMWall;

            if (wall.StackedWallOwnerId != null && wall.StackedWallOwnerId != ElementId.InvalidElementId)
                return null;

            if (wall.CurtainGrid != null)
                bHoMWall = wall.WallFromRevit_Curtain(settings, refObjects);
            else
                bHoMWall = wall.WallFromRevit_Solid(settings, refObjects);

            if (bHoMWall == null)
                return null;
            
            HostObjAttributes hostObjAttributes = wall.Document.GetElement(wall.GetTypeId()) as HostObjAttributes;
            string materialGrade = wall.MaterialGrade(settings);
            oM.Physical.Constructions.Construction construction = hostObjAttributes.ConstructionFromRevit(materialGrade, settings, refObjects);
            bHoMWall.Construction = construction;

            bHoMWall.Name = wall.FamilyTypeFullName();

            //Set identifiers, parameters & custom data
            bHoMWall.SetIdentifiers(wall);
            bHoMWall.CopyParameters(wall, settings.MappingSettings);
            bHoMWall.SetProperties(wall, settings.MappingSettings);

            refObjects.AddOrReplace(wall.Id, bHoMWall);
            return bHoMWall;
        }


        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        [Description("Converts a Revit Wall to BH.oM.Physical.Elements.Wall in case the Revit Roof is a solid one.")]
        [Input("wall", "Revit Wall to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("wall", "BH.oM.Physical.Elements.Wall resulting from converting the input Revit Wall.")]
        private static oM.Physical.Elements.Wall WallFromRevit_Solid(this Wall wall, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            ISurface location = null;
            List<BH.oM.Physical.Elements.IOpening> openings = new List<oM.Physical.Elements.IOpening>();

            Dictionary<PlanarSurface, List<PlanarSurface>> surfaces = null;
            BoundingBoxXYZ bbox = wall.get_BoundingBox(null);
            if (bbox != null)
            {
                IList<ElementId> insertIds = wall.InsertsToIgnore(oM.Adapters.Revit.Enums.Discipline.Physical);

                SurfaceCache cache = refObjects.GetValue<SurfaceCache>(wall.Id.SurfaceCacheKey());
                if (cache != null)
                    surfaces = cache.Surfaces;
                else
                    surfaces = wall.PanelSurfaces(insertIds, settings);

                if (surfaces != null)
                {
                    List<FamilyInstance> inserts = insertIds.Select(x => wall.Document.GetElement(x)).Cast<FamilyInstance>().ToList();

                    List<ISurface> locations = new List<ISurface>(surfaces.Keys);
                    if (locations.Count == 1)
                        location = locations[0];
                    else
                        location = new PolySurface { Surfaces = locations };

                    foreach (List<PlanarSurface> ps in surfaces.Values)
                    {
                        openings.AddRange(ps.Select(x => new BH.oM.Physical.Elements.Void { Location = x }));
                    }

                    foreach (FamilyInstance window in inserts.Where(x => x.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Windows))
                    {
                        BH.oM.Physical.Elements.Window bHoMWindow = window.WindowFromRevit(wall, settings, refObjects);
                        if (bHoMWindow != null)
                            openings.Add(bHoMWindow);
                    }

                    foreach (FamilyInstance door in inserts.Where(x => x.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors))
                    {
                        BH.oM.Physical.Elements.Door bHoMDoor = door.DoorFromRevit(wall, settings, refObjects);
                        if (bHoMDoor != null)
                            openings.Add(bHoMDoor);
                    }
                }
            }

            if (surfaces == null || surfaces.Count == 0)
                wall.NoPanelLocationError();

            return new oM.Physical.Elements.Wall { Location = location, Openings = openings };
        }

        /***************************************************/

        [Description("Converts a Revit Wall to BH.oM.Physical.Elements.Wall in case the Revit Roof is a curtain one.")]
        [Input("wall", "Revit Wall to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("wall", "BH.oM.Physical.Elements.Wall resulting from converting the input Revit Wall.")]
        private static oM.Physical.Elements.Wall WallFromRevit_Curtain(this Wall wall, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            List<BH.oM.Physical.Elements.IOpening> curtainPanels = wall.CurtainGrid.CurtainPanels(wall.Document, settings, refObjects);
            
            ISurface location = null;
            if (curtainPanels == null || curtainPanels.Count == 0)
                BH.Engine.Base.Compute.RecordError(String.Format("Processing of panels of Revit curtain wall failed. BHoM wall without location has been returned. Revit ElementId: {0}", wall.Id.IntegerValue));
            else if (curtainPanels.Count == 1)
                location = curtainPanels[0].Location;
            else
                location = new PolySurface { Surfaces = curtainPanels.Select(x => x.Location).ToList() };
            
            return new oM.Physical.Elements.Wall { Location = location, Openings = curtainPanels };
        }

        /***************************************************/
    }
}



