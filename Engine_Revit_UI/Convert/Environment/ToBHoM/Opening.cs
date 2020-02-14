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

        public static oM.Environment.Elements.Opening ToBHoMOpening(this EnergyAnalysisOpening energyAnalysisOpening, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            Element element = energyAnalysisOpening.Element();
            if (element == null)
            {
                oM.Environment.Elements.Opening result = refObjects.GetValue<oM.Environment.Elements.Opening>(energyAnalysisOpening.Id);
                if (result != null)
                    return result;

                ICurve curve = energyAnalysisOpening.GetPolyloop().ToBHoM();
                result = BH.Engine.Environment.Create.Opening(externalEdges: curve.ToEdges());

                OriginContextFragment originContext = new OriginContextFragment();
                originContext.ElementID = energyAnalysisOpening.Id.IntegerValue.ToString();
                originContext.TypeName = energyAnalysisOpening.OpeningName;
                originContext = originContext.UpdateValues(settings, energyAnalysisOpening) as OriginContextFragment;
                result.AddFragment(originContext);

                result.OpeningConstruction = energyAnalysisOpening.Construction(settings);
                result.Type = OpeningType.Undefined;

                //Set identifiers & custom data
                result = result.SetIdentifiers(energyAnalysisOpening) as oM.Environment.Elements.Opening;
                result = result.SetCustomData(energyAnalysisOpening) as oM.Environment.Elements.Opening;

                refObjects.AddOrReplace(energyAnalysisOpening.Id, result);
                result = result.UpdateValues(settings, energyAnalysisOpening) as oM.Environment.Elements.Opening;
                return result;
            }
            else
            {
                oM.Environment.Elements.Opening result = refObjects.GetValue<oM.Environment.Elements.Opening>(energyAnalysisOpening.Id.IntegerValue);
                if (result != null)
                    return result;

                ElementType elementType = element.Document.GetElement(element.GetTypeId()) as ElementType;

                ICurve curve = energyAnalysisOpening.GetPolyloop().ToBHoM();
                result = BH.Engine.Environment.Create.Opening(externalEdges: curve.ToEdges());
                result.Name = element.FamilyTypeFullName();

                OriginContextFragment originContext = new OriginContextFragment();
                originContext.ElementID = element.Id.IntegerValue.ToString();
                originContext.TypeName = element.FamilyTypeFullName();
                originContext = originContext.UpdateValues(settings, element) as OriginContextFragment;
                originContext = originContext.UpdateValues(settings, elementType) as OriginContextFragment;
                result.AddFragment(originContext);

                result.OpeningConstruction = energyAnalysisOpening.Construction(settings);

                OpeningType? openingType = element.Category.OpeningType();
                if (openingType.HasValue)
                    result.Type = openingType.Value;
                else
                    result.Type = OpeningType.Undefined;

                //Set identifiers & custom data
                result = result.SetIdentifiers(element) as oM.Environment.Elements.Opening;
                result = Modify.SetCustomData(result, element) as oM.Environment.Elements.Opening;

                if (elementType != null)
                    result = result.SetCustomData(elementType, "Type ") as oM.Environment.Elements.Opening;

                refObjects.AddOrReplace(energyAnalysisOpening.Id, result);
                result = result.UpdateValues(settings, element) as oM.Environment.Elements.Opening;
                result = result.UpdateValues(settings, elementType) as oM.Environment.Elements.Opening;
                return result;
            }
        }

        /***************************************************/
    }
}
