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
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Structure.Elements;
using System;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static FamilyInstance ToRevitFamilyInstance(this FramingElement framingElement, Document document, PushSettings pushSettings = null)
        {
            if (framingElement == null || document == null)
                return null;

            switch (framingElement.StructuralUsage)
            {
                case StructuralUsage1D.Column:
                    return framingElement.ToRevitFamilyInstance_Column(document, pushSettings);
                case StructuralUsage1D.Beam:
                case StructuralUsage1D.Brace:
                case StructuralUsage1D.Cable:
                    return framingElement.ToRevitFamilyInstance_Framing(document, pushSettings);
                case StructuralUsage1D.Pile:
                    BH.Engine.Reflection.Compute.RecordError(string.Format("Push of pile foundations is not supported in current version of BHoM. BHoM element Guid: {0}", framingElement.BHoM_Guid));
                    return null;
                default:
                    BH.Engine.Reflection.Compute.RecordWarning(string.Format("Structural usage type is not set. An attempt to create a structural framing element is being made. BHoM element Guid: {0}", framingElement.BHoM_Guid));
                    return framingElement.ToRevitFamilyInstance_Framing(document, pushSettings);
            }
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static FamilyInstance ToRevitFamilyInstance_Column(this FramingElement framingElement, Document document, PushSettings pushSettings = null)
        {
            if (framingElement == null || document == null)
                return null;

            FamilyInstance aFamilyInstance = pushSettings.FindRefObject<FamilyInstance>(document, framingElement.BHoM_Guid);
            if (aFamilyInstance != null)
                return aFamilyInstance;

            pushSettings.DefaultIfNull();

            object aCustomDataValue = null;

            Curve aCurve = framingElement.LocationCurve.ToRevitCurve(pushSettings);
            Level aLevel = null;

            aCustomDataValue = framingElement.CustomDataValue("Base Level");
            if (aCustomDataValue != null && aCustomDataValue is int)
            {
                ElementId aElementId = new ElementId((int)aCustomDataValue);
                aLevel = document.GetElement(aElementId) as Level;
            }

            if (aLevel == null)
                aLevel = Query.BottomLevel(framingElement.LocationCurve, document, pushSettings.ConvertUnits);

            FamilySymbol aFamilySymbol = framingElement.Property.ToRevitFamilySymbol_Column(document, pushSettings);

            if (aFamilySymbol == null)
            {
                aFamilySymbol = Query.ElementType(framingElement, document, BuiltInCategory.OST_StructuralColumns, pushSettings.FamilyLoadSettings) as FamilySymbol;

                if (aFamilySymbol == null)
                {
                    Compute.ElementTypeNotFoundWarning(framingElement);
                    return null;
                }
            }

            FamilyPlacementType aFamilyPlacementType = aFamilySymbol.Family.FamilyPlacementType;
            if (aFamilyPlacementType != FamilyPlacementType.CurveBased && aFamilyPlacementType != FamilyPlacementType.CurveBasedDetail && aFamilyPlacementType != FamilyPlacementType.CurveDrivenStructural && aFamilyPlacementType != FamilyPlacementType.TwoLevelsBased)
            {
                Compute.InvalidFamilyPlacementTypeWarning(framingElement, aFamilySymbol);
                return null;
            }

            aFamilyInstance = document.Create.NewFamilyInstance(aCurve, aFamilySymbol, aLevel, Autodesk.Revit.DB.Structure.StructuralType.Column);

            aFamilyInstance.CheckIfNullPush(framingElement);
            if (aFamilyInstance == null)
                return null;

            oM.Structure.FramingProperties.ConstantFramingElementProperty barProperty = framingElement.Property as oM.Structure.FramingProperties.ConstantFramingElementProperty;
            if (barProperty != null)
            {
                double orientationAngle = (Math.PI * 0.5 - barProperty.OrientationAngle) % (2 * Math.PI);
                Parameter aParameter = aFamilyInstance.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE);
                if(aParameter != null && !aParameter.IsReadOnly)
                    aParameter.Set(orientationAngle);
            }

            if (pushSettings.CopyCustomData)
            {
                BuiltInParameter[] paramsToIgnore = new BuiltInParameter[]
                {
                    //BuiltInParameter.SCHEDULE_BASE_LEVEL_PARAM,
                    //BuiltInParameter.FAMILY_BASE_LEVEL_PARAM,
                    BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM,
                    BuiltInParameter.SCHEDULE_BASE_LEVEL_OFFSET_PARAM,
                    BuiltInParameter.SLANTED_COLUMN_TYPE_PARAM,
                    BuiltInParameter.ELEM_FAMILY_PARAM,
                    BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM,
                    BuiltInParameter.ALL_MODEL_IMAGE,
                    //BuiltInParameter.SCHEDULE_LEVEL_PARAM,
                    //BuiltInParameter.FAMILY_TOP_LEVEL_PARAM,
                    //BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM,
                    BuiltInParameter.SCHEDULE_TOP_LEVEL_OFFSET_PARAM,
                    BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM,
                    BuiltInParameter.ELEM_TYPE_PARAM
                };
                Modify.SetParameters(aFamilyInstance, framingElement, paramsToIgnore, pushSettings.ConvertUnits);
            }

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(framingElement, aFamilyInstance);

            return aFamilyInstance;
        }

        /***************************************************/

        private static FamilyInstance ToRevitFamilyInstance_Framing(this FramingElement framingElement, Document document, PushSettings pushSettings = null)
        {
            if (framingElement == null || document == null)
                return null;

            FamilyInstance aFamilyInstance = pushSettings.FindRefObject<FamilyInstance>(document, framingElement.BHoM_Guid);
            if (aFamilyInstance != null)
                return aFamilyInstance;

            pushSettings.DefaultIfNull();

            object aCustomDataValue = null;

            Curve aCurve = framingElement.LocationCurve.ToRevitCurve(pushSettings);
            Level aLevel = null;

            aCustomDataValue = framingElement.CustomDataValue("Reference Level");
            if (aCustomDataValue != null && aCustomDataValue is int)
            {
                ElementId aElementId = new ElementId((int)aCustomDataValue);
                aLevel = document.GetElement(aElementId) as Level;
            }

            if (aLevel == null)
                aLevel = Query.BottomLevel(framingElement.LocationCurve, document, pushSettings.ConvertUnits);

            FamilySymbol aFamilySymbol = framingElement.Property.ToRevitFamilySymbol_Framing(document, pushSettings);

            if (aFamilySymbol == null)
            {
                aFamilySymbol = Query.ElementType(framingElement, document, BuiltInCategory.OST_StructuralColumns, pushSettings.FamilyLoadSettings) as FamilySymbol;

                if (aFamilySymbol == null)
                {
                    Compute.ElementTypeNotFoundWarning(framingElement);
                    return null;
                }
            }

            FamilyPlacementType aFamilyPlacementType = aFamilySymbol.Family.FamilyPlacementType;
            if (aFamilyPlacementType != FamilyPlacementType.CurveBased && aFamilyPlacementType != FamilyPlacementType.CurveBasedDetail && aFamilyPlacementType != FamilyPlacementType.CurveDrivenStructural && aFamilyPlacementType != FamilyPlacementType.TwoLevelsBased)
            {
                Compute.InvalidFamilyPlacementTypeWarning(framingElement, aFamilySymbol);
                return null;
            }

            switch (framingElement.StructuralUsage)
            {
                case StructuralUsage1D.Beam:
                    aFamilyInstance = document.Create.NewFamilyInstance(aCurve, aFamilySymbol, aLevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                    break;
                case StructuralUsage1D.Brace:
                    aFamilyInstance = document.Create.NewFamilyInstance(aCurve, aFamilySymbol, aLevel, Autodesk.Revit.DB.Structure.StructuralType.Brace);
                    break;
                case StructuralUsage1D.Cable:
                    aFamilyInstance = document.Create.NewFamilyInstance(aCurve, aFamilySymbol, aLevel, Autodesk.Revit.DB.Structure.StructuralType.Brace);
                    break;
                default:
                    aFamilyInstance = document.Create.NewFamilyInstance(aCurve, aFamilySymbol, aLevel, Autodesk.Revit.DB.Structure.StructuralType.UnknownFraming);
                    break;
            }

            aFamilyInstance.CheckIfNullPush(framingElement);
            if (aFamilyInstance == null)
                return null;

            if (pushSettings.CopyCustomData)
            {
                BuiltInParameter[] paramsToIgnore = new BuiltInParameter[]
                {
                    BuiltInParameter.STRUCTURAL_BEAM_END0_ELEVATION,
                    BuiltInParameter.STRUCTURAL_BEAM_END1_ELEVATION,
                    BuiltInParameter.ELEM_FAMILY_PARAM,
                    BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM,
                    BuiltInParameter.ALL_MODEL_IMAGE,
                    BuiltInParameter.ELEM_TYPE_PARAM
                };
                Modify.SetParameters(aFamilyInstance, framingElement, paramsToIgnore, pushSettings.ConvertUnits);
            }

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(framingElement, aFamilyInstance);

            return aFamilyInstance;
        }

        /***************************************************/
    }
}