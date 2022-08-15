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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Geometry;
using BH.oM.Base.Attributes;
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

        [Description("Converts a Revit Floor to BH.oM.Physical.Elements.Floor.")]
        [Input("floor", "Revit Floor to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("floor", "BH.oM.Physical.Elements.Floor resulting from converting the input Revit Floor.")]
        public static oM.Physical.Elements.Floor FloorFromRevit(this Floor floor, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (floor == null)
                return null;

            settings = settings.DefaultIfNull();

            oM.Physical.Elements.Floor bHoMFloor = refObjects.GetValue<oM.Physical.Elements.Floor>(floor.Id);
            if (bHoMFloor != null)
                return bHoMFloor;

            HostObjAttributes hostObjAttributes = floor.Document.GetElement(floor.GetTypeId()) as HostObjAttributes;
            string materialGrade = floor.MaterialGrade(settings);
            oM.Physical.Constructions.Construction construction = hostObjAttributes.ConstructionFromRevit(materialGrade, settings, refObjects);

            ISurface location = null;
            List<BH.oM.Physical.Elements.IOpening> openings = new List<oM.Physical.Elements.IOpening>();

            Dictionary<PlanarSurface, List<PlanarSurface>> surfaces = null;
            BoundingBoxXYZ bbox = floor.get_BoundingBox(null);
            if (bbox != null)
            {
                IList<ElementId> insertIds = floor.InsertsToIgnore(oM.Adapters.Revit.Enums.Discipline.Physical);

                SurfaceCache cache = refObjects.GetValue<SurfaceCache>(floor.Id.SurfaceCacheKey());
                if (cache != null)
                    surfaces = cache.Surfaces;
                else
                    surfaces = floor.PanelSurfaces(insertIds, settings);

                if (surfaces != null)
                {
                    List<FamilyInstance> inserts = insertIds.Select(x => floor.Document.GetElement(x)).Cast<FamilyInstance>().ToList();

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
                        BH.oM.Physical.Elements.Window bHoMWindow = window.WindowFromRevit(floor, settings, refObjects);
                        if (bHoMWindow != null)
                            openings.Add(bHoMWindow);
                    }

                    foreach (FamilyInstance door in inserts.Where(x => x.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors))
                    {
                        BH.oM.Physical.Elements.Door bHoMDoor = door.DoorFromRevit(floor, settings, refObjects);
                        if (bHoMDoor != null)
                            openings.Add(bHoMDoor);
                    }
                }
            }

            if (surfaces == null || surfaces.Count == 0)
                floor.NoPanelLocationError();

            bHoMFloor = new oM.Physical.Elements.Floor { Location = location, Openings = openings, Construction = construction };
            bHoMFloor.Name = floor.FamilyTypeFullName();

            //Set identifiers, parameters & custom data
            bHoMFloor.SetIdentifiers(floor);
            bHoMFloor.CopyParameters(floor, settings.MappingSettings);
            bHoMFloor.SetProperties(floor, settings.MappingSettings);

            refObjects.AddOrReplace(floor.Id, bHoMFloor);
            return bHoMFloor;
        }

        /***************************************************/
    }
}


