/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using BH.oM.Geometry;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit EnergyAnalysisOpening to BH.oM.Environment.Elements.Opening.")]
        [Input("energyAnalysisOpening", "Revit EnergyAnalysisOpening to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("opening", "BH.oM.Environment.Elements.Opening resulting from converting the input Revit EnergyAnalysisOpening.")]
        public static oM.Environment.Elements.Opening OpeningFromRevit(this EnergyAnalysisOpening energyAnalysisOpening, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            Element element = energyAnalysisOpening.Element();
            if (element == null)
            {
                oM.Environment.Elements.Opening result = refObjects.GetValue<oM.Environment.Elements.Opening>(energyAnalysisOpening.Id);
                if (result != null)
                    return result;

                ICurve curve = energyAnalysisOpening.GetPolyloop().FromRevit();
                result = new oM.Environment.Elements.Opening()
                {
                    Edges = curve.ToEdges(),
                };

                OriginContextFragment originContext = new OriginContextFragment() { ElementID = energyAnalysisOpening.Id.IntegerValue.ToString(), TypeName = energyAnalysisOpening.OpeningName };
                originContext.SetProperties(energyAnalysisOpening, settings.MappingSettings);
                result.AddFragment(originContext);

                result.OpeningConstruction = energyAnalysisOpening.Construction(settings);
                result.Type = OpeningType.Undefined;

                //Set identifiers, parameters & custom data
                result.SetIdentifiers(energyAnalysisOpening);
                result.CopyParameters(energyAnalysisOpening, settings.MappingSettings);
                result.SetProperties(energyAnalysisOpening, settings.MappingSettings);

                refObjects.AddOrReplace(energyAnalysisOpening.Id, result);
                return result;
            }
            else
            {
                oM.Environment.Elements.Opening result = refObjects.GetValue<oM.Environment.Elements.Opening>(energyAnalysisOpening.Id.IntegerValue);
                if (result != null)
                    return result;

                ElementType elementType = element.Document.GetElement(element.GetTypeId()) as ElementType;

                ICurve curve = energyAnalysisOpening.GetPolyloop().FromRevit();
                result = new oM.Environment.Elements.Opening()
                {
                    Edges = curve.ToEdges(),
                    Name = element.FamilyTypeFullName(),
                };

                OriginContextFragment originContext = new OriginContextFragment() { ElementID = element.Id.IntegerValue.ToString(), TypeName = element.FamilyTypeFullName() };
                originContext.SetProperties(element, settings.MappingSettings);
                originContext.SetProperties(elementType, settings.MappingSettings);
                result.AddFragment(originContext);

                result.OpeningConstruction = energyAnalysisOpening.Construction(settings);

                OpeningType? openingType = element.Category.OpeningType();
                if (openingType.HasValue)
                    result.Type = openingType.Value;
                else
                    result.Type = OpeningType.Undefined;

                //Set identifiers, parameters & custom data
                result.SetIdentifiers(element);
                result.CopyParameters(element, settings.MappingSettings);
                result.SetProperties(element, settings.MappingSettings);

                refObjects.AddOrReplace(energyAnalysisOpening.Id, result);
                return result;
            }
        }

        /***************************************************/
    }
}


