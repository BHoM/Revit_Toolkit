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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using BH.oM.Environment.Fragments;
using BH.oM.Environment.Elements;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static oM.Environment.Elements.Opening ToBHoMOpening(this EnergyAnalysisOpening energyAnalysisOpening, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            Element element = energyAnalysisOpening.Element();
            if (element == null)
            {
                oM.Environment.Elements.Opening result = pullSettings.FindRefObject<oM.Environment.Elements.Opening>(energyAnalysisOpening.Id.IntegerValue);
                if (result != null)
                    return result;

                ICurve curve = energyAnalysisOpening.GetPolyloop().ToBHoM();
                result = Create.Opening(externalEdges: curve.ToEdges());

                OriginContextFragment originContext = new OriginContextFragment();
                originContext.ElementID = energyAnalysisOpening.Id.IntegerValue.ToString();
                originContext.TypeName = energyAnalysisOpening.OpeningName;
                originContext = originContext.UpdateValues(pullSettings, energyAnalysisOpening) as OriginContextFragment;
                result.AddFragment(originContext);

                result.OpeningConstruction = Query.Construction(energyAnalysisOpening, pullSettings);
                result.Type = OpeningType.Undefined;

                result = Modify.SetIdentifiers(result, energyAnalysisOpening) as oM.Environment.Elements.Opening;
                if (pullSettings.CopyCustomData)
                    result = Modify.SetCustomData(result, energyAnalysisOpening) as oM.Environment.Elements.Opening;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(result);
                result = result.UpdateValues(pullSettings, energyAnalysisOpening) as oM.Environment.Elements.Opening;
                return result;
            }
            else
            {
                oM.Environment.Elements.Opening result = pullSettings.FindRefObject<oM.Environment.Elements.Opening>(energyAnalysisOpening.Id.IntegerValue);
                if (result != null)
                    return result;

                ElementType elementType = element.Document.GetElement(element.GetTypeId()) as ElementType;

                ICurve curve = energyAnalysisOpening.GetPolyloop().ToBHoM();
                result = Create.Opening(externalEdges: curve.ToEdges());
                result .Name = Query.FamilyTypeFullName(element);

                OriginContextFragment originContext = new OriginContextFragment();
                originContext.ElementID = element.Id.IntegerValue.ToString();
                originContext.TypeName = Query.FamilyTypeFullName(element);
                originContext = originContext.UpdateValues(pullSettings, element) as OriginContextFragment;
                originContext = originContext.UpdateValues(pullSettings, elementType) as OriginContextFragment;
                result.AddFragment(originContext);

                result.OpeningConstruction = Query.Construction(energyAnalysisOpening, pullSettings);

                OpeningType? openingType = Query.OpeningType(element.Category);
                if (openingType.HasValue)
                    result.Type = openingType.Value;
                else
                    result.Type = OpeningType.Undefined;

                result = Modify.SetIdentifiers(result, element) as oM.Environment.Elements.Opening;
                if (pullSettings.CopyCustomData)
                    result = Modify.SetCustomData(result, element) as oM.Environment.Elements.Opening;

                if (elementType != null)
                    result = Modify.SetCustomData(result, elementType, "Type ") as oM.Environment.Elements.Opening;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(result, energyAnalysisOpening.Id.IntegerValue);
                result = result.UpdateValues(pullSettings, element) as oM.Environment.Elements.Opening;
                result = result.UpdateValues(pullSettings, elementType) as oM.Environment.Elements.Opening;
                return result;
            }
        }

        /***************************************************/
    }
}