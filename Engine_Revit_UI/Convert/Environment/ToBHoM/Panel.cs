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

using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

using BH.Engine.Environment;
using BH.oM.Environment.Elements;
using BH.oM.Environment.Fragments;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static oM.Environment.Elements.Panel ToBHoMEnvironmentPanel(this Element element, ICurve crv, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            oM.Environment.Elements.Panel panel = pullSettings.FindRefObject<oM.Environment.Elements.Panel>(element.Id.IntegerValue);
            if (panel != null)
                return panel;

            ElementType elementType = element.Document.GetElement(element.GetTypeId()) as ElementType;

            panel = BH.Engine.Environment.Create.Panel(externalEdges: crv.ToEdges());
            panel.Name = Query.FamilyTypeFullName(element);

            //Set ExtendedProperties
            OriginContextFragment originContext = new OriginContextFragment();
            originContext.ElementID = element.Id.IntegerValue.ToString();
            originContext.TypeName = Query.FamilyTypeFullName(element);
            originContext = originContext.UpdateValues(pullSettings, element) as OriginContextFragment;
            originContext = originContext.UpdateValues(pullSettings, elementType) as OriginContextFragment;
            panel.Fragments.Add(originContext);

            PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
            panelAnalytical = panelAnalytical.UpdateValues(pullSettings, element) as PanelAnalyticalFragment;
            panelAnalytical = panelAnalytical.UpdateValues(pullSettings, elementType) as PanelAnalyticalFragment;
            panel.Fragments.Add(panelAnalytical);

            PanelContextFragment panelContext = new PanelContextFragment();
            panelContext = panelContext.UpdateValues(pullSettings, element) as PanelContextFragment;
            panelContext = panelContext.UpdateValues(pullSettings, elementType) as PanelContextFragment;
            panel.Fragments.Add(panelContext);

            BuildingResultFragment buildingResults = new BuildingResultFragment();
            buildingResults = buildingResults.UpdateValues(pullSettings, element) as BuildingResultFragment;
            buildingResults = buildingResults.UpdateValues(pullSettings, elementType) as BuildingResultFragment;
            panel.Fragments.Add(buildingResults);

            oM.Environment.Elements.PanelType? panelType = Query.PanelType(element.Category);
            if (panelType.HasValue)
                panel.Type = panelType.Value;
            else
                panel.Type = oM.Environment.Elements.PanelType.Undefined;

            panel = Modify.SetIdentifiers(panel, element) as oM.Environment.Elements.Panel;
            if (pullSettings.CopyCustomData)
                panel = Modify.SetCustomData(panel, element) as oM.Environment.Elements.Panel;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(panel);

            panel = panel.UpdateBuildingElementTypeByCustomData();
            panel = panel.UpdateValues(pullSettings, element) as oM.Environment.Elements.Panel;
            panel = panel.UpdateValues(pullSettings, elementType) as oM.Environment.Elements.Panel;
            return panel;
        }

        /***************************************************/

        public static oM.Environment.Elements.Panel ToBHoMEnvironmentPanel(this FamilyInstance familyInstance, PullSettings pullSettings = null)
        {
            //Create a BuildingElement from the familyInstance geometry
            pullSettings = pullSettings.DefaultIfNull();

            oM.Environment.Elements.Panel panel = pullSettings.FindRefObject<oM.Environment.Elements.Panel>(familyInstance.Id.IntegerValue);
            if (panel != null)
                return panel;

            PolyCurve polycurve = Query.PolyCurve(familyInstance, pullSettings);
            if (polycurve == null)
                return null;

            panel = BH.Engine.Environment.Create.Panel(externalEdges: polycurve.ToEdges());
            panel.Name = Query.FamilyTypeFullName(familyInstance);

            //Set ExtendedProperties
            OriginContextFragment originContext = new OriginContextFragment();
            originContext.ElementID = familyInstance.Id.IntegerValue.ToString();
            originContext.TypeName = Query.FamilyTypeFullName(familyInstance);
            originContext = originContext.UpdateValues(pullSettings, familyInstance.Symbol) as OriginContextFragment;
            originContext = originContext.UpdateValues(pullSettings, familyInstance) as OriginContextFragment;
            panel.Fragments.Add(originContext);

            PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
            panelAnalytical = panelAnalytical.UpdateValues(pullSettings, familyInstance.Symbol) as PanelAnalyticalFragment;
            panelAnalytical = panelAnalytical.UpdateValues(pullSettings, familyInstance) as PanelAnalyticalFragment;
            panel.Fragments.Add(panelAnalytical);

            PanelContextFragment panelContext = new PanelContextFragment();
            panelContext = panelContext.UpdateValues(pullSettings, familyInstance.Symbol) as PanelContextFragment;
            panelContext = panelContext.UpdateValues(pullSettings, familyInstance) as PanelContextFragment;
            panel.Fragments.Add(panelContext);

            BuildingResultFragment buildingResults = new BuildingResultFragment();
            buildingResults = buildingResults.UpdateValues(pullSettings, familyInstance.Symbol) as BuildingResultFragment;
            buildingResults = buildingResults.UpdateValues(pullSettings, familyInstance) as BuildingResultFragment;
            panel.Fragments.Add(buildingResults);

            oM.Environment.Elements.PanelType? panelType = Query.PanelType(familyInstance.Category);
            if (panelType.HasValue)
                panel.Type = panelType.Value;
            else
                panel.Type = oM.Environment.Elements.PanelType.Undefined;

            panel = Modify.SetIdentifiers(panel, familyInstance) as oM.Environment.Elements.Panel;
            if (pullSettings.CopyCustomData)
                panel = Modify.SetCustomData(panel, familyInstance) as oM.Environment.Elements.Panel;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(panel);

            panel = panel.UpdateBuildingElementTypeByCustomData();
            panel = panel.UpdateValues(pullSettings, familyInstance.Symbol) as oM.Environment.Elements.Panel;
            panel = panel.UpdateValues(pullSettings, familyInstance) as oM.Environment.Elements.Panel;
            return panel;
        }

        /***************************************************/

        public static oM.Environment.Elements.Panel ToBHoMEnvironmentPanel(this EnergyAnalysisSurface energyAnalysisSurface, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            oM.Environment.Elements.Panel panel = pullSettings.FindRefObject<oM.Environment.Elements.Panel>(energyAnalysisSurface.Id.IntegerValue);
            if (panel != null)
                return panel;

            //Get the geometry Curve
            ICurve curve = null;
            if (energyAnalysisSurface != null)
                curve = energyAnalysisSurface.GetPolyloop().ToBHoM();

            //Get the name and element type
            Element element = Query.Element(energyAnalysisSurface.Document, energyAnalysisSurface.CADObjectUniqueId, energyAnalysisSurface.CADLinkUniqueId);
            ElementType elementType = null;
            if (element != null)
            {
                elementType = element.Document.GetElement(element.GetTypeId()) as ElementType;
                panel = BH.Engine.Environment.Create.Panel(name: Query.FamilyTypeFullName(element), externalEdges: curve.ToEdges());
            }

            //Set ExtendedProperties
            OriginContextFragment originContext = new OriginContextFragment();
            originContext.ElementID = element.Id.IntegerValue.ToString();
            originContext.TypeName = Query.FamilyTypeFullName(element);
            originContext = originContext.UpdateValues(pullSettings, element) as OriginContextFragment;
            originContext = originContext.UpdateValues(pullSettings, elementType) as OriginContextFragment;
            panel.AddFragment(originContext);

            PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
            panelAnalytical = panelAnalytical.UpdateValues(pullSettings, elementType) as PanelAnalyticalFragment;
            panelAnalytical = panelAnalytical.UpdateValues(pullSettings, element) as PanelAnalyticalFragment;
            panel.AddFragment(panelAnalytical);

            PanelContextFragment panelContext = new PanelContextFragment();

            List<string> connectedSpaces = new List<string>();
            EnergyAnalysisSpace energyAnalysisSpace = null;
            energyAnalysisSpace = energyAnalysisSurface.GetAnalyticalSpace();
            if (energyAnalysisSpace != null)
            {
                SpatialElement spatialElement = Query.Element(energyAnalysisSpace.Document, energyAnalysisSpace.CADObjectUniqueId) as SpatialElement;
                connectedSpaces.Add(Query.Name(spatialElement));
            }

            energyAnalysisSpace = energyAnalysisSurface.GetAdjacentAnalyticalSpace();
            if (energyAnalysisSpace != null)
            {
                SpatialElement spatialElement = Query.Element(energyAnalysisSpace.Document, energyAnalysisSpace.CADObjectUniqueId) as SpatialElement;
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
            panelContext = panelContext.UpdateValues(pullSettings, elementType) as PanelContextFragment;
            panelContext = panelContext.UpdateValues(pullSettings, element) as PanelContextFragment;
            panel.AddFragment(panelContext);

            oM.Environment.Elements.PanelType? panelType = Query.PanelType(element.Category);
            if (panelType.HasValue)
                panel.Type = panelType.Value;
            else
                panel.Type = oM.Environment.Elements.PanelType.Undefined;

            panel.Construction = Convert.ToBHoMConstruction(elementType as dynamic, pullSettings);

            //Set some custom data properties
            panel = Modify.SetIdentifiers(panel, element) as oM.Environment.Elements.Panel;
            if (pullSettings.CopyCustomData)
            {
                panel = Modify.SetCustomData(panel, element) as oM.Environment.Elements.Panel;
                double heigh = energyAnalysisSurface.Height.ToSI(UnitType.UT_Length);
                double width = energyAnalysisSurface.Width.ToSI(UnitType.UT_Length);
                double azimuth = energyAnalysisSurface.Azimuth;

                panel = Modify.SetCustomData(panel, "Height", heigh) as oM.Environment.Elements.Panel;
                panel = Modify.SetCustomData(panel, "Width", width) as oM.Environment.Elements.Panel;
                panel = Modify.SetCustomData(panel, "Azimuth", azimuth) as oM.Environment.Elements.Panel;
                panel = Modify.SetCustomData(panel, elementType, BuiltInParameter.ALL_MODEL_FAMILY_NAME) as oM.Environment.Elements.Panel;
                panel = Modify.AddSpaceId(panel, energyAnalysisSurface);
                panel = Modify.AddAdjacentSpaceId(panel, energyAnalysisSurface);

                if (elementType != null)
                    panel = Modify.SetCustomData(panel, elementType, "Type ") as oM.Environment.Elements.Panel;
            }

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(panel);

            panel = panel.UpdateBuildingElementTypeByCustomData();
            panel = panel.UpdateValues(pullSettings, elementType) as oM.Environment.Elements.Panel;
            panel = panel.UpdateValues(pullSettings, element) as oM.Environment.Elements.Panel;
            return panel;
        }

        /***************************************************/

        public static List<oM.Environment.Elements.Panel> ToBHoMEnvironmentPanels(this Ceiling ceiling, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> panels = pullSettings.FindRefObjects<oM.Environment.Elements.Panel>(ceiling.Id.IntegerValue);
            if (panels != null && panels.Count > 0)
                return panels;

            List<PolyCurve> polycurves = Query.Profiles(ceiling, pullSettings);
            if (polycurves == null)
                return panels;

            panels = new List<oM.Environment.Elements.Panel>();

            CeilingType ceilingType = ceiling.Document.GetElement(ceiling.GetTypeId()) as CeilingType;
            BH.oM.Physical.Constructions.Construction construction = ToBHoMConstruction(ceilingType, pullSettings);

            List<PolyCurve> polycurveListOuter = BH.Engine.Adapters.Revit.Query.OuterPolyCurves(polycurves);
            foreach (ICurve curve in polycurveListOuter)
            {
                //Create the BuildingElement
                oM.Environment.Elements.Panel panel = BH.Engine.Environment.Create.Panel(externalEdges: curve.ToEdges());
                panel.Name = Query.FamilyTypeFullName(ceiling);

                //Set ExtendedProperties
                OriginContextFragment originContext = new OriginContextFragment();
                originContext.ElementID = ceiling.Id.IntegerValue.ToString();
                originContext.TypeName = Query.FamilyTypeFullName(ceiling);
                originContext = originContext.UpdateValues(pullSettings, ceilingType) as OriginContextFragment;
                originContext = originContext.UpdateValues(pullSettings, ceiling) as OriginContextFragment;
                panel.AddFragment(originContext);

                PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
                panelAnalytical = panelAnalytical.UpdateValues(pullSettings, ceilingType) as PanelAnalyticalFragment;
                panelAnalytical = panelAnalytical.UpdateValues(pullSettings, ceiling) as PanelAnalyticalFragment;
                panel.AddFragment(panelAnalytical);

                PanelContextFragment panelContext = new PanelContextFragment();
                panelContext = panelContext.UpdateValues(pullSettings, ceilingType) as PanelContextFragment;
                panelContext = panelContext.UpdateValues(pullSettings, ceiling) as PanelContextFragment;
                panel.AddFragment(panelContext);

                BuildingResultFragment buildingResults = new BuildingResultFragment();
                buildingResults = buildingResults.UpdateValues(pullSettings, ceilingType) as BuildingResultFragment;
                buildingResults = buildingResults.UpdateValues(pullSettings, ceiling) as BuildingResultFragment;
                panel.AddFragment(buildingResults);

                panel.Construction = construction;
                panel.Type = oM.Environment.Elements.PanelType.Ceiling;

                //Assign custom data
                panel = Modify.SetIdentifiers(panel, ceiling) as oM.Environment.Elements.Panel;
                if (pullSettings.CopyCustomData)
                    panel = Modify.SetCustomData(panel, ceiling) as oM.Environment.Elements.Panel;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(panel);
                panel = panel.UpdateValues(pullSettings, ceilingType) as oM.Environment.Elements.Panel;
                panel = panel.UpdateValues(pullSettings, ceiling) as oM.Environment.Elements.Panel;
                panels.Add(panel);
            }

            panels = panels.UpdateBuildingElementTypeByCustomData();
            return panels;
        }

        /***************************************************/

        public static List<oM.Environment.Elements.Panel> ToBHoMEnvironmentPanels(this Floor floor, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> panels = pullSettings.FindRefObjects<oM.Environment.Elements.Panel>(floor.Id.IntegerValue);
            if (panels != null && panels.Count > 0)
                return panels;

            List<PolyCurve> polycurves = Query.Profiles(floor, pullSettings);
            if (polycurves == null)
                return panels;

            panels = new List<oM.Environment.Elements.Panel>();

            BH.oM.Physical.Constructions.Construction construction = ToBHoMConstruction(floor.FloorType, pullSettings);

            FloorType floorType = floor.Document.GetElement(floor.GetTypeId()) as FloorType;

            List<PolyCurve> polycurveListOuter = BH.Engine.Adapters.Revit.Query.OuterPolyCurves(polycurves);
            foreach (ICurve curve in polycurveListOuter)
            {
                //Create the BuildingElement
                oM.Environment.Elements.Panel panel = BH.Engine.Environment.Create.Panel(externalEdges: curve.ToEdges());
                panel.Name = Query.FamilyTypeFullName(floor);

                //Set ExtendedProperties
                OriginContextFragment originContext = new OriginContextFragment();
                originContext.ElementID = floor.Id.IntegerValue.ToString();
                originContext.TypeName = Query.FamilyTypeFullName(floor);
                originContext = originContext.UpdateValues(pullSettings, floorType) as OriginContextFragment;
                originContext = originContext.UpdateValues(pullSettings, floor) as OriginContextFragment;
                panel.AddFragment(originContext);

                PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
                panelAnalytical = panelAnalytical.UpdateValues(pullSettings, floorType) as PanelAnalyticalFragment;
                panelAnalytical = panelAnalytical.UpdateValues(pullSettings, floor) as PanelAnalyticalFragment;
                panel.AddFragment(panelAnalytical);

                PanelContextFragment panelContext = new PanelContextFragment();
                panelContext = panelContext.UpdateValues(pullSettings, floorType) as PanelContextFragment;
                panelContext = panelContext.UpdateValues(pullSettings, floor) as PanelContextFragment;
                panel.AddFragment(panelContext);

                BuildingResultFragment buildingResults = new BuildingResultFragment();
                buildingResults = buildingResults.UpdateValues(pullSettings, floorType) as BuildingResultFragment;
                buildingResults = buildingResults.UpdateValues(pullSettings, floor) as BuildingResultFragment;
                panel.AddFragment(buildingResults);

                panel.Construction = construction;
                panel.Type = oM.Environment.Elements.PanelType.Floor;

                //Assign custom data
                panel = Modify.SetIdentifiers(panel, floor) as oM.Environment.Elements.Panel;
                if (pullSettings.CopyCustomData)
                    panel = Modify.SetCustomData(panel, floor) as oM.Environment.Elements.Panel;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(panel);

                panel = panel.UpdateValues(pullSettings, floorType) as oM.Environment.Elements.Panel;
                panel = panel.UpdateValues(pullSettings, floor) as oM.Environment.Elements.Panel;
                panels.Add(panel);
            }

            panels = panels.UpdateBuildingElementTypeByCustomData();
            return panels;
        }

        /***************************************************/

        public static List<oM.Environment.Elements.Panel> ToBHoMEnvironmentPanels(this RoofBase roofBase, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> panels = pullSettings.FindRefObjects<oM.Environment.Elements.Panel>(roofBase.Id.IntegerValue);
            if (panels != null && panels.Count > 0)
                return panels;

            List<PolyCurve> polycurves = Query.Profiles(roofBase, pullSettings);
            if (polycurves == null)
                return panels;

            panels = new List<oM.Environment.Elements.Panel>();

            BH.oM.Physical.Constructions.Construction construction = ToBHoMConstruction(roofBase.RoofType, pullSettings);

            List<PolyCurve> polycurvesListOuter = BH.Engine.Adapters.Revit.Query.OuterPolyCurves(polycurves);
            foreach (ICurve curve in polycurvesListOuter)
            {
                //Create the BuildingElement
                oM.Environment.Elements.Panel panel = BH.Engine.Environment.Create.Panel(externalEdges: curve.ToEdges());
                panel.Name = Query.FamilyTypeFullName(roofBase);

                //Set ExtendedProperties
                OriginContextFragment originContext = new OriginContextFragment();
                originContext.ElementID = roofBase.Id.IntegerValue.ToString();
                originContext.TypeName = Query.FamilyTypeFullName(roofBase);
                originContext = originContext.UpdateValues(pullSettings, roofBase.RoofType) as OriginContextFragment;
                originContext = originContext.UpdateValues(pullSettings, roofBase) as OriginContextFragment;
                panel.AddFragment(originContext);

                PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
                panelAnalytical = panelAnalytical.UpdateValues(pullSettings, roofBase.RoofType) as PanelAnalyticalFragment;
                panelAnalytical = panelAnalytical.UpdateValues(pullSettings, roofBase) as PanelAnalyticalFragment;
                panel.AddFragment(panelAnalytical);

                PanelContextFragment panelContext = new PanelContextFragment();
                panelContext = panelContext.UpdateValues(pullSettings, roofBase.RoofType) as PanelContextFragment;
                panelContext = panelContext.UpdateValues(pullSettings, roofBase) as PanelContextFragment;
                panel.AddFragment(panelContext);

                BuildingResultFragment buildingResults = new BuildingResultFragment();
                buildingResults = buildingResults.UpdateValues(pullSettings, roofBase.RoofType) as BuildingResultFragment;
                buildingResults = buildingResults.UpdateValues(pullSettings, roofBase) as BuildingResultFragment;
                panel.AddFragment(buildingResults);

                panel.Construction = construction;
                panel.Type = oM.Environment.Elements.PanelType.Roof;

                //Assign custom data
                panel = Modify.SetIdentifiers(panel, roofBase) as oM.Environment.Elements.Panel;
                if (pullSettings.CopyCustomData)
                    panel = Modify.SetCustomData(panel, roofBase) as oM.Environment.Elements.Panel;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(panel);

                panel = panel.UpdateValues(pullSettings, roofBase.RoofType) as oM.Environment.Elements.Panel;
                panel = panel.UpdateValues(pullSettings, roofBase) as oM.Environment.Elements.Panel;
                panels.Add(panel);
            }

            panels = panels.UpdateBuildingElementTypeByCustomData();
            return panels;
        }

        /***************************************************/

        public static List<oM.Environment.Elements.Panel> ToBHoMEnvironmentPanels(this Wall wall, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> panels = pullSettings.FindRefObjects<oM.Environment.Elements.Panel>(wall.Id.IntegerValue);
            if (panels != null && panels.Count > 0)
                return panels;

            panels = new List<oM.Environment.Elements.Panel>();

            BH.oM.Physical.Constructions.Construction constrtuction = ToBHoMConstruction(wall.WallType, pullSettings);

            List<PolyCurve> polycurves = Query.Profiles(wall, pullSettings);
            List<PolyCurve> polycurveListOuter = BH.Engine.Adapters.Revit.Query.OuterPolyCurves(polycurves);

            foreach (ICurve curve in polycurveListOuter)
            {
                //Create the BuildingElement
                oM.Environment.Elements.Panel panel = BH.Engine.Environment.Create.Panel(externalEdges: curve.ToEdges());
                panel.Name = Query.FamilyTypeFullName(wall);

                //Set ExtendedProperties
                OriginContextFragment originContext = new OriginContextFragment();
                originContext.ElementID = wall.Id.IntegerValue.ToString();
                originContext.TypeName = Query.FamilyTypeFullName(wall);
                originContext = originContext.UpdateValues(pullSettings, wall.WallType) as OriginContextFragment;
                originContext = originContext.UpdateValues(pullSettings, wall) as OriginContextFragment;
                panel.AddFragment(originContext);

                PanelAnalyticalFragment panelAnalytical = new PanelAnalyticalFragment();
                panelAnalytical = panelAnalytical.UpdateValues(pullSettings, wall.WallType) as PanelAnalyticalFragment;
                panelAnalytical = panelAnalytical.UpdateValues(pullSettings, wall) as PanelAnalyticalFragment;
                panel.AddFragment(panelAnalytical);

                PanelContextFragment panelContext = new PanelContextFragment();
                panelContext = panelContext.UpdateValues(pullSettings, wall.WallType) as PanelContextFragment;
                panelContext = panelContext.UpdateValues(pullSettings, wall) as PanelContextFragment;
                panel.AddFragment(panelContext);

                BuildingResultFragment buildingResults = new BuildingResultFragment();
                buildingResults = buildingResults.UpdateValues(pullSettings, wall.WallType) as BuildingResultFragment;
                buildingResults = buildingResults.UpdateValues(pullSettings, wall) as BuildingResultFragment;
                panel.AddFragment(buildingResults);

                panel.Construction = constrtuction;
                panel.Type = oM.Environment.Elements.PanelType.Wall;

                //Assign custom data
                panel = Modify.SetIdentifiers(panel, wall) as oM.Environment.Elements.Panel;
                if (pullSettings.CopyCustomData)
                    panel = Modify.SetCustomData(panel, wall) as oM.Environment.Elements.Panel;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(panel);

                panel = panel.UpdateValues(pullSettings, wall.WallType) as oM.Environment.Elements.Panel;
                panel = panel.UpdateValues(pullSettings, wall) as oM.Environment.Elements.Panel;
                panels.Add(panel);
            }

            panels = panels.UpdateBuildingElementTypeByCustomData();
            return panels;
        }

        /***************************************************/
    }
}
