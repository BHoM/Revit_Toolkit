/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Environment.Elements;
using BH.oM.Environment.Fragments;
using BH.oM.Geometry;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BH.Engine.Base;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit SpatialElement to BH.oM.Environment.Elements.Space.")]
        [Input("spatialElement", "Revit SpatialElement to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("space", "BH.oM.Environment.Elements.Space resulting from converting the input Revit SpatialElement.")]
        public static Space SpaceFromRevit(this SpatialElement spatialElement, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            SpatialElementBoundaryOptions spatialElementBoundaryOptions = new SpatialElementBoundaryOptions();
            spatialElementBoundaryOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
            spatialElementBoundaryOptions.StoreFreeBoundaryFaces = false;

            return spatialElement.SpaceFromRevit(spatialElementBoundaryOptions, settings, refObjects);
        }

        /***************************************************/

        [Description("Converts a Revit SpatialElement to BH.oM.Environment.Elements.Space.")]
        [Input("spatialElement", "Revit SpatialElement to be converted.")]
        [Input("spatialElementBoundaryOptions", "Revit SpatialElementBoundaryOptions while performing the convert.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("space", "BH.oM.Environment.Elements.Space resulting from converting the input Revit SpatialElement.")]
        public static Space SpaceFromRevit(this SpatialElement spatialElement, SpatialElementBoundaryOptions spatialElementBoundaryOptions, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (spatialElement == null || spatialElementBoundaryOptions == null)
                return new Space();

            SpatialElementGeometryCalculator spatialElementGeometryCalculator = new SpatialElementGeometryCalculator(spatialElement.Document, spatialElementBoundaryOptions);

            return SpaceFromRevit(spatialElement, spatialElementGeometryCalculator, settings, refObjects);
        }

        /***************************************************/

        [Description("Converts a Revit SpatialElement to BH.oM.Environment.Elements.Space.")]
        [Input("spatialElement", "Revit SpatialElement to be converted.")]
        [Input("spatialElementGeometryCalculator", "Revit SpatialElementGeometryCalculator while performing the convert.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("space", "BH.oM.Environment.Elements.Space resulting from converting the input Revit SpatialElement.")]
        public static Space SpaceFromRevit(this SpatialElement spatialElement, SpatialElementGeometryCalculator spatialElementGeometryCalculator, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (spatialElement == null || spatialElementGeometryCalculator == null)
                return new Space();

            settings = settings.DefaultIfNull();

            Space space = refObjects.GetValue<Space>(spatialElement.Id);
            if (space != null)
                return space;

            //Create the Space
            space = new Space();
            space.Name = Query.Name(spatialElement);

            PolyCurve pcurve = spatialElement.Perimeter(settings).FirstOrDefault();

            if (pcurve != null)
            {
                space.Location = pcurve.Centroid();
                space.Perimeter = pcurve;
            }
            else if (spatialElement.Location != null && spatialElement.Location is LocationPoint)
                space.Location = ((LocationPoint)spatialElement.Location).FromRevit();

            //Set ExtendedProperties
            OriginContextFragment originContext = new OriginContextFragment() { ElementID = spatialElement.Id.IntegerValue.ToString(), TypeName = Query.Name(spatialElement) };
            originContext.SetProperties(spatialElement, settings.MappingSettings);
            space.AddFragment(originContext);

            SpaceAnalyticalFragment spaceAnalytical = new SpaceAnalyticalFragment();
            spaceAnalytical.SetProperties(spatialElement, settings.MappingSettings);
            space.AddFragment(spaceAnalytical);

            SpaceContextFragment spaceContext = new SpaceContextFragment();
            spaceContext.SetProperties(spatialElement, settings.MappingSettings);
            
            //TODO: Implement ConnectedElements
            space.AddFragment(spaceContext);

            //Set type
            spatialElement.CopySpatialElementTypeToFragment(space, settings, refObjects);

            //Set identifiers, parameters & custom data
            space.SetIdentifiers(spatialElement);
            space.CopyParameters(spatialElement, settings.MappingSettings);
            space.SetProperties(spatialElement, settings.MappingSettings);

            refObjects.AddOrReplace(spatialElement.Id, space);
            return space;
        }

        /***************************************************/

        [Description("Converts a Revit EnergyAnalysisSpace to BH.oM.Environment.Elements.Space.")]
        [Input("energyAnalysisSpace", "Revit EnergyAnalysisSpace to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("space", "BH.oM.Environment.Elements.Space resulting from converting the input Revit EnergyAnalysisSpace.")]
        public static Space SpaceFromRevit(this EnergyAnalysisSpace energyAnalysisSpace, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (energyAnalysisSpace == null)
                return new Space();

            settings = settings.DefaultIfNull();

            Space space = refObjects.GetValue<Space>(energyAnalysisSpace.Id.IntegerValue);
            if (space != null)
                return space;

            SpatialElement spatialElement = energyAnalysisSpace.Document.Element(energyAnalysisSpace.CADObjectUniqueId) as SpatialElement;

            if (spatialElement == null)
                return space; //If we can't get data from it let's not waste our time

            space = new Space();
            space.Name = Query.Name(spatialElement);

            Polyline pline = energyAnalysisSpace.GetBoundary().Select(x => x.FromRevit()).ToList().Join().FirstOrDefault();

            if (pline != null)
            {
                space.Location = pline.Centroid();
                space.Perimeter = pline;
            }
            else if (spatialElement != null && spatialElement.Location != null)
                space.Location = (spatialElement.Location as LocationPoint).FromRevit();

            //Set ExtendedProperties
            OriginContextFragment originContext = new OriginContextFragment() { ElementID = spatialElement.Id.IntegerValue.ToString(), TypeName = Query.Name(spatialElement) };
            originContext.SetProperties(energyAnalysisSpace, settings.MappingSettings);
            originContext.SetProperties(spatialElement, settings.MappingSettings);
            space.AddFragment(originContext);

            SpaceAnalyticalFragment spaceAnalytical = new SpaceAnalyticalFragment();
            spaceAnalytical.SetProperties(energyAnalysisSpace, settings.MappingSettings);
            spaceAnalytical.SetProperties(spatialElement, settings.MappingSettings);
            space.AddFragment(spaceAnalytical);

            SpaceContextFragment spaceContext = new SpaceContextFragment();
            List<string> connectedElements = new List<string>();
            foreach (EnergyAnalysisSurface energyAnalysisSurface in energyAnalysisSpace.GetAnalyticalSurfaces())
                connectedElements.Add(energyAnalysisSurface.CADObjectUniqueId);

            spaceContext.ConnectedElements = connectedElements;
            spaceContext.SetProperties(energyAnalysisSpace, settings.MappingSettings);
            spaceContext.SetProperties(spatialElement, settings.MappingSettings);
            space.AddFragment(spaceContext);

            //Set type
            spatialElement.CopyTypeToFragment(space, settings, refObjects);

            //Set identifiers, parameters & custom data
            space.SetIdentifiers(spatialElement);
            space.CopyParameters(spatialElement, settings.MappingSettings);
            space.SetProperties(spatialElement, settings.MappingSettings);
            space.SetProperties(energyAnalysisSpace, settings.MappingSettings);

            refObjects.AddOrReplace(energyAnalysisSpace.Id, space);
            return space;
        }

        /***************************************************/
    }
}





