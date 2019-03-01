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
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Internal Methods                          ****/
        /***************************************************/

        internal static Element ToRevitElement(this GenericObject genericObject, Document document, PushSettings pushSettings = null)
        {
            Element aElement = pushSettings.FindRefObject<Element>(document, genericObject.BHoM_Guid);
            if (aElement != null)
                return aElement;

            pushSettings.DefaultIfNull();

            BuiltInCategory aBuiltInCategory = genericObject.BuiltInCategory(document, pushSettings.FamilyLoadSettings);

            ElementType aElementType = genericObject.ElementType(document, aBuiltInCategory, pushSettings.FamilyLoadSettings);
            if (aElementType == null)
            {
                Compute.ElementTypeNotFoundWarning(genericObject);
                return null;
            }

            if (aElementType is FamilySymbol)
            {
                FamilySymbol aFamilySymbol = (FamilySymbol)aElementType;
                FamilyPlacementType aFamilyPlacementType = aFamilySymbol.Family.FamilyPlacementType;

                IGeometry aIGeometry = genericObject.Location;

                if(aFamilyPlacementType == FamilyPlacementType.Invalid || 
                    aFamilyPlacementType == FamilyPlacementType.Adaptive ||
                    (aIGeometry is oM.Geometry.Point && (aFamilyPlacementType == FamilyPlacementType.CurveBased || aFamilyPlacementType == FamilyPlacementType.CurveBasedDetail || aFamilyPlacementType == FamilyPlacementType.CurveDrivenStructural)) || 
                    (aIGeometry is ICurve && (aFamilyPlacementType == FamilyPlacementType.OneLevelBased || aFamilyPlacementType == FamilyPlacementType.OneLevelBasedHosted || aFamilyPlacementType == FamilyPlacementType.TwoLevelsBased)))
                {
                    Compute.InvalidFamilyPlacementTypeWarning(genericObject, aElementType);
                    return null;
                }

                switch (aFamilyPlacementType)
                {
                    //TODO: Implement rest of the cases
                    case FamilyPlacementType.CurveBased:
                        aElement = ToRevitElement_CurveBased(genericObject, aFamilySymbol, pushSettings);
                        break;
                    case FamilyPlacementType.OneLevelBased:
                        aElement = ToRevitElement_OneLevelBased(genericObject, aFamilySymbol, pushSettings);
                        break;
                    case FamilyPlacementType.CurveDrivenStructural:
                        aElement = ToRevitElement_CurveDrivenStructural(genericObject, aFamilySymbol, pushSettings);
                        break;
                    case FamilyPlacementType.TwoLevelsBased:
                        aElement = ToRevitElement_TwoLevelsBased(genericObject, aFamilySymbol, pushSettings);
                        break;
                    default:
                        Compute.FamilyPlacementTypeNotSupportedWarning(genericObject, aElementType);
                        return null;
                }
            }
            else if (aElementType is WallType)
                aElement = ToRevitElement_Wall(genericObject, (WallType)aElementType, pushSettings);

            aElement.CheckIfNullPush(genericObject);
            if (aElement == null)
                return null;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aElement, genericObject, null, pushSettings.ConvertUnits);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(genericObject, aElement);

            return aElement;
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static Element ToRevitElement(this DraftingObject draftingObject, Document document, PushSettings pushSettings = null)
        {
            if (draftingObject == null || string.IsNullOrEmpty(draftingObject.ViewName) || document == null)
                return null;

            if (string.IsNullOrWhiteSpace(draftingObject.ViewName))
                return null;

            Element aElement = pushSettings.FindRefObject<Element>(document, draftingObject.BHoM_Guid);
            if (aElement != null)
                return aElement;

            pushSettings = pushSettings.DefaultIfNull();

            List<View> aViewList = new FilteredElementCollector(document).OfClass(typeof(ViewDrafting)).Cast<View>().ToList();
            if(aViewList == null || aViewList.Count == 0)
                return null;

            View aView = aViewList.Find(x => x.Name == draftingObject.ViewName);
            if (aView == null)
            {
                aViewList = new FilteredElementCollector(document).OfClass(typeof(ViewSheet)).Cast<View>().ToList();
                string aTitle = draftingObject.ViewName;
                if (!aTitle.StartsWith("Sheet: "))
                    aTitle = string.Format("Sheet: {0}", aTitle);

                aView = aViewList.Find(x => x.Title == aTitle);
            }

            if (aView == null)
                return null;

            BuiltInCategory aBuiltInCategory = draftingObject.BuiltInCategory(document, pushSettings.FamilyLoadSettings);

            ElementType aElementType = draftingObject.ElementType(document, aBuiltInCategory, pushSettings.FamilyLoadSettings);

            if (aElementType != null)
            {
                if (aElementType is FamilySymbol)
                {
                    FamilySymbol aFamilySymbol = (FamilySymbol)aElementType;
                    Family aFamily = aFamilySymbol.Family;

                    IGeometry aIGeometry = draftingObject.Location;

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

            aElement.CheckIfNullPush(draftingObject);
            if (aElement == null)
                return null;

            if (aElement != null && pushSettings.CopyCustomData)
                Modify.SetParameters(aElement, draftingObject, null, pushSettings.ConvertUnits);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(draftingObject, aElement);

            return aElement;
        }

        /***************************************************/

        private static Element ToRevitElement_CurveBased(this GenericObject genericObject, FamilySymbol familySymbol, PushSettings pushSettings = null)
        {
            if (familySymbol == null || genericObject == null)
                return null;

            if (!(genericObject.Location is ICurve))
            {
                Compute.InvalidFamilyPlacementTypeWarning(genericObject, familySymbol);
                return null;
            }

            Document aDocument = familySymbol.Document;

            ICurve Curve = (ICurve)genericObject.Location;

            Level aLevel = Curve.BottomLevel(familySymbol.Document, pushSettings.ConvertUnits);
            if (aLevel == null)
                return null;

            Curve aCurve = ToRevitCurve(Curve, pushSettings);
            return aDocument.Create.NewFamilyInstance(aCurve, familySymbol, aLevel, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
        }

        /***************************************************/

        private static Element ToRevitElement_OneLevelBased(this GenericObject genericObject, FamilySymbol familySymbol, PushSettings pushSettings = null)
        {
            if (familySymbol == null || genericObject == null)
                return null;

            if (!(genericObject.Location is oM.Geometry.Point))
            {
                Compute.InvalidFamilyPlacementTypeWarning(genericObject, familySymbol);
                return null;
            }

            Document aDocument = familySymbol.Document;

            oM.Geometry.Point aPoint = (oM.Geometry.Point)genericObject.Location;

            Level aLevel = aPoint.BottomLevel(aDocument, pushSettings.ConvertUnits);
            if (aLevel == null)
                return null;

            XYZ aXYZ = ToRevitXYZ(aPoint, pushSettings);
            return aDocument.Create.NewFamilyInstance(aXYZ, familySymbol, aLevel, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
        }

        /***************************************************/

        private static Element ToRevitElement_CurveDrivenStructural(this GenericObject genericObject, FamilySymbol familySymbol, PushSettings pushSettings = null)
        {
            if (familySymbol == null || genericObject == null)
                return null;

            if (!(genericObject.Location is ICurve))
            {
                Compute.InvalidFamilyPlacementTypeWarning(genericObject, familySymbol);
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

            ICurve Curve = (ICurve)genericObject.Location;

            Level aLevel = Curve.BottomLevel(aDocument, pushSettings.ConvertUnits);
            if (aLevel == null)
                return null;

            Curve aCurve = ToRevitCurve(Curve, pushSettings);
            return aDocument.Create.NewFamilyInstance(aCurve, familySymbol, aLevel, aStructuralType);
        }

        /***************************************************/

        private static Element ToRevitElement_TwoLevelsBased(this GenericObject genericObject, FamilySymbol familySymbol, PushSettings pushSettings = null)
        {
            if (familySymbol == null || genericObject == null)
                return null;

            if (!(genericObject.Location is oM.Geometry.Point))
            {
                Compute.InvalidFamilyPlacementTypeWarning(genericObject, familySymbol);
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

            oM.Geometry.Point aPoint = (oM.Geometry.Point)genericObject.Location;

            Level aLevel = aPoint.BottomLevel(aDocument, pushSettings.ConvertUnits);
            if (aLevel == null)
                return null;

            XYZ aXYZ = ToRevitXYZ(aPoint, pushSettings);
            return aDocument.Create.NewFamilyInstance(aXYZ, familySymbol, aLevel, aStructuralType);
        }

        /***************************************************/

        private static Element ToRevitElement_Wall(this GenericObject genericObject, WallType wallType, PushSettings pushSettings = null)
        {
            if (wallType == null || genericObject == null)
                return null;

            if (!(genericObject.Location is ICurve))
            {
                Compute.InvalidFamilyPlacementTypeWarning(genericObject, wallType);
                return null;
            }

            Document aDocument = wallType.Document;

            ICurve Curve = (ICurve)genericObject.Location;

            Level aLevel = Curve.BottomLevel(wallType.Document, pushSettings.ConvertUnits);
            if (aLevel == null)
                return null;

            Curve aCurve = ToRevitCurve(Curve, pushSettings);
            return Wall.Create(aDocument, aCurve, aLevel.Id, false);
        }

        /***************************************************/
    }
}