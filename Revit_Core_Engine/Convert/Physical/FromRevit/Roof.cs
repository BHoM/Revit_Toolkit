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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit RoofBase to BH.oM.Physical.Elements.Roof.")]
        [Input("roof", "Revit RoofBase to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("roof", "BH.oM.Physical.Elements.Roof resulting from converting the input Revit RoofBase.")]
        public static oM.Physical.Elements.Roof RoofFromRevit(this RoofBase roof, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (roof == null)
                return null;

            settings = settings.DefaultIfNull();

            oM.Physical.Elements.Roof bHoMRoof = refObjects.GetValue<oM.Physical.Elements.Roof>(roof.Id);
            if (bHoMRoof != null)
                return bHoMRoof;

            List<CurtainGrid> curtainGrids = roof.ICurtainGrids();
            if (curtainGrids.Count != 0)
                bHoMRoof = roof.CurtainRoofFromRevit(settings, refObjects);
            else
                bHoMRoof = roof.SolidRoofFromRevit(settings, refObjects);

            if (bHoMRoof == null)
                return null;

            HostObjAttributes hostObjAttributes = roof.Document.GetElement(roof.GetTypeId()) as HostObjAttributes;
            string materialGrade = roof.MaterialGrade(settings);
            oM.Physical.Constructions.Construction construction = hostObjAttributes.ConstructionFromRevit(materialGrade, settings, refObjects);
            bHoMRoof.Construction = construction;

            bHoMRoof.Name = roof.FamilyTypeFullName();

            //Set identifiers, parameters & custom data
            bHoMRoof.SetIdentifiers(roof);
            bHoMRoof.CopyParameters(roof, settings.MappingSettings);
            bHoMRoof.SetProperties(roof, settings.MappingSettings);

            refObjects.AddOrReplace(roof.Id, bHoMRoof);
            return bHoMRoof;
        }


        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        [Description("Converts a Revit RoofBase to BH.oM.Physical.Elements.Roof in case the Revit Roof is a solid one.")]
        [Input("roof", "Revit RoofBase to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("roof", "BH.oM.Physical.Elements.Roof resulting from converting the input Revit RoofBase.")]
        private static oM.Physical.Elements.Roof SolidRoofFromRevit(this RoofBase roof, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            ISurface location = null;
            List<BH.oM.Physical.Elements.IOpening> openings = new List<oM.Physical.Elements.IOpening>();
            
            Dictionary<PlanarSurface, List<PlanarSurface>> surfaces = null;
            BoundingBoxXYZ bbox = roof.get_BoundingBox(null);
            if (bbox != null)
            {
                BoundingBoxIntersectsFilter bbif = new BoundingBoxIntersectsFilter(new Outline(bbox.Min, bbox.Max));
                IList<ElementId> insertIds = (roof as HostObject).FindInserts(true, true, true, true);
                List<Element> inserts;
                if (insertIds.Count == 0)
                    inserts = new List<Element>();
                else
                    inserts = new FilteredElementCollector(roof.Document, insertIds).WherePasses(bbif).ToList();

                List<FamilyInstance> windows = inserts.Where(x => x is FamilyInstance && x.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Windows).Cast<FamilyInstance>().ToList();
                List<FamilyInstance> doors = inserts.Where(x => x is FamilyInstance && x.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors).Cast<FamilyInstance>().ToList();

                surfaces = roof.PanelSurfaces(windows.Union(doors).Select(x => x.Id), settings);
                if (surfaces != null)
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
                        BH.oM.Physical.Elements.Window bHoMWindow = window.WindowFromRevit(roof, settings, refObjects);
                        if (bHoMWindow != null)
                            openings.Add(bHoMWindow);
                    }

                    foreach (FamilyInstance door in doors)
                    {
                        BH.oM.Physical.Elements.Door bHoMDoor = door.DoorFromRevit(roof, settings, refObjects);
                        if (bHoMDoor != null)
                            openings.Add(bHoMDoor);
                    }
                }
            }

            if (surfaces == null || surfaces.Count == 0)
                roof.NoPanelLocationError();

            return new oM.Physical.Elements.Roof { Location = location, Openings = openings };
        }

        /***************************************************/

        [Description("Converts a Revit RoofBase to BH.oM.Physical.Elements.Roof in case the Revit Roof is a curtain one.")]
        [Input("roof", "Revit RoofBase to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("roof", "BH.oM.Physical.Elements.Roof resulting from converting the input Revit RoofBase.")]
        private static oM.Physical.Elements.Roof CurtainRoofFromRevit(this RoofBase roof, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            List<BH.oM.Physical.Elements.IOpening> openings = new List<oM.Physical.Elements.IOpening>();
            List<CurtainGrid> curtainGrids = roof.ICurtainGrids();

            bool partFailed = false;
            foreach (CurtainGrid cg in curtainGrids)
            {
                List<BH.oM.Physical.Elements.IOpening> curtainPanels = cg.CurtainPanels(roof.Document, settings, refObjects);
                if (curtainPanels == null)
                    partFailed = true;
                else
                    openings.AddRange(curtainPanels);
            }

            ISurface location = null;
            if (openings == null || openings.Count == 0)
                BH.Engine.Base.Compute.RecordError(String.Format("Processing of panels of Revit curtain roof failed. BHoM roof without location has been returned. Revit ElementId: {0}", roof.Id.IntegerValue));
            else
            {
                if (partFailed)
                BH.Engine.Base.Compute.RecordError(String.Format("Processing of panels of a Revit curtain roof failed. Parts of the geometry of the BHoM roof may be missing. Revit ElementId: {0}", roof.Id.IntegerValue));

                if (openings.Count == 1)
                    location = openings[0].Location;
                else
                    location = new PolySurface { Surfaces = openings.Select(x => x.Location).ToList() };
            }

            return new oM.Physical.Elements.Roof { Location = location, Openings = openings };
        }

        /***************************************************/
    }
}


