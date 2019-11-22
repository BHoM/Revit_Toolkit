/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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

using BH.Engine.Environment;
using BH.oM.Environment.Elements;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Environment.Fragments;
using System.Collections.Generic;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static Space ToBHoMSpace(this SpatialElement spatialElement, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            SpatialElementBoundaryOptions aSpatialElementBoundaryOptions = new SpatialElementBoundaryOptions();
            aSpatialElementBoundaryOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
            aSpatialElementBoundaryOptions.StoreFreeBoundaryFaces = false;

            return ToBHoMSpace(spatialElement, aSpatialElementBoundaryOptions, pullSettings);
        }

        /***************************************************/

        public static Space ToBHoMSpace(this SpatialElement spatialElement, SpatialElementBoundaryOptions spatialElementBoundaryOptions, PullSettings pullSettings = null)
        {
            if (spatialElement == null || spatialElementBoundaryOptions == null)
                return new Space();

            SpatialElementGeometryCalculator aSpatialElementGeometryCalculator = new SpatialElementGeometryCalculator(spatialElement.Document, spatialElementBoundaryOptions);

            return ToBHoMSpace(spatialElement, aSpatialElementGeometryCalculator, pullSettings);
        }

        /***************************************************/

        public static Space ToBHoMSpace(this SpatialElement spatialElement, SpatialElementGeometryCalculator spatialElementGeometryCalculator, PullSettings pullSettings = null)
        {
            if (spatialElement == null || spatialElementGeometryCalculator == null)
                return new Space();

            pullSettings = pullSettings.DefaultIfNull();

            Space aSpace = pullSettings.FindRefObject<Space>(spatialElement.Id.IntegerValue);
            if (aSpace != null)
                return aSpace;

            string aName = Query.Name(spatialElement);

            //Create the Space
            aSpace = Create.Space(aName);

            if(spatialElement.Location != null && spatialElement.Location is LocationPoint)
                aSpace.Location = ((LocationPoint)spatialElement.Location).ToBHoM(pullSettings);

            //Set ExtendedProperties
            OriginContextFragment aOriginContextFragment = new OriginContextFragment();
            aOriginContextFragment.ElementID = spatialElement.Id.IntegerValue.ToString();
            aOriginContextFragment.TypeName = Query.Name(spatialElement);
            aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, spatialElement) as OriginContextFragment;
            aSpace.AddFragment(aOriginContextFragment);

            SpaceAnalyticalFragment aSpaceAnalyticalFragment = new SpaceAnalyticalFragment();
            aSpaceAnalyticalFragment = aSpaceAnalyticalFragment.UpdateValues(pullSettings, spatialElement) as SpaceAnalyticalFragment;
            aSpace.AddFragment(aSpaceAnalyticalFragment);

            SpaceContextFragment aSpaceContextFragment = new SpaceContextFragment();
            aSpaceContextFragment = aSpaceContextFragment.UpdateValues(pullSettings, spatialElement) as SpaceContextFragment;
            //TODO: Implement ConnectedElements
            aSpace.AddFragment(aSpaceContextFragment);

            //Set custom data
            aSpace = Modify.SetIdentifiers(aSpace, spatialElement) as Space;
            if (pullSettings.CopyCustomData)
                aSpace = Modify.SetCustomData(aSpace, spatialElement, pullSettings.ConvertUnits) as Space;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aSpace);
            aSpace = aSpace.UpdateValues(pullSettings, spatialElement) as Space;
            return aSpace;

            //if (!SpatialElementGeometryCalculator.CanCalculateGeometry(spatialElement))
            //{
            //    //SpatialElementGeometryResults aSpatialElementGeometryResults = spatialElementGeometryCalculator.CalculateSpatialElementGeometry(spatialElement);
            //    //foreach(SpatialElementBoundarySubface aSpatialElementBoundarySubface in  aSpatialElementGeometryResults.GetBoundaryFaceInfo())
            //    //{
            //    //    aSpatialElementBoundarySubface.
            //    //}
            //}
            //else
            //{

            //}
        }

        /***************************************************/

        public static Space ToBHoMSpace(this EnergyAnalysisSpace energyAnalysisSpace, PullSettings pullSettings = null)
        {
            if (energyAnalysisSpace == null)
                return new Space();

            pullSettings = pullSettings.DefaultIfNull();

            Space aSpace = pullSettings.FindRefObject<Space>(energyAnalysisSpace.Id.IntegerValue);
            if (aSpace != null)
                return aSpace;

            SpatialElement aSpatialElement = Query.Element(energyAnalysisSpace.Document, energyAnalysisSpace.CADObjectUniqueId) as SpatialElement;

            string aName = Query.Name(aSpatialElement);
            aSpace = Create.Space(aName);

            if (aSpatialElement != null && aSpatialElement.Location != null)
                aSpace.Location = (aSpatialElement.Location as LocationPoint).ToBHoM(pullSettings);

            //Set ExtendedProperties
            OriginContextFragment aOriginContextFragment = new OriginContextFragment();
            aOriginContextFragment.ElementID = aSpatialElement.Id.IntegerValue.ToString();
            aOriginContextFragment.TypeName = Query.Name(aSpatialElement);
            aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, energyAnalysisSpace) as OriginContextFragment;
            aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, aSpatialElement) as OriginContextFragment;
            aSpace.AddFragment(aOriginContextFragment);

            SpaceAnalyticalFragment aSpaceAnalyticalFragment = new SpaceAnalyticalFragment();
            aSpaceAnalyticalFragment = aSpaceAnalyticalFragment.UpdateValues(pullSettings, energyAnalysisSpace) as SpaceAnalyticalFragment;
            aSpaceAnalyticalFragment = aSpaceAnalyticalFragment.UpdateValues(pullSettings, aSpatialElement) as SpaceAnalyticalFragment;
            aSpace.AddFragment(aSpaceAnalyticalFragment);

            SpaceContextFragment aSpaceContextFragment = new SpaceContextFragment();
            List<string> aConnectedElements = new List<string>();
            foreach (EnergyAnalysisSurface aEnergyAnalysisSurface in energyAnalysisSpace.GetAnalyticalSurfaces())
                aConnectedElements.Add(aEnergyAnalysisSurface.CADObjectUniqueId);
            aSpaceContextFragment.ConnectedElements = aConnectedElements;
            aSpaceContextFragment = aSpaceContextFragment.UpdateValues(pullSettings, energyAnalysisSpace) as SpaceContextFragment;
            aSpaceContextFragment = aSpaceContextFragment.UpdateValues(pullSettings, aSpatialElement) as SpaceContextFragment;
            aSpace.AddFragment(aSpaceContextFragment);

            //Set custom data
            aSpace = Modify.SetIdentifiers(aSpace, aSpatialElement) as Space;
            if (pullSettings.CopyCustomData)
            {
                aSpace = Modify.SetCustomData(aSpace, aSpatialElement, pullSettings.ConvertUnits) as Space;
                double aInnerVolume = energyAnalysisSpace.InnerVolume;
                double aAnalyticalVolume = energyAnalysisSpace.AnalyticalVolume;
                if (pullSettings.ConvertUnits)
                {
                    aInnerVolume = aInnerVolume.ToSI(UnitType.UT_Volume);
                    aAnalyticalVolume = aAnalyticalVolume.ToSI(UnitType.UT_Volume);
                }

                aSpace = Modify.SetCustomData(aSpace, "Inner Volume", aInnerVolume) as Space;
                aSpace = Modify.SetCustomData(aSpace, "Analytical Volume", aAnalyticalVolume) as Space;
            }

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aSpace);
            aSpace = aSpace.UpdateValues(pullSettings, energyAnalysisSpace) as Space;
            aSpace = aSpace.UpdateValues(pullSettings, aSpatialElement) as Space;
            return aSpace;
        }

        /***************************************************/
    }
}