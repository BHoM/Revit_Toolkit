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
using Autodesk.Revit.DB.Analysis;
using BH.Engine.Adapters.Revit;
using BH.Engine.Environment;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Environment.Fragments;
using BH.oM.Geometry;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit FamilyInstance to BH.oM.Environment.Elements.Panel.")]
        [Input("familyInstance", "Revit FamilyInstance to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("panel", "BH.oM.Environment.Elements.Panel resulting from converting the input Revit FamilyInstance.")]
        public static oM.Environment.Elements.Panel EnvironmentPanelFromRevit(this FamilyInstance familyInstance, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            //Create a BuildingElement from the familyInstance geometry
            settings = settings.DefaultIfNull();

            oM.Environment.Elements.Panel panel = refObjects.GetValue<oM.Environment.Elements.Panel>(familyInstance.Id);
            if (panel != null)
                return panel;

            PlanarSurface openingSurface = familyInstance.OpeningSurface(null, settings) as PlanarSurface;
            ICurve outline = openingSurface?.ExternalBoundary;
            if (outline == null)
                return null;

            panel = new oM.Environment.Elements.Panel()
            {
                ExternalEdges = outline.ToEdges(),
                Name = familyInstance.FamilyTypeFullName(),
            };

            //Set ExtendedProperties
            OriginContextFragment originContext = new OriginContextFragment() { ElementID = familyInstance.Id.IntegerValue.ToString(), TypeName = familyInstance.FamilyTypeFullName() };
            originContext.SetProperties(familyInstance.Symbol, settings.MappingSettings);
            originContext.SetProperties(familyInstance, settings.MappingSettings);
            panel.Fragments.Add(originContext);

            PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
            panelAnalytical.SetProperties(familyInstance.Symbol, settings.MappingSettings);
            panelAnalytical.SetProperties(familyInstance, settings.MappingSettings);
            panel.Fragments.Add(panelAnalytical);

            PanelContextFragment panelContext = new PanelContextFragment();
            panelContext.SetProperties(familyInstance.Symbol, settings.MappingSettings);
            panelContext.SetProperties(familyInstance, settings.MappingSettings);
            panel.Fragments.Add(panelContext);

            BuildingResultFragment buildingResults = new BuildingResultFragment();
            buildingResults.SetProperties(familyInstance.Symbol, settings.MappingSettings);
            buildingResults.SetProperties(familyInstance, settings.MappingSettings);
            panel.Fragments.Add(buildingResults);

            oM.Environment.Elements.PanelType? panelType = familyInstance.Category.PanelType();
            if (panelType.HasValue)
                panel.Type = panelType.Value;
            else
                panel.Type = oM.Environment.Elements.PanelType.Undefined;

            //Set identifiers, parameters & custom data
            panel.SetIdentifiers(familyInstance);
            panel.CopyParameters(familyInstance, settings.MappingSettings);
            panel.SetProperties(familyInstance, settings.MappingSettings);

            refObjects.AddOrReplace(familyInstance.Id, panel);
            return panel;
        }

        /***************************************************/

        [Description("Converts a Revit Ceiling to BH.oM.Environment.Elements.Panel.")]
        [Input("ceiling", "Revit Ceiling to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("panel", "BH.oM.Environment.Elements.Panel resulting from converting the input Revit Ceiling.")]
        public static List<oM.Environment.Elements.Panel> EnvironmentPanelsFromRevit(this Ceiling ceiling, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> panels = refObjects.GetValues<oM.Environment.Elements.Panel>(ceiling.Id);
            if (panels != null && panels.Count != 0)
                return panels;

            Dictionary<PlanarSurface, List<PlanarSurface>> surfaces = ceiling.PanelSurfaces(ceiling.FindInserts(true, true, true, true), settings);
            if (surfaces == null)
                return panels;

            CeilingType ceilingType = ceiling.Document.GetElement(ceiling.GetTypeId()) as CeilingType;
            BH.oM.Physical.Constructions.Construction construction = ceilingType.ConstructionFromRevit(null, settings, refObjects);
            
            panels = new List<oM.Environment.Elements.Panel>();
            foreach (PlanarSurface surface in surfaces.Keys)
            {
                //Create the BuildingElement
                oM.Environment.Elements.Panel panel = new oM.Environment.Elements.Panel()
                {
                    ExternalEdges = surface.ExternalBoundary.ToEdges(),
                    Name = ceiling.FamilyTypeFullName(),
                };

                //Set ExtendedProperties
                OriginContextFragment originContext = new OriginContextFragment() { ElementID = ceiling.Id.IntegerValue.ToString(), TypeName = ceiling.FamilyTypeFullName() };
                originContext.SetProperties(ceilingType, settings.MappingSettings);
                originContext.SetProperties(ceiling, settings.MappingSettings);
                panel.AddFragment(originContext);

                PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
                panelAnalytical.SetProperties(ceilingType, settings.MappingSettings);
                panelAnalytical.SetProperties(ceiling, settings.MappingSettings);
                panel.AddFragment(panelAnalytical);

                PanelContextFragment panelContext = new PanelContextFragment();
                panelContext.SetProperties(ceilingType, settings.MappingSettings);
                panelContext.SetProperties(ceiling, settings.MappingSettings);
                panel.AddFragment(panelContext);

                BuildingResultFragment buildingResults = new BuildingResultFragment();
                buildingResults.SetProperties(ceilingType, settings.MappingSettings);
                buildingResults.SetProperties(ceiling, settings.MappingSettings);
                panel.AddFragment(buildingResults);

                panel.Construction = construction;
                panel.Type = oM.Environment.Elements.PanelType.Ceiling;

                //Set identifiers, parameters & custom data
                panel.SetIdentifiers(ceiling);
                panel.CopyParameters(ceiling, settings.MappingSettings);
                panel.SetProperties(ceiling, settings.MappingSettings);

                refObjects.AddOrReplace(ceiling.Id, panel);
                panels.Add(panel);
            }
            
            return panels;
        }

        /***************************************************/

        [Description("Converts a Revit Floor to BH.oM.Environment.Elements.Panel.")]
        [Input("floor", "Revit Floor to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("panel", "BH.oM.Environment.Elements.Panel resulting from converting the input Revit Floor.")]
        public static List<oM.Environment.Elements.Panel> EnvironmentPanelsFromRevit(this Floor floor, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> panels = refObjects.GetValues<oM.Environment.Elements.Panel>(floor.Id);
            if (panels != null && panels.Count != 0)
                return panels;

            Dictionary<PlanarSurface, List<PlanarSurface>> surfaces = floor.PanelSurfaces(floor.FindInserts(true, true, true, true), settings);
            if (surfaces == null)
                return panels;

            FloorType floorType = floor.FloorType;
            BH.oM.Physical.Constructions.Construction construction = floorType.ConstructionFromRevit(null, settings, refObjects);
            
            panels = new List<oM.Environment.Elements.Panel>();
            foreach (PlanarSurface surface in surfaces.Keys)
            {
                //Create the BuildingElement
                oM.Environment.Elements.Panel panel = new oM.Environment.Elements.Panel()
                {
                    ExternalEdges = surface.ExternalBoundary.ToEdges(),
                    Name = floor.FamilyTypeFullName(),
                };

                //Set ExtendedProperties
                OriginContextFragment originContext = new OriginContextFragment() { ElementID = floor.Id.IntegerValue.ToString(), TypeName = floor.FamilyTypeFullName() };
                originContext.SetProperties(floorType, settings.MappingSettings);
                originContext.SetProperties(floor, settings.MappingSettings);
                panel.AddFragment(originContext);

                PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
                panelAnalytical.SetProperties(floorType, settings.MappingSettings);
                panelAnalytical.SetProperties(floor, settings.MappingSettings);
                panel.AddFragment(panelAnalytical);

                PanelContextFragment panelContext = new PanelContextFragment();
                panelContext.SetProperties(floorType, settings.MappingSettings);
                panelContext.SetProperties(floor, settings.MappingSettings);
                panel.AddFragment(panelContext);

                BuildingResultFragment buildingResults = new BuildingResultFragment();
                buildingResults.SetProperties(floorType, settings.MappingSettings);
                buildingResults.SetProperties(floor, settings.MappingSettings);
                panel.AddFragment(buildingResults);

                panel.Construction = construction;
                panel.Type = oM.Environment.Elements.PanelType.Floor;

                //Set identifiers, parameters & custom data
                panel.SetIdentifiers(floor);
                panel.CopyParameters(floor, settings.MappingSettings);
                panel.SetProperties(floor, settings.MappingSettings);

                refObjects.AddOrReplace(floor.Id, panel);

                panels.Add(panel);
            }
            
            return panels;
        }

        /***************************************************/

        [Description("Converts a Revit RoofBase to BH.oM.Environment.Elements.Panel.")]
        [Input("roofBase", "Revit RoofBase to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("panel", "BH.oM.Environment.Elements.Panel resulting from converting the input Revit RoofBase.")]
        public static List<oM.Environment.Elements.Panel> EnvironmentPanelsFromRevit(this RoofBase roofBase, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> panels = refObjects.GetValues<oM.Environment.Elements.Panel>(roofBase.Id);
            if (panels != null && panels.Count > 0)
                return panels;

            Dictionary<PlanarSurface, List<PlanarSurface>> surfaces = roofBase.PanelSurfaces(roofBase.FindInserts(true, true, true, true), settings);
            if (surfaces == null)
                return panels;
            
            BH.oM.Physical.Constructions.Construction construction = roofBase.RoofType.ConstructionFromRevit(null, settings, refObjects);

            panels = new List<oM.Environment.Elements.Panel>();
            foreach (PlanarSurface surface in surfaces.Keys)
            {
                //Create the BuildingElement
                oM.Environment.Elements.Panel panel = new oM.Environment.Elements.Panel()
                {
                    ExternalEdges = surface.ExternalBoundary.ToEdges(),
                    Name = roofBase.FamilyTypeFullName(),
                };

                //Set ExtendedProperties
                OriginContextFragment originContext = new OriginContextFragment() { ElementID = roofBase.Id.IntegerValue.ToString(), TypeName = roofBase.FamilyTypeFullName() };
                originContext.SetProperties(roofBase.RoofType, settings.MappingSettings);
                originContext.SetProperties(roofBase, settings.MappingSettings);
                panel.AddFragment(originContext);

                PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
                panelAnalytical.SetProperties(roofBase.RoofType, settings.MappingSettings);
                panelAnalytical.SetProperties(roofBase, settings.MappingSettings);
                panel.AddFragment(panelAnalytical);

                PanelContextFragment panelContext = new PanelContextFragment();
                panelContext.SetProperties(roofBase.RoofType, settings.MappingSettings);
                panelContext.SetProperties(roofBase, settings.MappingSettings);
                panel.AddFragment(panelContext);

                BuildingResultFragment buildingResults = new BuildingResultFragment();
                buildingResults.SetProperties(roofBase.RoofType, settings.MappingSettings);
                buildingResults.SetProperties(roofBase, settings.MappingSettings);
                panel.AddFragment(buildingResults);

                panel.Construction = construction;
                panel.Type = oM.Environment.Elements.PanelType.Roof;

                //Set identifiers, parameters & custom data
                panel.SetIdentifiers(roofBase);
                panel.CopyParameters(roofBase, settings.MappingSettings);
                panel.SetProperties(roofBase, settings.MappingSettings);

                refObjects.AddOrReplace(roofBase.Id, panel);

                panels.Add(panel);
            }
            
            return panels;
        }

        /***************************************************/

        [Description("Converts a Revit Wall to BH.oM.Environment.Elements.Panel.")]
        [Input("wall", "Revit Wall to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("panel", "BH.oM.Environment.Elements.Panel resulting from converting the input Revit Wall.")]
        public static List<oM.Environment.Elements.Panel> EnvironmentPanelsFromRevit(this Wall wall, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> panels = refObjects.GetValues<oM.Environment.Elements.Panel>(wall.Id);
            if (panels != null && panels.Count > 0)
                return panels;

            if (wall.StackedWallOwnerId != null && wall.StackedWallOwnerId != ElementId.InvalidElementId)
                return null;

            Dictionary<PlanarSurface, List<PlanarSurface>> surfaces = wall.PanelSurfaces(wall.FindInserts(true, true, true, true), settings);
            if (surfaces == null)
                return panels;

            BH.oM.Physical.Constructions.Construction constrtuction = wall.WallType.ConstructionFromRevit(null, settings, refObjects);
            
            panels = new List<oM.Environment.Elements.Panel>();
            foreach (PlanarSurface surface in surfaces.Keys)
            {
                //Create the BuildingElement
                oM.Environment.Elements.Panel panel = new oM.Environment.Elements.Panel()
                {
                    ExternalEdges = surface.ExternalBoundary.ToEdges(),
                    Name = wall.FamilyTypeFullName(),
                };

                //Set ExtendedProperties
                OriginContextFragment originContext = new OriginContextFragment() { ElementID = wall.Id.IntegerValue.ToString(), TypeName = wall.FamilyTypeFullName() };
                originContext.SetProperties(wall.WallType, settings.MappingSettings);
                originContext.SetProperties(wall, settings.MappingSettings);
                panel.AddFragment(originContext);

                PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
                panelAnalytical.SetProperties(wall.WallType, settings.MappingSettings);
                panelAnalytical.SetProperties(wall, settings.MappingSettings);
                panel.AddFragment(panelAnalytical);

                PanelContextFragment panelContext = new PanelContextFragment();
                panelContext.SetProperties(wall.WallType, settings.MappingSettings);
                panelContext.SetProperties(wall, settings.MappingSettings);
                panel.AddFragment(panelContext);

                BuildingResultFragment buildingResults = new BuildingResultFragment();
                buildingResults.SetProperties(wall.WallType, settings.MappingSettings);
                buildingResults.SetProperties(wall, settings.MappingSettings);
                panel.AddFragment(buildingResults);

                panel.Construction = constrtuction;
                panel.Type = oM.Environment.Elements.PanelType.Wall;

                //Set identifiers, parameters & custom data
                panel.SetIdentifiers(wall);
                panel.CopyParameters(wall, settings.MappingSettings);
                panel.SetProperties(wall, settings.MappingSettings);

                refObjects.AddOrReplace(wall.Id, panel);

                panels.Add(panel);
            }
            
            return panels;
        }


        /***************************************************/
        /****             Private Methods               ****/
        /***************************************************/

        [Description("Converts a Revit EnergyAnalysisSurface to BH.oM.Environment.Elements.Panel.")]
        [Input("energyAnalysisSurface", "Revit EnergyAnalysisSurface to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("panel", "BH.oM.Environment.Elements.Panel resulting from converting the input Revit EnergyAnalysisSurface.")]
        private static oM.Environment.Elements.Panel EnvironmentPanelFromRevit(this EnergyAnalysisSurface energyAnalysisSurface, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            oM.Environment.Elements.Panel panel = refObjects.GetValue<oM.Environment.Elements.Panel>(energyAnalysisSurface.Id);
            if (panel != null)
                return panel;

            //Get the geometry Curve
            ICurve curve = null;
            if (energyAnalysisSurface != null)
                curve = energyAnalysisSurface.GetPolyloop().FromRevit();

            //Get the name and element type
            Element element = energyAnalysisSurface.Document.Element(energyAnalysisSurface.CADObjectUniqueId, energyAnalysisSurface.CADLinkUniqueId);
            ElementType elementType = null;
            if (element == null)
                return panel;
            
            elementType = element.Document.GetElement(element.GetTypeId()) as ElementType;
            panel = new oM.Environment.Elements.Panel()
            {
                ExternalEdges = curve.ToEdges(),
                Name = element.FamilyTypeFullName(),
            };

            //Set ExtendedProperties
            OriginContextFragment originContext = new OriginContextFragment() { ElementID = element.Id.IntegerValue.ToString(), TypeName = element.FamilyTypeFullName() };
            originContext.SetProperties(element, settings.MappingSettings);
            originContext.SetProperties(elementType, settings.MappingSettings);
            panel.AddFragment(originContext);

            PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
            panelAnalytical.SetProperties(elementType, settings.MappingSettings);
            panelAnalytical.SetProperties(element, settings.MappingSettings);
            panel.AddFragment(panelAnalytical);

            PanelContextFragment panelContext = new PanelContextFragment();

            List<string> connectedSpaces = new List<string>();
            EnergyAnalysisSpace energyAnalysisSpace = null;
            energyAnalysisSpace = energyAnalysisSurface.GetAnalyticalSpace();
            if (energyAnalysisSpace != null)
            {
                SpatialElement spatialElement = energyAnalysisSpace.Document.Element(energyAnalysisSpace.CADObjectUniqueId) as SpatialElement;
                connectedSpaces.Add(Query.Name(spatialElement));
            }

            energyAnalysisSpace = energyAnalysisSurface.GetAdjacentAnalyticalSpace();
            if (energyAnalysisSpace != null)
            {
                SpatialElement spatialElement = energyAnalysisSpace.Document.Element(energyAnalysisSpace.CADObjectUniqueId) as SpatialElement;
                if (spatialElement != null)
                {
                    connectedSpaces.Add(Query.Name(spatialElement));

                    if (spatialElement is Autodesk.Revit.DB.Mechanical.Space)
                    {
                        Autodesk.Revit.DB.Mechanical.Space space = (Autodesk.Revit.DB.Mechanical.Space)spatialElement;

                        BuildingResultFragment buildingResultsProperties = new BuildingResultFragment();
                        buildingResultsProperties.PeakCooling = space.DesignCoolingLoad.ToSI(SpecTypeId.CoolingLoad);
                        buildingResultsProperties.PeakHeating = space.DesignHeatingLoad.ToSI(SpecTypeId.HeatingLoad);
                        panel.AddFragment(buildingResultsProperties);
                    }
                }
            }

            panel.ConnectedSpaces = connectedSpaces;
            panelContext.SetProperties(elementType, settings.MappingSettings);
            panelContext.SetProperties(element, settings.MappingSettings);
            panel.AddFragment(panelContext);

            oM.Environment.Elements.PanelType? panelType = element.Category.PanelType();
            if (panelType.HasValue)
                panel.Type = panelType.Value;
            else
                panel.Type = oM.Environment.Elements.PanelType.Undefined;

            panel.Construction = Convert.ConstructionFromRevit(elementType as dynamic, null, settings, refObjects);

            //Set identifiers, parameters & custom data
            panel.SetIdentifiers(element);
            panel.CopyParameters(element, settings.MappingSettings);
            panel.SetProperties(element, settings.MappingSettings);

            refObjects.AddOrReplace(energyAnalysisSurface.Id, panel);
            return panel;
        }

        /***************************************************/
    }
}


