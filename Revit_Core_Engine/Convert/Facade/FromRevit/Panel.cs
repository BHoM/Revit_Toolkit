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
using BH.oM.Base;
using BH.oM.Facade.Elements;
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

        [Description("Converts a Revit FamilyInstance to BH.oM.Facade.Elements.Panel.")]
        [Input("familyInstance", "Revit FamilyInstance to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("panel", "BH.oM.Facade.Elements.Panel resulting from converting the input Revit FamilyInstance.")]
        public static oM.Facade.Elements.Panel FacadePanelFromRevit(this FamilyInstance familyInstance, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            BH.Engine.Base.Compute.RecordError("Conversion of Panels from Revit for the Facade discipline is not yet implemented.");
            return null;
        }

        /***************************************************/

        [Description("Converts a Revit Wall to BH.oM.Facade.Elements.Panel.")]
        [Input("wall", "Revit Wall to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("panel", "BH.oM.Facade.Elements.Panel resulting from converting the input Revit Wall.")]
        public static oM.Facade.Elements.Panel FacadePanelFromRevit(this Wall wall, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (wall == null)
                return null; 
            
            settings = settings.DefaultIfNull();

            oM.Facade.Elements.Panel bHoMPanel = refObjects.GetValue<oM.Facade.Elements.Panel>(wall.Id);
            if (bHoMPanel != null)
                return bHoMPanel;

            if (wall.StackedWallOwnerId != null && wall.StackedWallOwnerId != ElementId.InvalidElementId && wall.IsStackedWallMember)
                return null;

            ISurface location = null;
            List <BH.oM.Facade.Elements.Opening> openings = new List<BH.oM.Facade.Elements.Opening>();

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
                        foreach (PlanarSurface srf in ps)
                        {
                            List<FrameEdge> frameEdges = new List<FrameEdge>();
                            foreach (ICurve crv in srf.IExternalEdges())
                            {
                                frameEdges.Add(new FrameEdge { Curve = crv });
                            }
                            BH.oM.Facade.Elements.Opening opening = new BH.oM.Facade.Elements.Opening { Edges = frameEdges };
                            opening.Type = OpeningType.Hole;
                            openings.Add(opening);
                        }
                    }

                    foreach (FamilyInstance window in inserts.Where(x => x.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Windows))
                    {
                        BH.oM.Facade.Elements.Opening bHoMWindow = window.FacadeOpeningFromRevit(wall, settings, refObjects);
                        if (bHoMWindow != null)
                            openings.Add(bHoMWindow);
                    }

                    foreach (FamilyInstance door in inserts.Where(x => x.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors))
                    {
                        BH.oM.Facade.Elements.Opening bHoMDoor = door.FacadeOpeningFromRevit(wall, settings, refObjects);
                        if (bHoMDoor != null)
                            openings.Add(bHoMDoor);
                    }
                }
            }

            if (surfaces == null || surfaces.Count == 0)
                wall.NoPanelLocationError();

            List<FrameEdge> panelEdges = new List<FrameEdge>();
            panelEdges.AddRange(location.IExternalEdges().Select(x => new BH.oM.Facade.Elements.FrameEdge { Curve = x }));
            BH.oM.Facade.Elements.Panel bhomPanel = BH.Engine.Facade.Create.Panel(panelEdges, openings);

            HostObjAttributes hostObjAttributes = wall.Document.GetElement(wall.GetTypeId()) as HostObjAttributes;
            string materialGrade = wall.MaterialGrade(settings);
            oM.Physical.Constructions.Construction construction = hostObjAttributes.ConstructionFromRevit(materialGrade, settings, refObjects);
            bhomPanel.Construction = construction;

            bhomPanel.Name = wall.FamilyTypeFullName();

            //Set identifiers, parameters & custom data
            bhomPanel.SetIdentifiers(wall);
            bhomPanel.CopyParameters(wall, settings.MappingSettings);
            bhomPanel.SetProperties(wall, settings.MappingSettings);

            refObjects.AddOrReplace(wall.Id, bhomPanel);

            return bhomPanel; 
        }

        /***************************************************/
    }
}


