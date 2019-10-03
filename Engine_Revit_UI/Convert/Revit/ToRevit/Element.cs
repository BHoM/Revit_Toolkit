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

using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Internal Methods                          ****/
        /***************************************************/

        internal static Element ToRevitElement(this ModelInstance modelInstance, Document document, PushSettings pushSettings = null)
        {
            Element aElement = pushSettings.FindRefObject<Element>(document, modelInstance.BHoM_Guid);
            if (aElement != null)
                return aElement;

            if(modelInstance.Properties == null)
            {
                Compute.ElementTypeNotFoundWarning(modelInstance);
                return null;
            }

            pushSettings.DefaultIfNull();

            BuiltInCategory aBuiltInCategory = modelInstance.Properties.BuiltInCategory(document, pushSettings.FamilyLoadSettings);

            ElementType aElementType = modelInstance.Properties.ElementType(document, aBuiltInCategory, pushSettings.FamilyLoadSettings);
            if (aElementType == null)
            {
                Compute.ElementTypeNotFoundWarning(modelInstance);
                return null;
            }

            if (aElementType is FamilySymbol)
            {
                FamilySymbol aFamilySymbol = (FamilySymbol)aElementType;
                FamilyPlacementType aFamilyPlacementType = aFamilySymbol.Family.FamilyPlacementType;

                IGeometry aIGeometry = modelInstance.Location;

                if(aFamilyPlacementType == FamilyPlacementType.Invalid || 
                    aFamilyPlacementType == FamilyPlacementType.Adaptive ||
                    (aIGeometry is oM.Geometry.Point && (aFamilyPlacementType == FamilyPlacementType.CurveBased || aFamilyPlacementType == FamilyPlacementType.CurveBasedDetail || aFamilyPlacementType == FamilyPlacementType.CurveDrivenStructural)) || 
                    (aIGeometry is ICurve && (aFamilyPlacementType == FamilyPlacementType.OneLevelBased || aFamilyPlacementType == FamilyPlacementType.OneLevelBasedHosted)))
                {
                    Compute.InvalidFamilyPlacementTypeWarning(modelInstance, aElementType);
                    return null;
                }

                switch (aFamilyPlacementType)
                {
                    //TODO: Implement rest of the cases
                    case FamilyPlacementType.CurveBased:
                        aElement = ToRevitElement_CurveBased(modelInstance, aFamilySymbol, pushSettings);
                        break;
                    case FamilyPlacementType.OneLevelBased:
                        aElement = ToRevitElement_OneLevelBased(modelInstance, aFamilySymbol, pushSettings);
                        break;
                    case FamilyPlacementType.CurveDrivenStructural:
                        aElement = ToRevitElement_CurveDrivenStructural(modelInstance, aFamilySymbol, pushSettings);
                        break;
                    case FamilyPlacementType.TwoLevelsBased:
                        aElement = ToRevitElement_TwoLevelsBased(modelInstance, aFamilySymbol, pushSettings);
                        break;
                    default:
                        Compute.FamilyPlacementTypeNotSupportedWarning(modelInstance, aElementType);
                        return null;
                }
            }
            else if (aElementType is WallType)
                aElement = ToRevitElement_Wall(modelInstance, (WallType)aElementType, pushSettings);

            aElement.CheckIfNullPush(modelInstance);
            if (aElement == null)
                return null;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aElement, modelInstance, null, pushSettings.ConvertUnits);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(modelInstance, aElement);

            return aElement;
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static Element ToRevitElement(this DraftingInstance draftingInstance, Document document, PushSettings pushSettings = null)
        {
            if (draftingInstance == null || string.IsNullOrEmpty(draftingInstance.ViewName) || document == null)
                return null;

            if (string.IsNullOrWhiteSpace(draftingInstance.ViewName))
                return null;

            Element aElement = pushSettings.FindRefObject<Element>(document, draftingInstance.BHoM_Guid);
            if (aElement != null)
                return aElement;

            if (draftingInstance.Properties == null)
            {
                Compute.NullObjectPropertiesWarining(draftingInstance);
                return null;
            }

            View aView = Query.View(draftingInstance, document);

            if (aView == null)
                return null;

            pushSettings = pushSettings.DefaultIfNull();

            BuiltInCategory aBuiltInCategory = draftingInstance.Properties.BuiltInCategory(document, pushSettings.FamilyLoadSettings);

            ElementType aElementType = draftingInstance.Properties.ElementType(document, aBuiltInCategory, pushSettings.FamilyLoadSettings);

            if (aElementType != null)
            {
                if (aElementType is FamilySymbol)
                {
                    FamilySymbol aFamilySymbol = (FamilySymbol)aElementType;
                    Autodesk.Revit.DB.Family aFamily = aFamilySymbol.Family;

                    IGeometry aIGeometry = draftingInstance.Location;

                    switch (aFamily.FamilyPlacementType)
                    {
                        //TODO: Implement rest of the cases

                        case FamilyPlacementType.ViewBased:
                            if (aIGeometry is oM.Geometry.Point)
                            {
                                XYZ aXYZ = ToRevit((oM.Geometry.Point)aIGeometry, pushSettings);
                                aElement = document.Create.NewFamilyInstance(aXYZ, aFamilySymbol, aView);
                            }
                            break;
                        case FamilyPlacementType.OneLevelBased:
                            break;
                    }
                }
            }

            aElement.CheckIfNullPush(draftingInstance);
            if (aElement == null)
                return null;

            if (aElement != null && pushSettings.CopyCustomData)
                Modify.SetParameters(aElement, draftingInstance, null, pushSettings.ConvertUnits);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(draftingInstance, aElement);

            return aElement;
        }

        /***************************************************/

        private static Element ToRevitElement_CurveBased(this ModelInstance modelInstance, FamilySymbol familySymbol, PushSettings pushSettings = null)
        {
            if (familySymbol == null || modelInstance == null)
                return null;

            if (!(modelInstance.Location is ICurve))
            {
                Compute.InvalidFamilyPlacementTypeWarning(modelInstance, familySymbol);
                return null;
            }

            Document aDocument = familySymbol.Document;

            ICurve Curve = (ICurve)modelInstance.Location;

            Level aLevel = Curve.BottomLevel(familySymbol.Document, pushSettings.ConvertUnits);
            if (aLevel == null)
                return null;

            Curve aCurve = ToRevitCurve(Curve, pushSettings);
            return aDocument.Create.NewFamilyInstance(aCurve, familySymbol, aLevel, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
        }

        /***************************************************/

        private static Element ToRevitElement_OneLevelBased(this ModelInstance modelInstance, FamilySymbol familySymbol, PushSettings pushSettings = null)
        {
            if (familySymbol == null || modelInstance == null)
                return null;

            if (!(modelInstance.Location is oM.Geometry.Point))
            {
                Compute.InvalidFamilyPlacementTypeWarning(modelInstance, familySymbol);
                return null;
            }

            Document aDocument = familySymbol.Document;

            oM.Geometry.Point aPoint = (oM.Geometry.Point)modelInstance.Location;

            Level aLevel = aPoint.BottomLevel(aDocument, pushSettings.ConvertUnits);
            if (aLevel == null)
                return null;

            XYZ aXYZ = ToRevitXYZ(aPoint, pushSettings);
            return aDocument.Create.NewFamilyInstance(aXYZ, familySymbol, aLevel, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
        }

        /***************************************************/

        private static Element ToRevitElement_CurveDrivenStructural(this ModelInstance modelInstance, FamilySymbol familySymbol, PushSettings pushSettings = null)
        {
            if (familySymbol == null || modelInstance == null)
                return null;

            if (!(modelInstance.Location is ICurve))
            {
                Compute.InvalidFamilyPlacementTypeWarning(modelInstance, familySymbol);
                return null;
            }

            Autodesk.Revit.DB.Structure.StructuralType aStructuralType = Autodesk.Revit.DB.Structure.StructuralType.NonStructural;

            BuiltInCategory aBuiltInCategory = (BuiltInCategory)familySymbol.Category.Id.IntegerValue;
            switch (aBuiltInCategory)
            {
                case BuiltInCategory.OST_StructuralColumns:
                    aStructuralType = Autodesk.Revit.DB.Structure.StructuralType.Column;
                    break;
                case BuiltInCategory.OST_StructuralFraming:
                    aStructuralType = Autodesk.Revit.DB.Structure.StructuralType.Beam;
                    break;
            }

            if (aStructuralType == Autodesk.Revit.DB.Structure.StructuralType.NonStructural)
                return null;

            Document aDocument = familySymbol.Document;

            ICurve Curve = (ICurve)modelInstance.Location;

            Level aLevel = Curve.BottomLevel(aDocument, pushSettings.ConvertUnits);
            if (aLevel == null)
                return null;

            Curve aCurve = ToRevitCurve(Curve, pushSettings);
            return aDocument.Create.NewFamilyInstance(aCurve, familySymbol, aLevel, aStructuralType);
        }

        /***************************************************/

        private static Element ToRevitElement_TwoLevelsBased(this ModelInstance modelInstance, FamilySymbol familySymbol, PushSettings pushSettings = null)
        {
            if (familySymbol == null || modelInstance == null)
                return null;

            Autodesk.Revit.DB.Structure.StructuralType aStructuralType = Autodesk.Revit.DB.Structure.StructuralType.NonStructural;

            BuiltInCategory aBuiltInCategory = (BuiltInCategory)familySymbol.Category.Id.IntegerValue;
            switch (aBuiltInCategory)
            {
                case BuiltInCategory.OST_StructuralColumns:
                    aStructuralType = Autodesk.Revit.DB.Structure.StructuralType.Column;
                    break;
                case BuiltInCategory.OST_StructuralFraming:
                    aStructuralType = Autodesk.Revit.DB.Structure.StructuralType.Beam;
                    break;
            }

            if (aStructuralType == Autodesk.Revit.DB.Structure.StructuralType.NonStructural)
                return null;

            Document aDocument = familySymbol.Document;

            if (modelInstance.Location is oM.Geometry.Point)
            {
                oM.Geometry.Point aPoint = (oM.Geometry.Point)modelInstance.Location;

                Level aLevel = aPoint.BottomLevel(aDocument, pushSettings.ConvertUnits);
                if (aLevel == null)
                    return null;

                XYZ aXYZ = ToRevitXYZ(aPoint, pushSettings);
                return aDocument.Create.NewFamilyInstance(aXYZ, familySymbol, aLevel, aStructuralType);
            }
            else if (modelInstance.Location is oM.Geometry.Line)
            {
                oM.Geometry.Line line = (oM.Geometry.Line)modelInstance.Location;
                if (line.Start.Z > line.End.Z)
                {
                    Compute.InvalidTwoLevelLocationWarning(modelInstance, familySymbol);
                    return null;
                }

                Level aLevel = line.Start.BottomLevel(aDocument, pushSettings.ConvertUnits);
                if (aLevel == null)
                    return null;

                Curve curve = line.ToRevitCurve(pushSettings);
                return aDocument.Create.NewFamilyInstance(curve, familySymbol, aLevel, aStructuralType);
            }
            else
            {
                Compute.InvalidFamilyPlacementTypeWarning(modelInstance, familySymbol);
                return null;
            }
        }

        /***************************************************/

        private static Element ToRevitElement_Wall(this ModelInstance modelInstance, WallType wallType, PushSettings pushSettings = null)
        {
            if (wallType == null || modelInstance == null)
                return null;

            if (!(modelInstance.Location is ICurve))
            {
                Compute.InvalidFamilyPlacementTypeWarning(modelInstance, wallType);
                return null;
            }

            Document aDocument = wallType.Document;

            ICurve Curve = (ICurve)modelInstance.Location;

            Level aLevel = Curve.BottomLevel(wallType.Document, pushSettings.ConvertUnits);
            if (aLevel == null)
                return null;

            Curve aCurve = ToRevitCurve(Curve, pushSettings);
            return Wall.Create(aDocument, aCurve, aLevel.Id, false);
        }

        /***************************************************/
    }
}