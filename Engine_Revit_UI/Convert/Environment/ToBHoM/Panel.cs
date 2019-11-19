/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

            oM.Environment.Elements.Panel aPanel = pullSettings.FindRefObject<oM.Environment.Elements.Panel>(element.Id.IntegerValue);
            if (aPanel != null)
                return aPanel;

            ElementType aElementType = element.Document.GetElement(element.GetTypeId()) as ElementType;

            aPanel = Create.Panel(externalEdges: crv.ToEdges());
            aPanel.Name = Query.FamilyTypeFullName(element);

            //Set ExtendedProperties
            OriginContextFragment aOriginContextFragment = new OriginContextFragment();
            aOriginContextFragment.ElementID = element.Id.IntegerValue.ToString();
            aOriginContextFragment.TypeName = Query.FamilyTypeFullName(element);
            aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, element) as OriginContextFragment;
            aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, aElementType) as OriginContextFragment;
            aPanel.Fragments.Add(aOriginContextFragment);

            PanelAnalyticalFragment aBuildingElementAnalyticalProperties = new PanelAnalyticalFragment();
            aBuildingElementAnalyticalProperties = aBuildingElementAnalyticalProperties.UpdateValues(pullSettings, element) as PanelAnalyticalFragment;
            aBuildingElementAnalyticalProperties = aBuildingElementAnalyticalProperties.UpdateValues(pullSettings, aElementType) as PanelAnalyticalFragment;
            aPanel.Fragments.Add(aBuildingElementAnalyticalProperties);

            PanelContextFragment aBuildingElementContextProperties = new PanelContextFragment();
            aBuildingElementContextProperties = aBuildingElementContextProperties.UpdateValues(pullSettings, element) as PanelContextFragment;
            aBuildingElementContextProperties = aBuildingElementContextProperties.UpdateValues(pullSettings, aElementType) as PanelContextFragment;
            aPanel.Fragments.Add(aBuildingElementContextProperties);

            BuildingResultFragment aBuildingResultsProperties = new BuildingResultFragment();
            aBuildingResultsProperties = aBuildingResultsProperties.UpdateValues(pullSettings, element) as BuildingResultFragment;
            aBuildingResultsProperties = aBuildingResultsProperties.UpdateValues(pullSettings, aElementType) as BuildingResultFragment;
            aPanel.Fragments.Add(aBuildingResultsProperties);

            oM.Environment.Elements.PanelType? aBuildingElementType = Query.PanelType(element.Category);
            if (aBuildingElementType.HasValue)
                aPanel.Type = aBuildingElementType.Value;
            else
                aPanel.Type = oM.Environment.Elements.PanelType.Undefined;

            aPanel = Modify.SetIdentifiers(aPanel, element) as oM.Environment.Elements.Panel;
            if (pullSettings.CopyCustomData)
                aPanel = Modify.SetCustomData(aPanel, element, pullSettings.ConvertUnits) as oM.Environment.Elements.Panel;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aPanel);

            aPanel = aPanel.UpdateBuildingElementTypeByCustomData();
            aPanel = aPanel.UpdateValues(pullSettings, element) as oM.Environment.Elements.Panel;
            aPanel = aPanel.UpdateValues(pullSettings, aElementType) as oM.Environment.Elements.Panel;
            return aPanel;
        }

        /***************************************************/

        public static oM.Environment.Elements.Panel ToBHoMEnvironmentPanel(this FamilyInstance familyInstance, PullSettings pullSettings = null)
        {
            //Create a BuildingElement from the familyInstance geometry
            pullSettings = pullSettings.DefaultIfNull();

            oM.Environment.Elements.Panel aPanel = pullSettings.FindRefObject<oM.Environment.Elements.Panel>(familyInstance.Id.IntegerValue);
            if (aPanel != null)
                return aPanel;

            PolyCurve aPolyCurve = Query.PolyCurve(familyInstance, pullSettings);
            if (aPolyCurve == null)
                return null;

            aPanel = Create.Panel(externalEdges: aPolyCurve.ToEdges());
            aPanel.Name = Query.FamilyTypeFullName(familyInstance);

            //Set ExtendedProperties
            OriginContextFragment aOriginContextFragment = new OriginContextFragment();
            aOriginContextFragment.ElementID = familyInstance.Id.IntegerValue.ToString();
            aOriginContextFragment.TypeName = Query.FamilyTypeFullName(familyInstance);
            aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, familyInstance.Symbol) as OriginContextFragment;
            aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, familyInstance) as OriginContextFragment;
            aPanel.Fragments.Add(aOriginContextFragment);

            PanelAnalyticalFragment aBuildingElementAnalyticalProperties = new PanelAnalyticalFragment();
            aBuildingElementAnalyticalProperties = aBuildingElementAnalyticalProperties.UpdateValues(pullSettings, familyInstance.Symbol) as PanelAnalyticalFragment;
            aBuildingElementAnalyticalProperties = aBuildingElementAnalyticalProperties.UpdateValues(pullSettings, familyInstance) as PanelAnalyticalFragment;
            aPanel.Fragments.Add(aBuildingElementAnalyticalProperties);

            PanelContextFragment aBuildingElementContextProperties = new PanelContextFragment();
            aBuildingElementContextProperties = aBuildingElementContextProperties.UpdateValues(pullSettings, familyInstance.Symbol) as PanelContextFragment;
            aBuildingElementContextProperties = aBuildingElementContextProperties.UpdateValues(pullSettings, familyInstance) as PanelContextFragment;
            aPanel.Fragments.Add(aBuildingElementContextProperties);

            BuildingResultFragment aBuildingResultsProperties = new BuildingResultFragment();
            aBuildingResultsProperties = aBuildingResultsProperties.UpdateValues(pullSettings, familyInstance.Symbol) as BuildingResultFragment;
            aBuildingResultsProperties = aBuildingResultsProperties.UpdateValues(pullSettings, familyInstance) as BuildingResultFragment;
            aPanel.Fragments.Add(aBuildingResultsProperties);

            oM.Environment.Elements.PanelType? aBuildingElementType = Query.PanelType(familyInstance.Category);
            if (aBuildingElementType.HasValue)
                aPanel.Type = aBuildingElementType.Value;
            else
                aPanel.Type = oM.Environment.Elements.PanelType.Undefined;

            aPanel = Modify.SetIdentifiers(aPanel, familyInstance) as oM.Environment.Elements.Panel;
            if (pullSettings.CopyCustomData)
                aPanel = Modify.SetCustomData(aPanel, familyInstance, pullSettings.ConvertUnits) as oM.Environment.Elements.Panel;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aPanel);

            aPanel = aPanel.UpdateBuildingElementTypeByCustomData();
            aPanel = aPanel.UpdateValues(pullSettings, familyInstance.Symbol) as oM.Environment.Elements.Panel;
            aPanel = aPanel.UpdateValues(pullSettings, familyInstance) as oM.Environment.Elements.Panel;
            return aPanel;
        }

        /***************************************************/

        public static oM.Environment.Elements.Panel ToBHoMEnvironmentPanel(this EnergyAnalysisSurface energyAnalysisSurface, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            oM.Environment.Elements.Panel aPanel = pullSettings.FindRefObject<oM.Environment.Elements.Panel>(energyAnalysisSurface.Id.IntegerValue);
            if (aPanel != null)
                return aPanel;

            //Get the geometry Curve
            ICurve aCurve = null;
            if (energyAnalysisSurface != null)
                aCurve = energyAnalysisSurface.GetPolyloop().ToBHoM(pullSettings);

            //Get the name and element type
            Element aElement = Query.Element(energyAnalysisSurface.Document, energyAnalysisSurface.CADObjectUniqueId, energyAnalysisSurface.CADLinkUniqueId);
            ElementType aElementType = null;
            if (aElement != null)
            {
                aElementType = aElement.Document.GetElement(aElement.GetTypeId()) as ElementType;
                aPanel = Create.Panel(name: Query.FamilyTypeFullName(aElement), externalEdges: aCurve.ToEdges());
            }

            //Set ExtendedProperties
            OriginContextFragment aOriginContextFragment = new OriginContextFragment();
            aOriginContextFragment.ElementID = aElement.Id.IntegerValue.ToString();
            aOriginContextFragment.TypeName = Query.FamilyTypeFullName(aElement);
            aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, aElement) as OriginContextFragment;
            aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, aElementType) as OriginContextFragment;
            aPanel.AddFragment(aOriginContextFragment);

            PanelAnalyticalFragment aBuildingElementAnalyticalProperties = new PanelAnalyticalFragment();
            aBuildingElementAnalyticalProperties = aBuildingElementAnalyticalProperties.UpdateValues(pullSettings, aElementType) as PanelAnalyticalFragment;
            aBuildingElementAnalyticalProperties = aBuildingElementAnalyticalProperties.UpdateValues(pullSettings, aElement) as PanelAnalyticalFragment;
            aPanel.AddFragment(aBuildingElementAnalyticalProperties);

            PanelContextFragment aBuildingElementContextProperties = new PanelContextFragment();

            List<string> aConnectedSpaces = new List<string>();
            EnergyAnalysisSpace aEnergyAnalysisSpace = null;
            aEnergyAnalysisSpace = energyAnalysisSurface.GetAnalyticalSpace();
            if (aEnergyAnalysisSpace != null)
            {
                SpatialElement aSpatialElement = Query.Element(aEnergyAnalysisSpace.Document, aEnergyAnalysisSpace.CADObjectUniqueId) as SpatialElement;
                aConnectedSpaces.Add(Query.Name(aSpatialElement));
            }

            aEnergyAnalysisSpace = energyAnalysisSurface.GetAdjacentAnalyticalSpace();
            if (aEnergyAnalysisSpace != null)
            {
                SpatialElement aSpatialElement = Query.Element(aEnergyAnalysisSpace.Document, aEnergyAnalysisSpace.CADObjectUniqueId) as SpatialElement;
                if (aSpatialElement != null)
                {
                    aConnectedSpaces.Add(Query.Name(aSpatialElement));

                    if (aSpatialElement is Autodesk.Revit.DB.Mechanical.Space)
                    {
                        Autodesk.Revit.DB.Mechanical.Space aSpace = (Autodesk.Revit.DB.Mechanical.Space)aSpatialElement;

                        BuildingResultFragment aBuildingResultsProperties = new BuildingResultFragment();
                        aBuildingResultsProperties.PeakCooling = UnitUtils.ConvertFromInternalUnits(aSpace.DesignCoolingLoad, DisplayUnitType.DUT_WATTS);
                        aBuildingResultsProperties.PeakHeating = UnitUtils.ConvertFromInternalUnits(aSpace.DesignHeatingLoad, DisplayUnitType.DUT_WATTS);
                        aPanel.AddFragment(aBuildingResultsProperties);
                    }
                }
            }

            aPanel.ConnectedSpaces = aConnectedSpaces;
            aBuildingElementContextProperties = aBuildingElementContextProperties.UpdateValues(pullSettings, aElementType) as PanelContextFragment;
            aBuildingElementContextProperties = aBuildingElementContextProperties.UpdateValues(pullSettings, aElement) as PanelContextFragment;
            aPanel.AddFragment(aBuildingElementContextProperties);

            oM.Environment.Elements.PanelType? aBuildingElementType = Query.PanelType(aElement.Category);
            if (aBuildingElementType.HasValue)
                aPanel.Type = aBuildingElementType.Value;
            else
                aPanel.Type = oM.Environment.Elements.PanelType.Undefined;

            aPanel.Construction = Convert.ToBHoMConstruction(aElementType as dynamic, pullSettings);

            //Set some custom data properties
            aPanel = Modify.SetIdentifiers(aPanel, aElement) as oM.Environment.Elements.Panel;
            if (pullSettings.CopyCustomData)
            {
                aPanel = Modify.SetCustomData(aPanel, aElement, pullSettings.ConvertUnits) as oM.Environment.Elements.Panel;
                double aHeight = energyAnalysisSurface.Height;
                double aWidth = energyAnalysisSurface.Width;
                double aAzimuth = energyAnalysisSurface.Azimuth;
                if (pullSettings.ConvertUnits)
                {
                    aHeight = UnitUtils.ConvertFromInternalUnits(aHeight, DisplayUnitType.DUT_METERS);
                    aWidth = UnitUtils.ConvertFromInternalUnits(aWidth, DisplayUnitType.DUT_METERS);
                }
                aPanel = Modify.SetCustomData(aPanel, "Height", aHeight) as oM.Environment.Elements.Panel;
                aPanel = Modify.SetCustomData(aPanel, "Width", aWidth) as oM.Environment.Elements.Panel;
                aPanel = Modify.SetCustomData(aPanel, "Azimuth", aAzimuth) as oM.Environment.Elements.Panel;
                aPanel = Modify.SetCustomData(aPanel, aElementType, BuiltInParameter.ALL_MODEL_FAMILY_NAME, pullSettings.ConvertUnits) as oM.Environment.Elements.Panel;
                aPanel = Modify.AddSpaceId(aPanel, energyAnalysisSurface);
                aPanel = Modify.AddAdjacentSpaceId(aPanel, energyAnalysisSurface);

                if (aElementType != null)
                    aPanel = Modify.SetCustomData(aPanel, aElementType, pullSettings.ConvertUnits, "Type ") as oM.Environment.Elements.Panel;
            }

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aPanel);

            aPanel = aPanel.UpdateBuildingElementTypeByCustomData();
            aPanel = aPanel.UpdateValues(pullSettings, aElementType) as oM.Environment.Elements.Panel;
            aPanel = aPanel.UpdateValues(pullSettings, aElement) as oM.Environment.Elements.Panel;
            return aPanel;
        }

        /***************************************************/

        //public static BuildingElement ToBHoMBuildingElement(this EnergyAnalysisOpening energyAnalysisOpening, EnergyAnalysisSurface energyAnalysisSurface = null, PullSettings pullSettings = null)
        //{
        //    pullSettings = pullSettings.DefaultIfNull();

        //    BuildingElement aBuildingElement = pullSettings.FindRefObject<BuildingElement>(energyAnalysisOpening.Id.IntegerValue);
        //    if (aBuildingElement != null)
        //        return aBuildingElement;

        //    //Get the geometry Curve
        //    ICurve aCurve = null;
        //    if (energyAnalysisOpening != null)
        //        aCurve = energyAnalysisOpening.GetPolyloop().ToBHoM(pullSettings);

        //    //Get the name and element type
        //    Element aElement = Query.Element(energyAnalysisOpening.Document, energyAnalysisOpening.CADObjectUniqueId, energyAnalysisOpening.CADLinkUniqueId);
        //    ElementType aElementType = null;
        //    if (aElement != null)
        //    {
        //        aElementType = aElement.Document.GetElement(aElement.GetTypeId()) as ElementType;
        //        BuildingElementProperties aBuildingElementProperties = aElementType.ToBHoMElementProperties(pullSettings);
        //        aBuildingElementProperties.Construction = Query.Construction(energyAnalysisOpening, pullSettings);
        //        aBuildingElement = Create.BuildingElement(Query.FamilyTypeFullName(aElement), aCurve, aBuildingElementProperties);
        //        aBuildingElement.ElementID = aElement.Id.IntegerValue.ToString();
        //    }

        //    //Set ExtendedProperties
        //    OriginContextFragment aOriginContextFragment = new OriginContextFragment();
        //    aOriginContextFragment.ElementID = aElement.Id.IntegerValue.ToString();
        //    aOriginContextFragment.TypeName = Query.FamilyTypeFullName(aElement);
        //    aBuildingElement.ExtendedProperties.Add(aOriginContextFragment);

        //    BuildingElementAnalyticalProperties aBuildingElementAnalyticalProperties = new BuildingElementAnalyticalProperties();
        //    aBuildingElement.AddFragment(aBuildingElementAnalyticalProperties);

        //    BuildingElementContextProperties aBuildingElementContextProperties = new BuildingElementContextProperties();
        //    if(energyAnalysisSurface != null)
        //    {
        //        List<string> aConnectedSpaces = new List<string>();
        //        EnergyAnalysisSpace aEnergyAnalysisSpace = null;
        //        aEnergyAnalysisSpace = energyAnalysisSurface.GetAnalyticalSpace();
        //        if (aEnergyAnalysisSpace != null)
        //        {
        //            SpatialElement aSpatialElement = Query.Element(aEnergyAnalysisSpace.Document, aEnergyAnalysisSpace.CADObjectUniqueId) as SpatialElement;
        //            if (aSpatialElement != null)
        //                aConnectedSpaces.Add(aSpatialElement.Name);
        //        }

        //        aEnergyAnalysisSpace = energyAnalysisSurface.GetAdjacentAnalyticalSpace();
        //        if (aEnergyAnalysisSpace != null)
        //        {
        //            SpatialElement aSpatialElement = Query.Element(aEnergyAnalysisSpace.Document, aEnergyAnalysisSpace.CADObjectUniqueId) as SpatialElement;
        //            if (aSpatialElement != null)
        //                aConnectedSpaces.Add(aSpatialElement.Name);
        //        }

        //        aBuildingElementContextProperties.ConnectedSpaces = aConnectedSpaces;
        //    }
        //    aBuildingElement.AddFragment(aBuildingElementContextProperties);

        //    BuildingResultsProperties aBuildingResultsProperties = new BuildingResultsProperties();
        //    aBuildingElement.AddFragment(aBuildingResultsProperties);

        //    ElementProperties aElementProperties = new ElementProperties();
        //    aElementProperties.Construction = Query.Construction(energyAnalysisOpening, pullSettings);
        //    BuildingElementType? aBuildingElementType = Query.BuildingElementType(aElement.Category);
        //    if (aBuildingElementType.HasValue)
        //        aElementProperties.BuildingElementType = aBuildingElementType.Value;
        //    else
        //        aElementProperties.BuildingElementType = BuildingElementType.Undefined;
        //    aBuildingElement.AddFragment(aElementProperties);

        //    //Set custom data on BuildingElement
        //    aBuildingElement = Modify.SetIdentifiers(aBuildingElement, aElement) as BuildingElement;
        //    if (pullSettings.CopyCustomData)
        //    {
        //        aBuildingElement = Modify.SetCustomData(aBuildingElement, aElement, pullSettings.ConvertUnits) as BuildingElement;

        //        double aHeight = energyAnalysisOpening.Height;
        //        double aWidth = energyAnalysisOpening.Width;
        //        if (pullSettings.ConvertUnits)
        //        {
        //            aHeight = UnitUtils.ConvertFromInternalUnits(aHeight, DisplayUnitType.DUT_METERS);
        //            aWidth = UnitUtils.ConvertFromInternalUnits(aWidth, DisplayUnitType.DUT_METERS);
        //        }
        //        aBuildingElement = Modify.SetCustomData(aBuildingElement, "Height", aHeight) as BuildingElement;
        //        aBuildingElement = Modify.SetCustomData(aBuildingElement, "Width", aWidth) as BuildingElement;
        //        aBuildingElement = Modify.SetCustomData(aBuildingElement, "Opening Type", energyAnalysisOpening.OpeningType.ToString()) as BuildingElement;
        //        aBuildingElement = Modify.SetCustomData(aBuildingElement, "Opening Name", energyAnalysisOpening.OpeningName) as BuildingElement;
        //        aBuildingElement = Modify.SetCustomData(aBuildingElement, aElementType, BuiltInParameter.ALL_MODEL_FAMILY_NAME, pullSettings.ConvertUnits) as BuildingElement;
        //        aBuildingElement = Modify.AddSpaceId(aBuildingElement, energyAnalysisSurface);
        //        aBuildingElement = Modify.AddAdjacentSpaceId(aBuildingElement, energyAnalysisSurface);

        //    }

        //    pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aBuildingElement);

        //    return aBuildingElement;
        //}

        /***************************************************/

        public static List<oM.Environment.Elements.Panel> ToBHoMEnvironmentPanels(this Ceiling ceiling, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> aPanels = pullSettings.FindRefObjects<oM.Environment.Elements.Panel>(ceiling.Id.IntegerValue);
            if (aPanels != null && aPanels.Count > 0)
                return aPanels;

            List<PolyCurve> aPolyCurveList = Query.Profiles(ceiling, pullSettings);
            if (aPolyCurveList == null)
                return aPanels;

            aPanels = new List<oM.Environment.Elements.Panel>();

            CeilingType aCeilingType = ceiling.Document.GetElement(ceiling.GetTypeId()) as CeilingType;
            BH.oM.Physical.Constructions.Construction aConstruction = ToBHoMConstruction(aCeilingType, pullSettings);

            List<PolyCurve> aPolyCurveList_Outer = BH.Engine.Adapters.Revit.Query.OuterPolyCurves(aPolyCurveList);
            foreach (ICurve aCurve in aPolyCurveList_Outer)
            {
                //Create the BuildingElement
                oM.Environment.Elements.Panel aPanel = Create.Panel(externalEdges: aCurve.ToEdges());
                aPanel.Name = Query.FamilyTypeFullName(ceiling);

                //Set ExtendedProperties
                OriginContextFragment aOriginContextFragment = new OriginContextFragment();
                aOriginContextFragment.ElementID = ceiling.Id.IntegerValue.ToString();
                aOriginContextFragment.TypeName = Query.FamilyTypeFullName(ceiling);
                aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, aCeilingType) as OriginContextFragment;
                aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, ceiling) as OriginContextFragment;
                aPanel.AddFragment(aOriginContextFragment);

                PanelAnalyticalFragment aBuildingElementAnalyticalProperties = new PanelAnalyticalFragment();
                aBuildingElementAnalyticalProperties = aBuildingElementAnalyticalProperties.UpdateValues(pullSettings, aCeilingType) as PanelAnalyticalFragment;
                aBuildingElementAnalyticalProperties = aBuildingElementAnalyticalProperties.UpdateValues(pullSettings, ceiling) as PanelAnalyticalFragment;
                aPanel.AddFragment(aBuildingElementAnalyticalProperties);

                PanelContextFragment aBuildingElementContextProperties = new PanelContextFragment();
                aBuildingElementContextProperties = aBuildingElementContextProperties.UpdateValues(pullSettings, aCeilingType) as PanelContextFragment;
                aBuildingElementContextProperties = aBuildingElementContextProperties.UpdateValues(pullSettings, ceiling) as PanelContextFragment;
                aPanel.AddFragment(aBuildingElementContextProperties);

                BuildingResultFragment aBuildingResultsProperties = new BuildingResultFragment();
                aBuildingResultsProperties = aBuildingResultsProperties.UpdateValues(pullSettings, aCeilingType) as BuildingResultFragment;
                aBuildingResultsProperties = aBuildingResultsProperties.UpdateValues(pullSettings, ceiling) as BuildingResultFragment;
                aPanel.AddFragment(aBuildingResultsProperties);

                aPanel.Construction = aConstruction;
                aPanel.Type = oM.Environment.Elements.PanelType.Ceiling;

                //Assign custom data
                aPanel = Modify.SetIdentifiers(aPanel, ceiling) as oM.Environment.Elements.Panel;
                if (pullSettings.CopyCustomData)
                    aPanel = Modify.SetCustomData(aPanel, ceiling, pullSettings.ConvertUnits) as oM.Environment.Elements.Panel;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aPanel);
                aPanel = aPanel.UpdateValues(pullSettings, aCeilingType) as oM.Environment.Elements.Panel;
                aPanel = aPanel.UpdateValues(pullSettings, ceiling) as oM.Environment.Elements.Panel;
                aPanels.Add(aPanel);
            }

            aPanels = aPanels.UpdateBuildingElementTypeByCustomData();
            return aPanels;
        }

        /***************************************************/

        public static List<oM.Environment.Elements.Panel> ToBHoMEnvironmentPanels(this Floor floor, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> aPanels = pullSettings.FindRefObjects<oM.Environment.Elements.Panel>(floor.Id.IntegerValue);
            if (aPanels != null && aPanels.Count > 0)
                return aPanels;

            List<PolyCurve> aPolyCurveList = Query.Profiles(floor, pullSettings);
            if (aPolyCurveList == null)
                return aPanels;

            aPanels = new List<oM.Environment.Elements.Panel>();

            BH.oM.Physical.Constructions.Construction aConstruction = ToBHoMConstruction(floor.FloorType, pullSettings);

            FloorType aFloorType = floor.Document.GetElement(floor.GetTypeId()) as FloorType;

            List<PolyCurve> aPolyCurveList_Outer = BH.Engine.Adapters.Revit.Query.OuterPolyCurves(aPolyCurveList);
            foreach (ICurve aCurve in aPolyCurveList_Outer)
            {
                //Create the BuildingElement
                oM.Environment.Elements.Panel aPanel = Create.Panel(externalEdges: aCurve.ToEdges());
                aPanel.Name = Query.FamilyTypeFullName(floor);

                //Set ExtendedProperties
                OriginContextFragment aOriginContextFragment = new OriginContextFragment();
                aOriginContextFragment.ElementID = floor.Id.IntegerValue.ToString();
                aOriginContextFragment.TypeName = Query.FamilyTypeFullName(floor);
                aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, aFloorType) as OriginContextFragment;
                aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, floor) as OriginContextFragment;
                aPanel.AddFragment(aOriginContextFragment);

                PanelAnalyticalFragment aBuildingElementAnalyticalProperties = new PanelAnalyticalFragment();
                aBuildingElementAnalyticalProperties = aBuildingElementAnalyticalProperties.UpdateValues(pullSettings, aFloorType) as PanelAnalyticalFragment;
                aBuildingElementAnalyticalProperties = aBuildingElementAnalyticalProperties.UpdateValues(pullSettings, floor) as PanelAnalyticalFragment;
                aPanel.AddFragment(aBuildingElementAnalyticalProperties);

                PanelContextFragment aBuildingElementContextProperties = new PanelContextFragment();
                aBuildingElementContextProperties = aBuildingElementContextProperties.UpdateValues(pullSettings, aFloorType) as PanelContextFragment;
                aBuildingElementContextProperties = aBuildingElementContextProperties.UpdateValues(pullSettings, floor) as PanelContextFragment;
                aPanel.AddFragment(aBuildingElementContextProperties);

                BuildingResultFragment aBuildingResultsProperties = new BuildingResultFragment();
                aBuildingResultsProperties = aBuildingResultsProperties.UpdateValues(pullSettings, aFloorType) as BuildingResultFragment;
                aBuildingResultsProperties = aBuildingResultsProperties.UpdateValues(pullSettings, floor) as BuildingResultFragment;
                aPanel.AddFragment(aBuildingResultsProperties);

                aPanel.Construction = aConstruction;
                aPanel.Type = oM.Environment.Elements.PanelType.Floor;

                //Assign custom data
                aPanel = Modify.SetIdentifiers(aPanel, floor) as oM.Environment.Elements.Panel;
                if (pullSettings.CopyCustomData)
                    aPanel = Modify.SetCustomData(aPanel, floor, pullSettings.ConvertUnits) as oM.Environment.Elements.Panel;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aPanel);

                aPanel = aPanel.UpdateValues(pullSettings, aFloorType) as oM.Environment.Elements.Panel;
                aPanel = aPanel.UpdateValues(pullSettings, floor) as oM.Environment.Elements.Panel;
                aPanels.Add(aPanel);
            }

            aPanels = aPanels.UpdateBuildingElementTypeByCustomData();
            return aPanels;
        }

        /***************************************************/

        public static List<oM.Environment.Elements.Panel> ToBHoMEnvironmentPanels(this RoofBase roofBase, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> aPanels = pullSettings.FindRefObjects<oM.Environment.Elements.Panel>(roofBase.Id.IntegerValue);
            if (aPanels != null && aPanels.Count > 0)
                return aPanels;

            List<PolyCurve> aPolyCurveList = Query.Profiles(roofBase, pullSettings);
            if (aPolyCurveList == null)
                return aPanels;

            aPanels = new List<oM.Environment.Elements.Panel>();

            BH.oM.Physical.Constructions.Construction aConstruction = ToBHoMConstruction(roofBase.RoofType, pullSettings);

            List<PolyCurve> aPolyCurveList_Outer = BH.Engine.Adapters.Revit.Query.OuterPolyCurves(aPolyCurveList);
            foreach (ICurve aCurve in aPolyCurveList_Outer)
            {
                //Create the BuildingElement
                oM.Environment.Elements.Panel aPanel = Create.Panel(externalEdges: aCurve.ToEdges());
                aPanel.Name = Query.FamilyTypeFullName(roofBase);

                //Set ExtendedProperties
                OriginContextFragment aOriginContextFragment = new OriginContextFragment();
                aOriginContextFragment.ElementID = roofBase.Id.IntegerValue.ToString();
                aOriginContextFragment.TypeName = Query.FamilyTypeFullName(roofBase);
                aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, roofBase.RoofType) as OriginContextFragment;
                aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, roofBase) as OriginContextFragment;
                aPanel.AddFragment(aOriginContextFragment);

                PanelAnalyticalFragment aBuildingElementAnalyticalProperties = new PanelAnalyticalFragment();
                aBuildingElementAnalyticalProperties = aBuildingElementAnalyticalProperties.UpdateValues(pullSettings, roofBase.RoofType) as PanelAnalyticalFragment;
                aBuildingElementAnalyticalProperties = aBuildingElementAnalyticalProperties.UpdateValues(pullSettings, roofBase) as PanelAnalyticalFragment;
                aPanel.AddFragment(aBuildingElementAnalyticalProperties);

                PanelContextFragment aBuildingElementContextProperties = new PanelContextFragment();
                aBuildingElementContextProperties = aBuildingElementContextProperties.UpdateValues(pullSettings, roofBase.RoofType) as PanelContextFragment;
                aBuildingElementContextProperties = aBuildingElementContextProperties.UpdateValues(pullSettings, roofBase) as PanelContextFragment;
                aPanel.AddFragment(aBuildingElementContextProperties);

                BuildingResultFragment aBuildingResultsProperties = new BuildingResultFragment();
                aBuildingResultsProperties = aBuildingResultsProperties.UpdateValues(pullSettings, roofBase.RoofType) as BuildingResultFragment;
                aBuildingResultsProperties = aBuildingResultsProperties.UpdateValues(pullSettings, roofBase) as BuildingResultFragment;
                aPanel.AddFragment(aBuildingResultsProperties);

                aPanel.Construction = aConstruction;
                aPanel.Type = oM.Environment.Elements.PanelType.Roof;

                //Assign custom data
                aPanel = Modify.SetIdentifiers(aPanel, roofBase) as oM.Environment.Elements.Panel;
                if (pullSettings.CopyCustomData)
                    aPanel = Modify.SetCustomData(aPanel, roofBase, pullSettings.ConvertUnits) as oM.Environment.Elements.Panel;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aPanel);

                aPanel = aPanel.UpdateValues(pullSettings, roofBase.RoofType) as oM.Environment.Elements.Panel;
                aPanel = aPanel.UpdateValues(pullSettings, roofBase) as oM.Environment.Elements.Panel;
                aPanels.Add(aPanel);
            }

            aPanels = aPanels.UpdateBuildingElementTypeByCustomData();
            return aPanels;
        }

        /***************************************************/

        public static List<oM.Environment.Elements.Panel> ToBHoMEnvironmentPanels(this Wall wall, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> aPanels = pullSettings.FindRefObjects<oM.Environment.Elements.Panel>(wall.Id.IntegerValue);
            if (aPanels != null && aPanels.Count > 0)
                return aPanels;

            aPanels = new List<oM.Environment.Elements.Panel>();

            BH.oM.Physical.Constructions.Construction aConstruction = ToBHoMConstruction(wall.WallType, pullSettings);

            List<PolyCurve> aPolyCurveList = Query.Profiles(wall, pullSettings);
            List<PolyCurve> aPolyCurveList_Outer = BH.Engine.Adapters.Revit.Query.OuterPolyCurves(aPolyCurveList);

            foreach (ICurve aCurve in aPolyCurveList_Outer)
            {
                //Create the BuildingElement
                oM.Environment.Elements.Panel aPanel = Create.Panel(externalEdges: aCurve.ToEdges());
                aPanel.Name = Query.FamilyTypeFullName(wall);

                //Set ExtendedProperties
                OriginContextFragment aOriginContextFragment = new OriginContextFragment();
                aOriginContextFragment.ElementID = wall.Id.IntegerValue.ToString();
                aOriginContextFragment.TypeName = Query.FamilyTypeFullName(wall);
                aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, wall.WallType) as OriginContextFragment;
                aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, wall) as OriginContextFragment;
                aPanel.AddFragment(aOriginContextFragment);

                PanelAnalyticalFragment aBuildingElementAnalyticalProperties = new PanelAnalyticalFragment();
                aBuildingElementAnalyticalProperties = aBuildingElementAnalyticalProperties.UpdateValues(pullSettings, wall.WallType) as PanelAnalyticalFragment;
                aBuildingElementAnalyticalProperties = aBuildingElementAnalyticalProperties.UpdateValues(pullSettings, wall) as PanelAnalyticalFragment;
                aPanel.AddFragment(aBuildingElementAnalyticalProperties);

                PanelContextFragment aBuildingElementContextProperties = new PanelContextFragment();
                aBuildingElementContextProperties = aBuildingElementContextProperties.UpdateValues(pullSettings, wall.WallType) as PanelContextFragment;
                aBuildingElementContextProperties = aBuildingElementContextProperties.UpdateValues(pullSettings, wall) as PanelContextFragment;
                aPanel.AddFragment(aBuildingElementContextProperties);

                BuildingResultFragment aBuildingResultsProperties = new BuildingResultFragment();
                aBuildingResultsProperties = aBuildingResultsProperties.UpdateValues(pullSettings, wall.WallType) as BuildingResultFragment;
                aBuildingResultsProperties = aBuildingResultsProperties.UpdateValues(pullSettings, wall) as BuildingResultFragment;
                aPanel.AddFragment(aBuildingResultsProperties);

                aPanel.Construction = aConstruction;
                aPanel.Type = oM.Environment.Elements.PanelType.Wall;

                //Assign custom data
                aPanel = Modify.SetIdentifiers(aPanel, wall) as oM.Environment.Elements.Panel;
                if (pullSettings.CopyCustomData)
                    aPanel = Modify.SetCustomData(aPanel, wall, pullSettings.ConvertUnits) as oM.Environment.Elements.Panel;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aPanel);

                aPanel = aPanel.UpdateValues(pullSettings, wall.WallType) as oM.Environment.Elements.Panel;
                aPanel = aPanel.UpdateValues(pullSettings, wall) as oM.Environment.Elements.Panel;
                aPanels.Add(aPanel);
            }

            aPanels = aPanels.UpdateBuildingElementTypeByCustomData();
            return aPanels;
        }

        /***************************************************/
    }
}