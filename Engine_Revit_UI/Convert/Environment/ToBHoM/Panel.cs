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

        public static oM.Environment.Elements.Panel ToBHoMEnvironmentPanel(this Element element, ICurve crv, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            oM.Environment.Elements.Panel panel = refObjects.GetValue<oM.Environment.Elements.Panel>(element.Id);
            if (panel != null)
                return panel;

            ElementType elementType = element.Document.GetElement(element.GetTypeId()) as ElementType;

            panel = BH.Engine.Environment.Create.Panel(externalEdges: crv.ToEdges());
            panel.Name = element.FamilyTypeFullName();

            //Set ExtendedProperties
            OriginContextFragment originContext = new OriginContextFragment();
            originContext.ElementID = element.Id.IntegerValue.ToString();
            originContext.TypeName = element.FamilyTypeFullName();
            originContext.UpdateValues(settings, element);
            originContext.UpdateValues(settings, elementType);
            panel.Fragments.Add(originContext);

            PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
            panelAnalytical.UpdateValues(settings, element);
            panelAnalytical.UpdateValues(settings, elementType);
            panel.Fragments.Add(panelAnalytical);

            PanelContextFragment panelContext = new PanelContextFragment();
            panelContext.UpdateValues(settings, element);
            panelContext.UpdateValues(settings, elementType);
            panel.Fragments.Add(panelContext);

            BuildingResultFragment buildingResults = new BuildingResultFragment();
            buildingResults.UpdateValues(settings, element);
            buildingResults.UpdateValues(settings, elementType);
            panel.Fragments.Add(buildingResults);

            oM.Environment.Elements.PanelType? panelType = element.Category.PanelType();
            if (panelType.HasValue)
                panel.Type = panelType.Value;
            else
                panel.Type = oM.Environment.Elements.PanelType.Undefined;

            //Set identifiers & custom data
            panel.SetIdentifiers(element);
            panel.SetCustomData(element);

            refObjects.AddOrReplace(element.Id, panel);

            panel = panel.UpdateBuildingElementTypeByCustomData();
            panel.UpdateValues(settings, element);
            panel.UpdateValues(settings, elementType);
            return panel;
        }

        /***************************************************/

        public static oM.Environment.Elements.Panel ToBHoMEnvironmentPanel(this FamilyInstance familyInstance, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
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
            OriginContextFragment originContext = new OriginContextFragment();
            originContext.ElementID = familyInstance.Id.IntegerValue.ToString();
            originContext.TypeName = familyInstance.FamilyTypeFullName();
            originContext.UpdateValues(settings, familyInstance.Symbol);
            originContext.UpdateValues(settings, familyInstance);
            panel.Fragments.Add(originContext);

            PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
            panelAnalytical.UpdateValues(settings, familyInstance.Symbol);
            panelAnalytical.UpdateValues(settings, familyInstance);
            panel.Fragments.Add(panelAnalytical);

            PanelContextFragment panelContext = new PanelContextFragment();
            panelContext.UpdateValues(settings, familyInstance.Symbol);
            panelContext.UpdateValues(settings, familyInstance);
            panel.Fragments.Add(panelContext);

            BuildingResultFragment buildingResults = new BuildingResultFragment();
            buildingResults.UpdateValues(settings, familyInstance.Symbol);
            buildingResults.UpdateValues(settings, familyInstance);
            panel.Fragments.Add(buildingResults);

            oM.Environment.Elements.PanelType? panelType = familyInstance.Category.PanelType();
            if (panelType.HasValue)
                panel.Type = panelType.Value;
            else
                panel.Type = oM.Environment.Elements.PanelType.Undefined;

            //Set identifiers & custom data
            panel.SetIdentifiers(familyInstance);
            panel.SetCustomData(familyInstance);

            refObjects.AddOrReplace(familyInstance.Id, panel);

            panel = panel.UpdateBuildingElementTypeByCustomData();
            panel.UpdateValues(settings, familyInstance.Symbol);
            panel.UpdateValues(settings, familyInstance);
            return panel;
        }

        /***************************************************/

        public static oM.Environment.Elements.Panel ToBHoMEnvironmentPanel(this EnergyAnalysisSurface energyAnalysisSurface, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            oM.Environment.Elements.Panel panel = refObjects.GetValue<oM.Environment.Elements.Panel>(energyAnalysisSurface.Id);
            if (panel != null)
                return panel;

            //Get the geometry Curve
            ICurve curve = null;
            if (energyAnalysisSurface != null)
                curve = energyAnalysisSurface.GetPolyloop().ToBHoM();

            //Get the name and element type
            Element element = energyAnalysisSurface.Document.Element(energyAnalysisSurface.CADObjectUniqueId, energyAnalysisSurface.CADLinkUniqueId);
            ElementType elementType = null;
            if (element != null)
            {
                elementType = element.Document.GetElement(element.GetTypeId()) as ElementType;
                panel = BH.Engine.Environment.Create.Panel(name: element.FamilyTypeFullName(), externalEdges: curve.ToEdges());
            }

            //Set ExtendedProperties
            OriginContextFragment originContext = new OriginContextFragment();
            originContext.ElementID = element.Id.IntegerValue.ToString();
            originContext.TypeName = element.FamilyTypeFullName();
            originContext.UpdateValues(settings, element);
            originContext.UpdateValues(settings, elementType);
            panel.AddFragment(originContext);

            PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
            panelAnalytical.UpdateValues(settings, elementType);
            panelAnalytical.UpdateValues(settings, element);
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
            panelContext.UpdateValues(settings, elementType);
            panelContext.UpdateValues(settings, element);
            panel.AddFragment(panelContext);

            oM.Environment.Elements.PanelType? panelType = element.Category.PanelType();
            if (panelType.HasValue)
                panel.Type = panelType.Value;
            else
                panel.Type = oM.Environment.Elements.PanelType.Undefined;

            panel.Construction = Convert.ToBHoMConstruction(elementType as dynamic, settings, refObjects);

            //Set identifiers & custom data
            panel.SetIdentifiers(element);
            panel.SetCustomData(element);
            double heigh = energyAnalysisSurface.Height.ToSI(UnitType.UT_Length);
            double width = energyAnalysisSurface.Width.ToSI(UnitType.UT_Length);
            double azimuth = energyAnalysisSurface.Azimuth;

            panel.SetCustomData("Height", heigh);
            panel.SetCustomData("Width", width);
            panel.SetCustomData("Azimuth", azimuth);
            panel.SetCustomData(elementType, BuiltInParameter.ALL_MODEL_FAMILY_NAME);
            panel = panel.AddSpaceId(energyAnalysisSurface);
            panel = panel.AddAdjacentSpaceId(energyAnalysisSurface);

            if (elementType != null)
                panel.SetCustomData(elementType, "Type ");

            refObjects.AddOrReplace(energyAnalysisSurface.Id, panel);

            panel = panel.UpdateBuildingElementTypeByCustomData();
            panel.UpdateValues(settings, elementType);
            panel.UpdateValues(settings, element);
            return panel;
        }

        /***************************************************/

        public static List<oM.Environment.Elements.Panel> ToBHoMEnvironmentPanels(this Ceiling ceiling, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
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
            BH.oM.Physical.Constructions.Construction construction = ceilingType.ToBHoMConstruction(settings, refObjects);

            List<PolyCurve> polycurveListOuter = polycurves.OuterPolyCurves();
            foreach (ICurve curve in polycurveListOuter)
            {
                //Create the BuildingElement
                oM.Environment.Elements.Panel panel = BH.Engine.Environment.Create.Panel(externalEdges: curve.ToEdges());
                panel.Name = ceiling.FamilyTypeFullName();

                //Set ExtendedProperties
                OriginContextFragment originContext = new OriginContextFragment();
                originContext.ElementID = ceiling.Id.IntegerValue.ToString();
                originContext.TypeName = ceiling.FamilyTypeFullName();
                originContext.UpdateValues(settings, ceilingType);
                originContext.UpdateValues(settings, ceiling);
                panel.AddFragment(originContext);

                PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
                panelAnalytical.UpdateValues(settings, ceilingType);
                panelAnalytical.UpdateValues(settings, ceiling);
                panel.AddFragment(panelAnalytical);

                PanelContextFragment panelContext = new PanelContextFragment();
                panelContext.UpdateValues(settings, ceilingType);
                panelContext.UpdateValues(settings, ceiling);
                panel.AddFragment(panelContext);

                BuildingResultFragment buildingResults = new BuildingResultFragment();
                buildingResults.UpdateValues(settings, ceilingType);
                buildingResults.UpdateValues(settings, ceiling);
                panel.AddFragment(buildingResults);

                panel.Construction = construction;
                panel.Type = oM.Environment.Elements.PanelType.Ceiling;

                //Set identifiers & custom data
                panel.SetIdentifiers(ceiling);
                panel.SetCustomData(ceiling);

                refObjects.AddOrReplace(ceiling.Id, panel);
                panel.UpdateValues(settings, ceilingType);
                panel.UpdateValues(settings, ceiling);
                panels.Add(panel);
            }

            panels = panels.UpdateBuildingElementTypeByCustomData();
            return panels;
        }

        /***************************************************/

        public static List<oM.Environment.Elements.Panel> ToBHoMEnvironmentPanels(this Floor floor, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> panels = refObjects.GetValues<oM.Environment.Elements.Panel>(floor.Id);
            if (panels != null && panels.Count != 0)
                return panels;

            List<PolyCurve> polycurves = floor.Profiles(settings);
            if (polycurves == null)
                return panels;

            panels = new List<oM.Environment.Elements.Panel>();

            BH.oM.Physical.Constructions.Construction construction = floor.FloorType.ToBHoMConstruction(settings, refObjects);

            FloorType floorType = floor.Document.GetElement(floor.GetTypeId()) as FloorType;

            List<PolyCurve> polycurveListOuter = polycurves.OuterPolyCurves();
            foreach (ICurve curve in polycurveListOuter)
            {
                //Create the BuildingElement
                oM.Environment.Elements.Panel panel = BH.Engine.Environment.Create.Panel(externalEdges: curve.ToEdges());
                panel.Name = floor.FamilyTypeFullName();

                //Set ExtendedProperties
                OriginContextFragment originContext = new OriginContextFragment();
                originContext.ElementID = floor.Id.IntegerValue.ToString();
                originContext.TypeName = floor.FamilyTypeFullName();
                originContext.UpdateValues(settings, floorType);
                originContext.UpdateValues(settings, floor);
                panel.AddFragment(originContext);

                PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
                panelAnalytical.UpdateValues(settings, floorType);
                panelAnalytical.UpdateValues(settings, floor);
                panel.AddFragment(panelAnalytical);

                PanelContextFragment panelContext = new PanelContextFragment();
                panelContext.UpdateValues(settings, floorType);
                panelContext.UpdateValues(settings, floor);
                panel.AddFragment(panelContext);

                BuildingResultFragment buildingResults = new BuildingResultFragment();
                buildingResults.UpdateValues(settings, floorType);
                buildingResults.UpdateValues(settings, floor);
                panel.AddFragment(buildingResults);

                panel.Construction = construction;
                panel.Type = oM.Environment.Elements.PanelType.Floor;

                //Set identifiers & custom data
                panel.SetIdentifiers(floor);
                panel.SetCustomData(floor);

                refObjects.AddOrReplace(floor.Id, panel);

                panel.UpdateValues(settings, floorType);
                panel.UpdateValues(settings, floor);
                panels.Add(panel);
            }

            panels = panels.UpdateBuildingElementTypeByCustomData();
            return panels;
        }

        /***************************************************/

        public static List<oM.Environment.Elements.Panel> ToBHoMEnvironmentPanels(this RoofBase roofBase, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> panels = refObjects.GetValues<oM.Environment.Elements.Panel>(roofBase.Id);
            if (panels != null && panels.Count > 0)
                return panels;

            List<PolyCurve> polycurves = roofBase.Profiles(settings);
            if (polycurves == null)
                return panels;

            panels = new List<oM.Environment.Elements.Panel>();

            BH.oM.Physical.Constructions.Construction construction = roofBase.RoofType.ToBHoMConstruction(settings, refObjects);

            List<PolyCurve> polycurvesListOuter = polycurves.OuterPolyCurves();
            foreach (ICurve curve in polycurvesListOuter)
            {
                //Create the BuildingElement
                oM.Environment.Elements.Panel panel = BH.Engine.Environment.Create.Panel(externalEdges: curve.ToEdges());
                panel.Name = roofBase.FamilyTypeFullName();

                //Set ExtendedProperties
                OriginContextFragment originContext = new OriginContextFragment();
                originContext.ElementID = roofBase.Id.IntegerValue.ToString();
                originContext.TypeName = roofBase.FamilyTypeFullName();
                originContext.UpdateValues(settings, roofBase.RoofType);
                originContext.UpdateValues(settings, roofBase);
                panel.AddFragment(originContext);

                PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
                panelAnalytical.UpdateValues(settings, roofBase.RoofType);
                panelAnalytical.UpdateValues(settings, roofBase);
                panel.AddFragment(panelAnalytical);

                PanelContextFragment panelContext = new PanelContextFragment();
                panelContext.UpdateValues(settings, roofBase.RoofType);
                panelContext.UpdateValues(settings, roofBase);
                panel.AddFragment(panelContext);

                BuildingResultFragment buildingResults = new BuildingResultFragment();
                buildingResults.UpdateValues(settings, roofBase.RoofType);
                buildingResults.UpdateValues(settings, roofBase);
                panel.AddFragment(buildingResults);

                panel.Construction = construction;
                panel.Type = oM.Environment.Elements.PanelType.Roof;

                //Set identifiers & custom data
                panel.SetIdentifiers(roofBase);
                panel.SetCustomData(roofBase);

                refObjects.AddOrReplace(roofBase.Id, panel);

                panel.UpdateValues(settings, roofBase.RoofType);
                panel.UpdateValues(settings, roofBase);
                panels.Add(panel);
            }

            panels = panels.UpdateBuildingElementTypeByCustomData();
            return panels;
        }

        /***************************************************/

        public static List<oM.Environment.Elements.Panel> ToBHoMEnvironmentPanels(this Wall wall, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> panels = refObjects.GetValues<oM.Environment.Elements.Panel>(wall.Id);
            if (panels != null && panels.Count > 0)
                return panels;

            panels = new List<oM.Environment.Elements.Panel>();

            BH.oM.Physical.Constructions.Construction constrtuction = wall.WallType.ToBHoMConstruction(settings, refObjects);

            List<PolyCurve> polycurves = wall.Profiles(settings);
            List<PolyCurve> polycurveListOuter = polycurves.OuterPolyCurves();

            foreach (ICurve curve in polycurveListOuter)
            {
                //Create the BuildingElement
                oM.Environment.Elements.Panel panel = BH.Engine.Environment.Create.Panel(externalEdges: curve.ToEdges());
                panel.Name = wall.FamilyTypeFullName();

                //Set ExtendedProperties
                OriginContextFragment originContext = new OriginContextFragment();
                originContext.ElementID = wall.Id.IntegerValue.ToString();
                originContext.TypeName = wall.FamilyTypeFullName();
                originContext.UpdateValues(settings, wall.WallType);
                originContext.UpdateValues(settings, wall);
                panel.AddFragment(originContext);

                PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
                panelAnalytical.UpdateValues(settings, wall.WallType);
                panelAnalytical.UpdateValues(settings, wall);
                panel.AddFragment(panelAnalytical);

                PanelContextFragment panelContext = new PanelContextFragment();
                panelContext.UpdateValues(settings, wall.WallType);
                panelContext.UpdateValues(settings, wall);
                panel.AddFragment(panelContext);

                BuildingResultFragment buildingResults = new BuildingResultFragment();
                buildingResults.UpdateValues(settings, wall.WallType);
                buildingResults.UpdateValues(settings, wall);
                panel.AddFragment(buildingResults);

                panel.Construction = constrtuction;
                panel.Type = oM.Environment.Elements.PanelType.Wall;

                //Set identifiers & custom data
                panel.SetIdentifiers(wall);
                panel.SetCustomData(wall);

                refObjects.AddOrReplace(wall.Id, panel);

                panel.UpdateValues(settings, wall.WallType);
                panel.UpdateValues(settings, wall);
                panels.Add(panel);
            }

            panels = panels.UpdateBuildingElementTypeByCustomData();
            return panels;
        }

        /***************************************************/
    }
}
