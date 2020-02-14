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
using BH.Engine.Environment;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Environment.Elements;
using BH.oM.Environment.Fragments;
using System.Collections.Generic;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static Space ToBHoMSpace(this SpatialElement spatialElement, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            SpatialElementBoundaryOptions spatialElementBoundaryOptions = new SpatialElementBoundaryOptions();
            spatialElementBoundaryOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
            spatialElementBoundaryOptions.StoreFreeBoundaryFaces = false;

            return ToBHoMSpace(spatialElement, spatialElementBoundaryOptions, pullSettings);
        }

        /***************************************************/

        public static Space ToBHoMSpace(this SpatialElement spatialElement, SpatialElementBoundaryOptions spatialElementBoundaryOptions, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (spatialElement == null || spatialElementBoundaryOptions == null)
                return new Space();

            SpatialElementGeometryCalculator spatialElementGeometryCalculator = new SpatialElementGeometryCalculator(spatialElement.Document, spatialElementBoundaryOptions);

            return ToBHoMSpace(spatialElement, spatialElementGeometryCalculator, pullSettings);
        }

        /***************************************************/

        public static Space ToBHoMSpace(this SpatialElement spatialElement, SpatialElementGeometryCalculator spatialElementGeometryCalculator, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (spatialElement == null || spatialElementGeometryCalculator == null)
                return new Space();

            settings = settings.DefaultIfNull();

            Space space = pullSettings.FindRefObject<Space>(spatialElement.Id.IntegerValue);
            if (space != null)
                return space;

            string name = Query.Name(spatialElement);

            //Create the Space
            space = BH.Engine.Environment.Create.Space(name);

            if(spatialElement.Location != null && spatialElement.Location is LocationPoint)
                space.Location = ((LocationPoint)spatialElement.Location).ToBHoM();

            //Set ExtendedProperties
            OriginContextFragment originContext = new OriginContextFragment();
            originContext.ElementID = spatialElement.Id.IntegerValue.ToString();
            originContext.TypeName = Query.Name(spatialElement);
            originContext = originContext.UpdateValues(pullSettings, spatialElement) as OriginContextFragment;
            space.AddFragment(originContext);

            SpaceAnalyticalFragment spaceAnalytical = new SpaceAnalyticalFragment();
            spaceAnalytical = spaceAnalytical.UpdateValues(pullSettings, spatialElement) as SpaceAnalyticalFragment;
            space.AddFragment(spaceAnalytical);

            SpaceContextFragment spaceContext = new SpaceContextFragment();
            spaceContext = spaceContext.UpdateValues(pullSettings, spatialElement) as SpaceContextFragment;
            //TODO: Implement ConnectedElements
            space.AddFragment(spaceContext);

            //Set custom data
            space = Modify.SetIdentifiers(space, spatialElement) as Space;
            if (pullSettings.CopyCustomData)
                space = Modify.SetCustomData(space, spatialElement) as Space;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(space);
            space = space.UpdateValues(pullSettings, spatialElement) as Space;
            return space;
        }

        /***************************************************/

        public static Space ToBHoMSpace(this EnergyAnalysisSpace energyAnalysisSpace, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (energyAnalysisSpace == null)
                return new Space();

            settings = settings.DefaultIfNull();

            Space space = pullSettings.FindRefObject<Space>(energyAnalysisSpace.Id.IntegerValue);
            if (space != null)
                return space;

            SpatialElement spatialElement = Query.Element(energyAnalysisSpace.Document, energyAnalysisSpace.CADObjectUniqueId) as SpatialElement;

            string name = Query.Name(spatialElement);
            space = BH.Engine.Environment.Create.Space(name);

            if (spatialElement != null && spatialElement.Location != null)
                space.Location = (spatialElement.Location as LocationPoint).ToBHoM();

            //Set ExtendedProperties
            OriginContextFragment originContext = new OriginContextFragment();
            originContext.ElementID = spatialElement.Id.IntegerValue.ToString();
            originContext.TypeName = Query.Name(spatialElement);
            originContext = originContext.UpdateValues(pullSettings, energyAnalysisSpace) as OriginContextFragment;
            originContext = originContext.UpdateValues(pullSettings, spatialElement) as OriginContextFragment;
            space.AddFragment(originContext);

            SpaceAnalyticalFragment spaceAnalytical = new SpaceAnalyticalFragment();
            spaceAnalytical = spaceAnalytical.UpdateValues(pullSettings, energyAnalysisSpace) as SpaceAnalyticalFragment;
            spaceAnalytical = spaceAnalytical.UpdateValues(pullSettings, spatialElement) as SpaceAnalyticalFragment;
            space.AddFragment(spaceAnalytical);

            SpaceContextFragment spaceContext = new SpaceContextFragment();
            List<string> connectedElements = new List<string>();
            foreach (EnergyAnalysisSurface energyAnalysisSurface in energyAnalysisSpace.GetAnalyticalSurfaces())
                connectedElements.Add(energyAnalysisSurface.CADObjectUniqueId);
            spaceContext.ConnectedElements = connectedElements;
            spaceContext = spaceContext.UpdateValues(pullSettings, energyAnalysisSpace) as SpaceContextFragment;
            spaceContext = spaceContext.UpdateValues(pullSettings, spatialElement) as SpaceContextFragment;
            space.AddFragment(spaceContext);

            //Set custom data
            space = Modify.SetIdentifiers(space, spatialElement) as Space;
            if (pullSettings.CopyCustomData)
            {
                space = Modify.SetCustomData(space, spatialElement) as Space;
                double innerVolume = energyAnalysisSpace.InnerVolume.ToSI(UnitType.UT_Volume);
                double analyticalVolume = energyAnalysisSpace.AnalyticalVolume.ToSI(UnitType.UT_Volume);

                space = Modify.SetCustomData(space, "Inner Volume", innerVolume) as Space;
                space = Modify.SetCustomData(space, "Analytical Volume", analyticalVolume) as Space;
            }

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(space);
            space = space.UpdateValues(pullSettings, energyAnalysisSpace) as Space;
            space = space.UpdateValues(pullSettings, spatialElement) as Space;
            return space;
        }

        /***************************************************/
    }
}
