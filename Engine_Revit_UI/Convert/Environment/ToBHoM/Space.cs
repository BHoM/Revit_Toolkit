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

            return spatialElement.ToBHoMSpace(spatialElementBoundaryOptions, settings, refObjects);
        }

        /***************************************************/

        public static Space ToBHoMSpace(this SpatialElement spatialElement, SpatialElementBoundaryOptions spatialElementBoundaryOptions, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (spatialElement == null || spatialElementBoundaryOptions == null)
                return new Space();

            SpatialElementGeometryCalculator spatialElementGeometryCalculator = new SpatialElementGeometryCalculator(spatialElement.Document, spatialElementBoundaryOptions);

            return ToBHoMSpace(spatialElement, spatialElementGeometryCalculator, settings, refObjects);
        }

        /***************************************************/

        public static Space ToBHoMSpace(this SpatialElement spatialElement, SpatialElementGeometryCalculator spatialElementGeometryCalculator, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (spatialElement == null || spatialElementGeometryCalculator == null)
                return new Space();

            settings = settings.DefaultIfNull();

            Space space = refObjects.GetValue<Space>(spatialElement.Id);
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
            originContext.UpdateValues(settings, spatialElement);
            space.AddFragment(originContext);

            SpaceAnalyticalFragment spaceAnalytical = new SpaceAnalyticalFragment();
            spaceAnalytical.UpdateValues(settings, spatialElement);
            space.AddFragment(spaceAnalytical);

            SpaceContextFragment spaceContext = new SpaceContextFragment();
            spaceContext.UpdateValues(settings, spatialElement);
            
            //TODO: Implement ConnectedElements
            space.AddFragment(spaceContext);

            //Set identifiers & custom data
            space.SetIdentifiers(spatialElement);
            space.SetCustomData(spatialElement);

            refObjects.AddOrReplace(spatialElement.Id, space);
            space.UpdateValues(settings, spatialElement);
            return space;
        }

        /***************************************************/

        public static Space ToBHoMSpace(this EnergyAnalysisSpace energyAnalysisSpace, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (energyAnalysisSpace == null)
                return new Space();

            settings = settings.DefaultIfNull();

            Space space = refObjects.GetValue<Space>(energyAnalysisSpace.Id.IntegerValue);
            if (space != null)
                return space;

            SpatialElement spatialElement = energyAnalysisSpace.Document.Element(energyAnalysisSpace.CADObjectUniqueId) as SpatialElement;

            string name = Query.Name(spatialElement);
            space = BH.Engine.Environment.Create.Space(name);

            if (spatialElement != null && spatialElement.Location != null)
                space.Location = (spatialElement.Location as LocationPoint).ToBHoM();

            //Set ExtendedProperties
            OriginContextFragment originContext = new OriginContextFragment();
            originContext.ElementID = spatialElement.Id.IntegerValue.ToString();
            originContext.TypeName = Query.Name(spatialElement);
            originContext.UpdateValues(settings, energyAnalysisSpace);
            originContext.UpdateValues(settings, spatialElement);
            space.AddFragment(originContext);

            SpaceAnalyticalFragment spaceAnalytical = new SpaceAnalyticalFragment();
            spaceAnalytical.UpdateValues(settings, energyAnalysisSpace);
            spaceAnalytical.UpdateValues(settings, spatialElement);
            space.AddFragment(spaceAnalytical);

            SpaceContextFragment spaceContext = new SpaceContextFragment();
            List<string> connectedElements = new List<string>();
            foreach (EnergyAnalysisSurface energyAnalysisSurface in energyAnalysisSpace.GetAnalyticalSurfaces())
            {
                connectedElements.Add(energyAnalysisSurface.CADObjectUniqueId);
            }
            spaceContext.ConnectedElements = connectedElements;
            spaceContext.UpdateValues(settings, energyAnalysisSpace);
            spaceContext.UpdateValues(settings, spatialElement);
            space.AddFragment(spaceContext);

            //Set identifiers & custom data
            space.SetIdentifiers(spatialElement);
            space.SetCustomData(spatialElement);
                double innerVolume = energyAnalysisSpace.InnerVolume.ToSI(UnitType.UT_Volume);
                double analyticalVolume = energyAnalysisSpace.AnalyticalVolume.ToSI(UnitType.UT_Volume);

            space.CustomData["Inner Volume"] = innerVolume;
            space.CustomData["Analytical Volume"] = analyticalVolume;

            refObjects.AddOrReplace(energyAnalysisSpace.Id, space);
            space.UpdateValues(settings, energyAnalysisSpace);
            space.UpdateValues(settings, spatialElement);
            return space;
        }

        /***************************************************/
    }
}
