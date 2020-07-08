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
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static oM.Physical.Elements.Floor FloorFromRevit(this Floor floor, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            oM.Physical.Elements.Floor bHoMFloor = refObjects.GetValue<oM.Physical.Elements.Floor>(floor.Id);
            if (bHoMFloor != null)
                return bHoMFloor;

            HostObjAttributes hostObjAttributes = floor.Document.GetElement(floor.GetTypeId()) as HostObjAttributes;
            oM.Physical.Constructions.Construction construction = hostObjAttributes.ConstructionFromRevit(settings, refObjects);
            string materialGrade = floor.MaterialGrade(settings);
            construction = construction.UpdateMaterialProperties(hostObjAttributes, materialGrade, settings, refObjects);

            ISurface location = null;
            List<BH.oM.Physical.Elements.IOpening> openings = new List<oM.Physical.Elements.IOpening>();

            List<Element> inserts = (floor as HostObject).FindInserts(true, true, true, true).Select(x => floor.Document.GetElement(x)).ToList();
            List<FamilyInstance> windows = inserts.Where(x => x is FamilyInstance && x.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Windows).Cast<FamilyInstance>().ToList();
            List<FamilyInstance> doors = inserts.Where(x => x is FamilyInstance && x.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors).Cast<FamilyInstance>().ToList();

            Dictionary<PlanarSurface, List<PlanarSurface>> surfaces = floor.PanelSurfaces(windows.Union(doors).Select(x => x.Id), settings);
            if (surfaces != null && surfaces.Count != 0)
            {
                List<ISurface> locations = new List<ISurface>(surfaces.Keys);
                if (locations.Count == 1)
                    location = locations[0];
                else
                    location = new PolySurface { Surfaces = locations };

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
            }
            else
                floor.NoPanelLocationError();

            bHoMFloor = new oM.Physical.Elements.Floor { Location = location, Openings = openings, Construction = construction };
            bHoMFloor.Name = floor.FamilyTypeFullName();

            //Set identifiers, parameters & custom data
            bHoMFloor.SetIdentifiers(floor);
            bHoMFloor.CopyParameters(floor, settings.ParameterSettings);
            bHoMFloor.SetProperties(floor, settings.ParameterSettings);

            refObjects.AddOrReplace(floor.Id, bHoMFloor);
            return bHoMFloor;
        }

        /***************************************************/
    }
}
