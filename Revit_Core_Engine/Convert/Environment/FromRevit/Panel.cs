/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using System.Collections.Generic;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

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
            originContext.SetProperties(familyInstance.Symbol, settings.ParameterSettings);
            originContext.SetProperties(familyInstance, settings.ParameterSettings);
            panel.Fragments.Add(originContext);

            PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
            panelAnalytical.SetProperties(familyInstance.Symbol, settings.ParameterSettings);
            panelAnalytical.SetProperties(familyInstance, settings.ParameterSettings);
            panel.Fragments.Add(panelAnalytical);

            PanelContextFragment panelContext = new PanelContextFragment();
            panelContext.SetProperties(familyInstance.Symbol, settings.ParameterSettings);
            panelContext.SetProperties(familyInstance, settings.ParameterSettings);
            panel.Fragments.Add(panelContext);

            BuildingResultFragment buildingResults = new BuildingResultFragment();
            buildingResults.SetProperties(familyInstance.Symbol, settings.ParameterSettings);
            buildingResults.SetProperties(familyInstance, settings.ParameterSettings);
            panel.Fragments.Add(buildingResults);

            oM.Environment.Elements.PanelType? panelType = familyInstance.Category.PanelType();
            if (panelType.HasValue)
                panel.Type = panelType.Value;
            else
                panel.Type = oM.Environment.Elements.PanelType.Undefined;

            //Set identifiers, parameters & custom data
            panel.SetIdentifiers(familyInstance);
            panel.CopyParameters(familyInstance, settings.ParameterSettings);
            panel.SetProperties(familyInstance, settings.ParameterSettings);

            refObjects.AddOrReplace(familyInstance.Id, panel);
            return panel;
        }

        /***************************************************/

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
                originContext.SetProperties(ceilingType, settings.ParameterSettings);
                originContext.SetProperties(ceiling, settings.ParameterSettings);
                panel.AddFragment(originContext);

                PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
                panelAnalytical.SetProperties(ceilingType, settings.ParameterSettings);
                panelAnalytical.SetProperties(ceiling, settings.ParameterSettings);
                panel.AddFragment(panelAnalytical);

                PanelContextFragment panelContext = new PanelContextFragment();
                panelContext.SetProperties(ceilingType, settings.ParameterSettings);
                panelContext.SetProperties(ceiling, settings.ParameterSettings);
                panel.AddFragment(panelContext);

                BuildingResultFragment buildingResults = new BuildingResultFragment();
                buildingResults.SetProperties(ceilingType, settings.ParameterSettings);
                buildingResults.SetProperties(ceiling, settings.ParameterSettings);
                panel.AddFragment(buildingResults);

                panel.Construction = construction;
                panel.Type = oM.Environment.Elements.PanelType.Ceiling;

                //Set identifiers, parameters & custom data
                panel.SetIdentifiers(ceiling);
                panel.CopyParameters(ceiling, settings.ParameterSettings);
                panel.SetProperties(ceiling, settings.ParameterSettings);

                refObjects.AddOrReplace(ceiling.Id, panel);
                panels.Add(panel);
            }
            
            return panels;
        }

        /***************************************************/

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
                originContext.SetProperties(floorType, settings.ParameterSettings);
                originContext.SetProperties(floor, settings.ParameterSettings);
                panel.AddFragment(originContext);

                PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
                panelAnalytical.SetProperties(floorType, settings.ParameterSettings);
                panelAnalytical.SetProperties(floor, settings.ParameterSettings);
                panel.AddFragment(panelAnalytical);

                PanelContextFragment panelContext = new PanelContextFragment();
                panelContext.SetProperties(floorType, settings.ParameterSettings);
                panelContext.SetProperties(floor, settings.ParameterSettings);
                panel.AddFragment(panelContext);

                BuildingResultFragment buildingResults = new BuildingResultFragment();
                buildingResults.SetProperties(floorType, settings.ParameterSettings);
                buildingResults.SetProperties(floor, settings.ParameterSettings);
                panel.AddFragment(buildingResults);

                panel.Construction = construction;
                panel.Type = oM.Environment.Elements.PanelType.Floor;

                //Set identifiers, parameters & custom data
                panel.SetIdentifiers(floor);
                panel.CopyParameters(floor, settings.ParameterSettings);
                panel.SetProperties(floor, settings.ParameterSettings);

                refObjects.AddOrReplace(floor.Id, panel);

                panels.Add(panel);
            }
            
            return panels;
        }

        /***************************************************/

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
                originContext.SetProperties(roofBase.RoofType, settings.ParameterSettings);
                originContext.SetProperties(roofBase, settings.ParameterSettings);
                panel.AddFragment(originContext);

                PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
                panelAnalytical.SetProperties(roofBase.RoofType, settings.ParameterSettings);
                panelAnalytical.SetProperties(roofBase, settings.ParameterSettings);
                panel.AddFragment(panelAnalytical);

                PanelContextFragment panelContext = new PanelContextFragment();
                panelContext.SetProperties(roofBase.RoofType, settings.ParameterSettings);
                panelContext.SetProperties(roofBase, settings.ParameterSettings);
                panel.AddFragment(panelContext);

                BuildingResultFragment buildingResults = new BuildingResultFragment();
                buildingResults.SetProperties(roofBase.RoofType, settings.ParameterSettings);
                buildingResults.SetProperties(roofBase, settings.ParameterSettings);
                panel.AddFragment(buildingResults);

                panel.Construction = construction;
                panel.Type = oM.Environment.Elements.PanelType.Roof;

                //Set identifiers, parameters & custom data
                panel.SetIdentifiers(roofBase);
                panel.CopyParameters(roofBase, settings.ParameterSettings);
                panel.SetProperties(roofBase, settings.ParameterSettings);

                refObjects.AddOrReplace(roofBase.Id, panel);

                panels.Add(panel);
            }
            
            return panels;
        }

        /***************************************************/

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
                originContext.SetProperties(wall.WallType, settings.ParameterSettings);
                originContext.SetProperties(wall, settings.ParameterSettings);
                panel.AddFragment(originContext);

                PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
                panelAnalytical.SetProperties(wall.WallType, settings.ParameterSettings);
                panelAnalytical.SetProperties(wall, settings.ParameterSettings);
                panel.AddFragment(panelAnalytical);

                PanelContextFragment panelContext = new PanelContextFragment();
                panelContext.SetProperties(wall.WallType, settings.ParameterSettings);
                panelContext.SetProperties(wall, settings.ParameterSettings);
                panel.AddFragment(panelContext);

                BuildingResultFragment buildingResults = new BuildingResultFragment();
                buildingResults.SetProperties(wall.WallType, settings.ParameterSettings);
                buildingResults.SetProperties(wall, settings.ParameterSettings);
                panel.AddFragment(buildingResults);

                panel.Construction = constrtuction;
                panel.Type = oM.Environment.Elements.PanelType.Wall;

                //Set identifiers, parameters & custom data
                panel.SetIdentifiers(wall);
                panel.CopyParameters(wall, settings.ParameterSettings);
                panel.SetProperties(wall, settings.ParameterSettings);

                refObjects.AddOrReplace(wall.Id, panel);

                panels.Add(panel);
            }
            
            return panels;
        }


        /***************************************************/
        /****             Internal Methods              ****/
        /***************************************************/

        internal static oM.Environment.Elements.Panel EnvironmentPanelFromRevit(this Element element, ICurve crv, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            oM.Environment.Elements.Panel panel = refObjects.GetValue<oM.Environment.Elements.Panel>(element.Id);
            if (panel != null)
                return panel;

            ElementType elementType = element.Document.GetElement(element.GetTypeId()) as ElementType;

            panel = new oM.Environment.Elements.Panel()
            {
                ExternalEdges = crv.ToEdges(),
                Name = element.FamilyTypeFullName(),
            };

            //Set ExtendedProperties
            OriginContextFragment originContext = new OriginContextFragment() { ElementID = element.Id.IntegerValue.ToString(), TypeName = element.FamilyTypeFullName() };
            originContext.SetProperties(element, settings.ParameterSettings);
            originContext.SetProperties(elementType, settings.ParameterSettings);
            panel.Fragments.Add(originContext);

            PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
            panelAnalytical.SetProperties(element, settings.ParameterSettings);
            panelAnalytical.SetProperties(elementType, settings.ParameterSettings);
            panel.Fragments.Add(panelAnalytical);

            PanelContextFragment panelContext = new PanelContextFragment();
            panelContext.SetProperties(element, settings.ParameterSettings);
            panelContext.SetProperties(elementType, settings.ParameterSettings);
            panel.Fragments.Add(panelContext);

            BuildingResultFragment buildingResults = new BuildingResultFragment();
            buildingResults.SetProperties(element, settings.ParameterSettings);
            buildingResults.SetProperties(elementType, settings.ParameterSettings);
            panel.Fragments.Add(buildingResults);

            oM.Environment.Elements.PanelType? panelType = element.Category.PanelType();
            if (panelType.HasValue)
                panel.Type = panelType.Value;
            else
                panel.Type = oM.Environment.Elements.PanelType.Undefined;

            //Set identifiers, parameters & custom data
            panel.SetIdentifiers(element);
            panel.CopyParameters(element, settings.ParameterSettings);
            panel.SetProperties(element, settings.ParameterSettings);

            refObjects.AddOrReplace(element.Id, panel);
            return panel;
        }

        /***************************************************/

        internal static oM.Environment.Elements.Panel EnvironmentPanelFromRevit(this EnergyAnalysisSurface energyAnalysisSurface, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
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
            originContext.SetProperties(element, settings.ParameterSettings);
            originContext.SetProperties(elementType, settings.ParameterSettings);
            panel.AddFragment(originContext);

            PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
            panelAnalytical.SetProperties(elementType, settings.ParameterSettings);
            panelAnalytical.SetProperties(element, settings.ParameterSettings);
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
                        buildingResultsProperties.PeakCooling = space.DesignCoolingLoad.ToSI(UnitType.UT_HVAC_Cooling_Load);
                        buildingResultsProperties.PeakHeating = space.DesignHeatingLoad.ToSI(UnitType.UT_HVAC_Heating_Load);
                        panel.AddFragment(buildingResultsProperties);
                    }
                }
            }

            panel.ConnectedSpaces = connectedSpaces;
            panelContext.SetProperties(elementType, settings.ParameterSettings);
            panelContext.SetProperties(element, settings.ParameterSettings);
            panel.AddFragment(panelContext);

            oM.Environment.Elements.PanelType? panelType = element.Category.PanelType();
            if (panelType.HasValue)
                panel.Type = panelType.Value;
            else
                panel.Type = oM.Environment.Elements.PanelType.Undefined;

            panel.Construction = Convert.ConstructionFromRevit(elementType as dynamic, null, settings, refObjects);

            //Set identifiers, parameters & custom data
            panel.SetIdentifiers(element);
            panel.CopyParameters(element, settings.ParameterSettings);
            panel.SetProperties(element, settings.ParameterSettings);

            refObjects.AddOrReplace(energyAnalysisSurface.Id, panel);
            return panel;
        }

        /***************************************************/
    }
}

