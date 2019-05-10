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
        /****             Internal methods              ****/
        /***************************************************/

        internal static oM.Environment.Elements.Opening ToBHoMOpening(this EnergyAnalysisOpening energyAnalysisOpening, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            Element aElement = energyAnalysisOpening.Element();
            if (aElement == null)
            {
                oM.Environment.Elements.Opening aResult = pullSettings.FindRefObject<oM.Environment.Elements.Opening>(energyAnalysisOpening.Id.IntegerValue);
                if (aResult != null)
                    return aResult;

                ICurve aCurve = energyAnalysisOpening.GetPolyloop().ToBHoM(pullSettings);
                aResult = Create.Opening(externalEdges: aCurve.ToEdges());

                OriginContextFragment aOriginContextFragment = new OriginContextFragment();
                aOriginContextFragment.ElementID = energyAnalysisOpening.Id.IntegerValue.ToString();
                aOriginContextFragment.TypeName = energyAnalysisOpening.OpeningName;
                aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, energyAnalysisOpening) as OriginContextFragment;
                aResult.AddFragment(aOriginContextFragment);

                aResult.OpeningConstruction = Query.Construction(energyAnalysisOpening, pullSettings);
                aResult.Type = OpeningType.Undefined;

                aResult = Modify.SetIdentifiers(aResult, energyAnalysisOpening) as oM.Environment.Elements.Opening;
                if (pullSettings.CopyCustomData)
                    aResult = Modify.SetCustomData(aResult, energyAnalysisOpening, pullSettings.ConvertUnits) as oM.Environment.Elements.Opening;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aResult);
                aResult = aResult.UpdateValues(pullSettings, energyAnalysisOpening) as oM.Environment.Elements.Opening;
                return aResult;
            }
            else
            {
                oM.Environment.Elements.Opening aResult = pullSettings.FindRefObject<oM.Environment.Elements.Opening>(energyAnalysisOpening.Id.IntegerValue);
                if (aResult != null)
                    return aResult;

                ElementType aElementType = aElement.Document.GetElement(aElement.GetTypeId()) as ElementType;

                ICurve aCurve = energyAnalysisOpening.GetPolyloop().ToBHoM(pullSettings);
                aResult = Create.Opening(externalEdges: aCurve.ToEdges());
                aResult.Name = Query.FamilyTypeFullName(aElement);

                OriginContextFragment aOriginContextFragment = new OriginContextFragment();
                aOriginContextFragment.ElementID = aElement.Id.IntegerValue.ToString();
                aOriginContextFragment.TypeName = Query.FamilyTypeFullName(aElement);
                aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, aElement) as OriginContextFragment;
                aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, aElementType) as OriginContextFragment;
                aResult.AddFragment(aOriginContextFragment);

                aResult.OpeningConstruction = Query.Construction(energyAnalysisOpening, pullSettings);

                OpeningType? aBuildingElementType = Query.OpeningType(aElement.Category);
                if (aBuildingElementType.HasValue)
                    aResult.Type = aBuildingElementType.Value;
                else
                    aResult.Type = OpeningType.Undefined;

                aResult = Modify.SetIdentifiers(aResult, aElement) as oM.Environment.Elements.Opening;
                if (pullSettings.CopyCustomData)
                    aResult = Modify.SetCustomData(aResult, aElement, pullSettings.ConvertUnits) as oM.Environment.Elements.Opening;

                if (aElementType != null)
                    aResult = Modify.SetCustomData(aResult, aElementType, pullSettings.ConvertUnits, "Type ") as oM.Environment.Elements.Opening;



                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aResult, energyAnalysisOpening.Id.IntegerValue);
                aResult = aResult.UpdateValues(pullSettings, aElement) as oM.Environment.Elements.Opening;
                aResult = aResult.UpdateValues(pullSettings, aElementType) as oM.Environment.Elements.Opening;
                return aResult;
            }
        }

        /***************************************************/
    }
}