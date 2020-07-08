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

        public static oM.Physical.Elements.Roof RoofFromRevit(this RoofBase roof, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            oM.Physical.Elements.Roof bHoMRoof = refObjects.GetValue<oM.Physical.Elements.Roof>(roof.Id);
            if (bHoMRoof != null)
                return bHoMRoof;

            HostObjAttributes hostObjAttributes = roof.Document.GetElement(roof.GetTypeId()) as HostObjAttributes;
            oM.Physical.Constructions.Construction construction = hostObjAttributes.ConstructionFromRevit(settings, refObjects);
            string materialGrade = roof.MaterialGrade(settings);
            construction = construction.UpdateMaterialProperties(hostObjAttributes, materialGrade, settings, refObjects);

            ISurface location = null;
            List<BH.oM.Physical.Elements.IOpening> openings = new List<oM.Physical.Elements.IOpening>();

            List<Element> inserts = (roof as HostObject).FindInserts(true, true, true, true).Select(x => roof.Document.GetElement(x)).ToList();
            List<FamilyInstance> windows = inserts.Where(x => x is FamilyInstance && x.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Windows).Cast<FamilyInstance>().ToList();
            List<FamilyInstance> doors = inserts.Where(x => x is FamilyInstance && x.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors).Cast<FamilyInstance>().ToList();

            Dictionary<PlanarSurface, List<PlanarSurface>> surfaces = roof.PanelSurfaces(windows.Union(doors).Select(x => x.Id), settings);
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
                roof.NoPanelLocationError();

            bHoMRoof = new oM.Physical.Elements.Roof { Location = location, Openings = openings, Construction = construction };
            bHoMRoof.Name = roof.FamilyTypeFullName();

            //Set identifiers, parameters & custom data
            bHoMRoof.SetIdentifiers(roof);
            bHoMRoof.CopyParameters(roof, settings.ParameterSettings);
            bHoMRoof.SetProperties(roof, settings.ParameterSettings);

            refObjects.AddOrReplace(roof.Id, bHoMRoof);
            return bHoMRoof;
        }

        /***************************************************/
    }
}
