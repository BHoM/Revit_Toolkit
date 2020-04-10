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

using BH.oM.Base;
using BH.Engine.Environment;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using BH.oM.Environment.Fragments;
using BH.oM.Environment.Elements;

using System.Collections.Generic;
using BH.Engine.Adapters.Revit;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

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
                result = BH.Engine.Environment.Create.Opening(externalEdges: curve.ToEdges());

                OriginContextFragment originContext = new OriginContextFragment() { ElementID = energyAnalysisOpening.Id.IntegerValue.ToString(), TypeName = energyAnalysisOpening.OpeningName };
                originContext.UpdateValues(settings, energyAnalysisOpening);
                result.AddFragment(originContext);

                result.OpeningConstruction = energyAnalysisOpening.Construction(settings);
                result.Type = OpeningType.Undefined;

                //Set identifiers & custom data
                result.SetIdentifiers(energyAnalysisOpening);
                result.SetCustomData(energyAnalysisOpening);

                refObjects.AddOrReplace(energyAnalysisOpening.Id, result);
                result.UpdateValues(settings, energyAnalysisOpening);
                return result;
            }
            else
            {
                oM.Environment.Elements.Opening result = refObjects.GetValue<oM.Environment.Elements.Opening>(energyAnalysisOpening.Id.IntegerValue);
                if (result != null)
                    return result;

                ElementType elementType = element.Document.GetElement(element.GetTypeId()) as ElementType;

                ICurve curve = energyAnalysisOpening.GetPolyloop().FromRevit();
                result = BH.Engine.Environment.Create.Opening(externalEdges: curve.ToEdges());
                result.Name = element.FamilyTypeFullName();

                OriginContextFragment originContext = new OriginContextFragment() { ElementID = element.Id.IntegerValue.ToString(), TypeName = element.FamilyTypeFullName() };
                originContext.UpdateValues(settings, element);
                originContext.UpdateValues(settings, elementType);
                result.AddFragment(originContext);

                result.OpeningConstruction = energyAnalysisOpening.Construction(settings);

                OpeningType? openingType = element.Category.OpeningType();
                if (openingType.HasValue)
                    result.Type = openingType.Value;
                else
                    result.Type = OpeningType.Undefined;

                //Set identifiers & custom data
                result.SetIdentifiers(element);
                result.SetCustomData(element);

                if (elementType != null)
                    result.SetCustomData(elementType, "Type ");

                refObjects.AddOrReplace(energyAnalysisOpening.Id, result);
                result.UpdateValues(settings, element);
                result.UpdateValues(settings, elementType);
                return result;
            }
        }

        /***************************************************/
    }
}
