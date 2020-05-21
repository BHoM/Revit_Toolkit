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
using Autodesk.Revit.DB.Analysis;
using BH.Engine.Adapters.Revit;
using BH.Engine.Environment;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Environment.Fragments;
using BH.oM.Geometry;
using System.Collections.Generic;

namespace BH.UI.Revit.Engine
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

            PolyCurve polycurve = familyInstance.PolyCurve(settings);
            if (polycurve == null)
                return null;

            panel = BH.Engine.Environment.Create.Panel(externalEdges: polycurve.ToEdges());
            panel.Name = familyInstance.FamilyTypeFullName();

            //Set ExtendedProperties
            OriginContextFragment originContext = new OriginContextFragment() { ElementID = familyInstance.Id.IntegerValue.ToString(), TypeName = familyInstance.FamilyTypeFullName() };
            originContext.SetParameters(familyInstance.Symbol, settings.ParameterSettings);
            originContext.SetParameters(familyInstance, settings.ParameterSettings);
            panel.Fragments.Add(originContext);

            PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
            panelAnalytical.SetParameters(familyInstance.Symbol, settings.ParameterSettings);
            panelAnalytical.SetParameters(familyInstance, settings.ParameterSettings);
            panel.Fragments.Add(panelAnalytical);

            PanelContextFragment panelContext = new PanelContextFragment();
            panelContext.SetParameters(familyInstance.Symbol, settings.ParameterSettings);
            panelContext.SetParameters(familyInstance, settings.ParameterSettings);
            panel.Fragments.Add(panelContext);

            BuildingResultFragment buildingResults = new BuildingResultFragment();
            buildingResults.SetParameters(familyInstance.Symbol, settings.ParameterSettings);
            buildingResults.SetParameters(familyInstance, settings.ParameterSettings);
            panel.Fragments.Add(buildingResults);

            oM.Environment.Elements.PanelType? panelType = familyInstance.Category.PanelType();
            if (panelType.HasValue)
                panel.Type = panelType.Value;
            else
                panel.Type = oM.Environment.Elements.PanelType.Undefined;

            //Set identifiers, parameters & custom data
            panel.SetIdentifiers(familyInstance);
            panel.SetCustomData(familyInstance, settings.ParameterSettings);
            panel.SetParameters(familyInstance, settings.ParameterSettings);
            panel.SetParameters(familyInstance.Symbol, settings.ParameterSettings);

            refObjects.AddOrReplace(familyInstance.Id, panel);

            panel = panel.UpdateBuildingElementTypeByCustomData();
            return panel;
        }

        /***************************************************/

        public static List<oM.Environment.Elements.Panel> EnvironmentPanelsFromRevit(this Ceiling ceiling, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> panels = refObjects.GetValues<oM.Environment.Elements.Panel>(ceiling.Id);
            if (panels != null && panels.Count != 0)
                return panels;

            List<PolyCurve> polycurves = ceiling.Profiles(settings);
            if (polycurves == null)
                return panels;

            panels = new List<oM.Environment.Elements.Panel>();

            CeilingType ceilingType = ceiling.Document.GetElement(ceiling.GetTypeId()) as CeilingType;
            BH.oM.Physical.Constructions.Construction construction = ceilingType.ConstructionFromRevit(settings, refObjects);

            List<PolyCurve> polycurveListOuter = polycurves.OuterPolyCurves();
            foreach (ICurve curve in polycurveListOuter)
            {
                //Create the BuildingElement
                oM.Environment.Elements.Panel panel = BH.Engine.Environment.Create.Panel(externalEdges: curve.ToEdges());
                panel.Name = ceiling.FamilyTypeFullName();

                //Set ExtendedProperties
                OriginContextFragment originContext = new OriginContextFragment() { ElementID = ceiling.Id.IntegerValue.ToString(), TypeName = ceiling.FamilyTypeFullName() };
                originContext.SetParameters(ceilingType, settings.ParameterSettings);
                originContext.SetParameters(ceiling, settings.ParameterSettings);
                panel.AddFragment(originContext);

                PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
                panelAnalytical.SetParameters(ceilingType, settings.ParameterSettings);
                panelAnalytical.SetParameters(ceiling, settings.ParameterSettings);
                panel.AddFragment(panelAnalytical);

                PanelContextFragment panelContext = new PanelContextFragment();
                panelContext.SetParameters(ceilingType, settings.ParameterSettings);
                panelContext.SetParameters(ceiling, settings.ParameterSettings);
                panel.AddFragment(panelContext);

                BuildingResultFragment buildingResults = new BuildingResultFragment();
                buildingResults.SetParameters(ceilingType, settings.ParameterSettings);
                buildingResults.SetParameters(ceiling, settings.ParameterSettings);
                panel.AddFragment(buildingResults);

                panel.Construction = construction;
                panel.Type = oM.Environment.Elements.PanelType.Ceiling;

                //Set identifiers, parameters & custom data
                panel.SetIdentifiers(ceiling);
                panel.SetCustomData(ceiling, settings.ParameterSettings);
                panel.SetParameters(ceiling, settings.ParameterSettings);
                panel.SetParameters(ceilingType, settings.ParameterSettings);

                refObjects.AddOrReplace(ceiling.Id, panel);
                panels.Add(panel);
            }

            panels = panels.UpdateBuildingElementTypeByCustomData();
            return panels;
        }

        /***************************************************/

        public static List<oM.Environment.Elements.Panel> EnvironmentPanelsFromRevit(this Floor floor, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> panels = refObjects.GetValues<oM.Environment.Elements.Panel>(floor.Id);
            if (panels != null && panels.Count != 0)
                return panels;

            List<PolyCurve> polycurves = floor.Profiles(settings);
            if (polycurves == null)
                return panels;

            panels = new List<oM.Environment.Elements.Panel>();

            BH.oM.Physical.Constructions.Construction construction = floor.FloorType.ConstructionFromRevit(settings, refObjects);

            FloorType floorType = floor.Document.GetElement(floor.GetTypeId()) as FloorType;

            List<PolyCurve> polycurveListOuter = polycurves.OuterPolyCurves();
            foreach (ICurve curve in polycurveListOuter)
            {
                //Create the BuildingElement
                oM.Environment.Elements.Panel panel = BH.Engine.Environment.Create.Panel(externalEdges: curve.ToEdges());
                panel.Name = floor.FamilyTypeFullName();

                //Set ExtendedProperties
                OriginContextFragment originContext = new OriginContextFragment() { ElementID = floor.Id.IntegerValue.ToString(), TypeName = floor.FamilyTypeFullName() };
                originContext.SetParameters(floorType, settings.ParameterSettings);
                originContext.SetParameters(floor, settings.ParameterSettings);
                panel.AddFragment(originContext);

                PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
                panelAnalytical.SetParameters(floorType, settings.ParameterSettings);
                panelAnalytical.SetParameters(floor, settings.ParameterSettings);
                panel.AddFragment(panelAnalytical);

                PanelContextFragment panelContext = new PanelContextFragment();
                panelContext.SetParameters(floorType, settings.ParameterSettings);
                panelContext.SetParameters(floor, settings.ParameterSettings);
                panel.AddFragment(panelContext);

                BuildingResultFragment buildingResults = new BuildingResultFragment();
                buildingResults.SetParameters(floorType, settings.ParameterSettings);
                buildingResults.SetParameters(floor, settings.ParameterSettings);
                panel.AddFragment(buildingResults);

                panel.Construction = construction;
                panel.Type = oM.Environment.Elements.PanelType.Floor;

                //Set identifiers, parameters & custom data
                panel.SetIdentifiers(floor);
                panel.SetCustomData(floor, settings.ParameterSettings);
                panel.SetParameters(floor, settings.ParameterSettings);
                panel.SetParameters(floorType, settings.ParameterSettings);

                refObjects.AddOrReplace(floor.Id, panel);

                panels.Add(panel);
            }

            panels = panels.UpdateBuildingElementTypeByCustomData();
            return panels;
        }

        /***************************************************/

        public static List<oM.Environment.Elements.Panel> EnvironmentPanelsFromRevit(this RoofBase roofBase, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> panels = refObjects.GetValues<oM.Environment.Elements.Panel>(roofBase.Id);
            if (panels != null && panels.Count > 0)
                return panels;

            List<PolyCurve> polycurves = roofBase.Profiles(settings);
            if (polycurves == null)
                return panels;

            panels = new List<oM.Environment.Elements.Panel>();

            BH.oM.Physical.Constructions.Construction construction = roofBase.RoofType.ConstructionFromRevit(settings, refObjects);

            List<PolyCurve> polycurvesListOuter = polycurves.OuterPolyCurves();
            foreach (ICurve curve in polycurvesListOuter)
            {
                //Create the BuildingElement
                oM.Environment.Elements.Panel panel = BH.Engine.Environment.Create.Panel(externalEdges: curve.ToEdges());
                panel.Name = roofBase.FamilyTypeFullName();

                //Set ExtendedProperties
                OriginContextFragment originContext = new OriginContextFragment() { ElementID = roofBase.Id.IntegerValue.ToString(), TypeName = roofBase.FamilyTypeFullName() };
                originContext.SetParameters(roofBase.RoofType, settings.ParameterSettings);
                originContext.SetParameters(roofBase, settings.ParameterSettings);
                panel.AddFragment(originContext);

                PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
                panelAnalytical.SetParameters(roofBase.RoofType, settings.ParameterSettings);
                panelAnalytical.SetParameters(roofBase, settings.ParameterSettings);
                panel.AddFragment(panelAnalytical);

                PanelContextFragment panelContext = new PanelContextFragment();
                panelContext.SetParameters(roofBase.RoofType, settings.ParameterSettings);
                panelContext.SetParameters(roofBase, settings.ParameterSettings);
                panel.AddFragment(panelContext);

                BuildingResultFragment buildingResults = new BuildingResultFragment();
                buildingResults.SetParameters(roofBase.RoofType, settings.ParameterSettings);
                buildingResults.SetParameters(roofBase, settings.ParameterSettings);
                panel.AddFragment(buildingResults);

                panel.Construction = construction;
                panel.Type = oM.Environment.Elements.PanelType.Roof;

                //Set identifiers, parameters & custom data
                panel.SetIdentifiers(roofBase);
                panel.SetCustomData(roofBase, settings.ParameterSettings);
                panel.SetParameters(roofBase, settings.ParameterSettings);
                panel.SetParameters(roofBase.RoofType, settings.ParameterSettings);

                refObjects.AddOrReplace(roofBase.Id, panel);

                panels.Add(panel);
            }

            panels = panels.UpdateBuildingElementTypeByCustomData();
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

            panels = new List<oM.Environment.Elements.Panel>();

            BH.oM.Physical.Constructions.Construction constrtuction = wall.WallType.ConstructionFromRevit(settings, refObjects);

            List<PolyCurve> polycurves = wall.Profiles(settings);
            List<PolyCurve> polycurveListOuter = polycurves.OuterPolyCurves();

            foreach (ICurve curve in polycurveListOuter)
            {
                //Create the BuildingElement
                oM.Environment.Elements.Panel panel = BH.Engine.Environment.Create.Panel(externalEdges: curve.ToEdges());
                panel.Name = wall.FamilyTypeFullName();

                //Set ExtendedProperties
                OriginContextFragment originContext = new OriginContextFragment() { ElementID = wall.Id.IntegerValue.ToString(), TypeName = wall.FamilyTypeFullName() };
                originContext.SetParameters(wall.WallType, settings.ParameterSettings);
                originContext.SetParameters(wall, settings.ParameterSettings);
                panel.AddFragment(originContext);

                PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
                panelAnalytical.SetParameters(wall.WallType, settings.ParameterSettings);
                panelAnalytical.SetParameters(wall, settings.ParameterSettings);
                panel.AddFragment(panelAnalytical);

                PanelContextFragment panelContext = new PanelContextFragment();
                panelContext.SetParameters(wall.WallType, settings.ParameterSettings);
                panelContext.SetParameters(wall, settings.ParameterSettings);
                panel.AddFragment(panelContext);

                BuildingResultFragment buildingResults = new BuildingResultFragment();
                buildingResults.SetParameters(wall.WallType, settings.ParameterSettings);
                buildingResults.SetParameters(wall, settings.ParameterSettings);
                panel.AddFragment(buildingResults);

                panel.Construction = constrtuction;
                panel.Type = oM.Environment.Elements.PanelType.Wall;

                //Set identifiers, parameters & custom data
                panel.SetIdentifiers(wall);
                panel.SetCustomData(wall, settings.ParameterSettings);
                panel.SetParameters(wall, settings.ParameterSettings);
                panel.SetParameters(wall.WallType, settings.ParameterSettings);

                refObjects.AddOrReplace(wall.Id, panel);

                panels.Add(panel);
            }

            panels = panels.UpdateBuildingElementTypeByCustomData();
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

            panel = BH.Engine.Environment.Create.Panel(externalEdges: crv.ToEdges());
            panel.Name = element.FamilyTypeFullName();

            //Set ExtendedProperties
            OriginContextFragment originContext = new OriginContextFragment() { ElementID = element.Id.IntegerValue.ToString(), TypeName = element.FamilyTypeFullName() };
            originContext.SetParameters(element, settings.ParameterSettings);
            originContext.SetParameters(elementType, settings.ParameterSettings);
            panel.Fragments.Add(originContext);

            PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
            panelAnalytical.SetParameters(element, settings.ParameterSettings);
            panelAnalytical.SetParameters(elementType, settings.ParameterSettings);
            panel.Fragments.Add(panelAnalytical);

            PanelContextFragment panelContext = new PanelContextFragment();
            panelContext.SetParameters(element, settings.ParameterSettings);
            panelContext.SetParameters(elementType, settings.ParameterSettings);
            panel.Fragments.Add(panelContext);

            BuildingResultFragment buildingResults = new BuildingResultFragment();
            buildingResults.SetParameters(element, settings.ParameterSettings);
            buildingResults.SetParameters(elementType, settings.ParameterSettings);
            panel.Fragments.Add(buildingResults);

            oM.Environment.Elements.PanelType? panelType = element.Category.PanelType();
            if (panelType.HasValue)
                panel.Type = panelType.Value;
            else
                panel.Type = oM.Environment.Elements.PanelType.Undefined;

            //Set identifiers, parameters & custom data
            panel.SetIdentifiers(element);
            panel.SetCustomData(element, settings.ParameterSettings);
            panel.SetParameters(element, settings.ParameterSettings);
            panel.SetParameters(elementType, settings.ParameterSettings);

            refObjects.AddOrReplace(element.Id, panel);

            panel = panel.UpdateBuildingElementTypeByCustomData();
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
            if (element != null)
            {
                elementType = element.Document.GetElement(element.GetTypeId()) as ElementType;
                panel = BH.Engine.Environment.Create.Panel(name: element.FamilyTypeFullName(), externalEdges: curve.ToEdges());
            }

            //Set ExtendedProperties
            OriginContextFragment originContext = new OriginContextFragment() { ElementID = element.Id.IntegerValue.ToString(), TypeName = element.FamilyTypeFullName() };
            originContext.SetParameters(element, settings.ParameterSettings);
            originContext.SetParameters(elementType, settings.ParameterSettings);
            panel.AddFragment(originContext);

            PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
            panelAnalytical.SetParameters(elementType, settings.ParameterSettings);
            panelAnalytical.SetParameters(element, settings.ParameterSettings);
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
            panelContext.SetParameters(elementType, settings.ParameterSettings);
            panelContext.SetParameters(element, settings.ParameterSettings);
            panel.AddFragment(panelContext);

            oM.Environment.Elements.PanelType? panelType = element.Category.PanelType();
            if (panelType.HasValue)
                panel.Type = panelType.Value;
            else
                panel.Type = oM.Environment.Elements.PanelType.Undefined;

            panel.Construction = Convert.ConstructionFromRevit(elementType as dynamic, settings, refObjects);

            //Set identifiers, parameters & custom data
            panel.SetIdentifiers(element);
            panel.SetCustomData(element, settings.ParameterSettings);
            panel.SetParameters(element, settings.ParameterSettings);
            panel.SetParameters(elementType, settings.ParameterSettings);

            panel.SetCustomData(elementType, BuiltInParameter.ALL_MODEL_FAMILY_NAME);

            if (elementType != null)
                panel.SetCustomData(elementType, settings.ParameterSettings, "Type ");

            refObjects.AddOrReplace(energyAnalysisSurface.Id, panel);

            panel = panel.UpdateBuildingElementTypeByCustomData();
            return panel;
        }

        /***************************************************/
    }
}
