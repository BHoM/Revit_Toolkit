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
using BH.oM.Environment.Properties;
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
                aResult = Create.Opening(aCurve);

                EnvironmentContextProperties aEnvironmentContextProperties = new EnvironmentContextProperties();
                aEnvironmentContextProperties.ElementID = energyAnalysisOpening.Id.IntegerValue.ToString();
                aEnvironmentContextProperties.TypeName = energyAnalysisOpening.OpeningName;
                aEnvironmentContextProperties = aEnvironmentContextProperties.UpdateValues(pullSettings, energyAnalysisOpening) as EnvironmentContextProperties;
                aResult.AddExtendedProperty(aEnvironmentContextProperties);

                ElementProperties aElementProperties = new ElementProperties();
                aElementProperties.Construction = Query.Construction(energyAnalysisOpening, pullSettings);
                aElementProperties.BuildingElementType = BuildingElementType.Undefined;
                aElementProperties = aElementProperties.UpdateValues(pullSettings, energyAnalysisOpening) as ElementProperties;
                aResult.AddExtendedProperty(aElementProperties);

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
                aResult = Create.Opening(aCurve);
                aResult.Name = Query.FamilyTypeFullName(aElement);

                EnvironmentContextProperties aEnvironmentContextProperties = new EnvironmentContextProperties();
                aEnvironmentContextProperties.ElementID = aElement.Id.IntegerValue.ToString();
                aEnvironmentContextProperties.TypeName = Query.FamilyTypeFullName(aElement);
                aEnvironmentContextProperties = aEnvironmentContextProperties.UpdateValues(pullSettings, aElement) as EnvironmentContextProperties;
                aEnvironmentContextProperties = aEnvironmentContextProperties.UpdateValues(pullSettings, aElementType) as EnvironmentContextProperties;
                aResult.AddExtendedProperty(aEnvironmentContextProperties);

                ElementProperties aElementProperties = new ElementProperties();
                aElementProperties.Construction = Query.Construction(energyAnalysisOpening, pullSettings);
                BuildingElementType? aBuildingElementType = Query.BuildingElementType(aElement.Category);
                if (aBuildingElementType.HasValue)
                    aElementProperties.BuildingElementType = aBuildingElementType.Value;
                else
                    aElementProperties.BuildingElementType = BuildingElementType.Undefined;
                aElementProperties = aElementProperties.UpdateValues(pullSettings, aElement) as ElementProperties;
                aElementProperties = aElementProperties.UpdateValues(pullSettings, aElementType) as ElementProperties;
                aResult.AddExtendedProperty(aElementProperties);

                aResult = Modify.SetIdentifiers(aResult, aElement) as oM.Environment.Elements.Opening;
                if (pullSettings.CopyCustomData)
                    aResult = Modify.SetCustomData(aResult, aElement, pullSettings.ConvertUnits) as oM.Environment.Elements.Opening;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aResult, energyAnalysisOpening.Id.IntegerValue);
                aResult = aResult.UpdateValues(pullSettings, aElement) as oM.Environment.Elements.Opening;
                aResult = aResult.UpdateValues(pullSettings, aElementType) as oM.Environment.Elements.Opening;
                return aResult;
            }
        }

        /***************************************************/
    }
}