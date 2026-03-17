/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Physical.Elements;
using BH.oM.Physical.FramingProperties;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit FamilyInstance to BH.oM.Physical.Elements.PadFoundation.")]
        [Input("familyInstance", "Revit FamilyInstance to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("padFoundation", "BH.oM.Physical.Elements.PadFoundation resulting from converting the input Revit FamilyInstance.")]
        public static PadFoundation PadFoundationFromRevit(this FamilyInstance familyInstance, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            PadFoundation padFoundation = refObjects.GetValue<PadFoundation>(familyInstance.Id);
            if (padFoundation != null)
                return padFoundation;

            oM.Geometry.ICurve locationCurve = familyInstance.LocationCurvePadFoundation(settings);
            if (locationCurve == null)
            {
                BH.Engine.Base.Compute.RecordError($"Failed to extract geometry from pad foundation. ElementId: {familyInstance.Id.Value()}");
                return null;
            }

            oM.Geometry.PlanarSurface planarSurface = null;

            if (locationCurve is oM.Geometry.PlanarSurface ps)
            {
                planarSurface = ps;
            }
            else if (locationCurve is oM.Geometry.Polyline polyline)
            {
                planarSurface = BH.Engine.Geometry.Create.PlanarSurface(polyline);
            }
            else
            {
                BH.Engine.Base.Compute.RecordError(
                    $"Unsupported geometry type for PadFoundation: {locationCurve.GetType().Name}"
                );
                return null;
            }

            BoundingBoxXYZ bbox = familyInstance.get_BoundingBox(null);
            double depth = bbox.Max.Z - bbox.Min.Z;

            FamilySymbol familySymbol = familyInstance.Document.GetElement(familyInstance.GetTypeId()) as FamilySymbol;
            oM.Physical.Constructions.IConstruction construction = familySymbol?.ConstructionFromRevit(settings, refObjects);

            padFoundation = BH.Engine.Physical.Create.PadFoundation(planarSurface, construction, familyInstance.FamilyTypeFullName());

            //Set identifiers, parameters & custom data
            padFoundation.SetIdentifiers(familyInstance);
            padFoundation.CopyParameters(familyInstance, settings.MappingSettings);
            padFoundation.SetProperties(familyInstance, settings.MappingSettings);

            refObjects.AddOrReplace(familyInstance.Id, padFoundation);
            return padFoundation;
        }

        /***************************************************/
    }
} 
