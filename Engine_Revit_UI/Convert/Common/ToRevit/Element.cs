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

            if (aElementType != null)
            {
                if (aElementType is FamilySymbol)
                {
                    FamilySymbol aFamilySymbol = (FamilySymbol)aElementType;
                    Family aFamily = aFamilySymbol.Family;

                    IGeometry aIGeometry = genericObject.Location;

                    switch (aFamily.FamilyPlacementType)
                    {
                        //TODO: Implement rest of the cases

                        case FamilyPlacementType.ViewBased:
                            break;
                        case FamilyPlacementType.CurveBased:
                            if (aIGeometry is ICurve)
                            {
                                Level aLevel = ((ICurve)aIGeometry).BottomLevel(document, pushSettings.ConvertUnits);
                                if (aLevel == null)
                                    break;

                                Curve aCurve = ToRevitCurve((ICurve)aIGeometry, pushSettings);
                                aElement = document.Create.NewFamilyInstance(aCurve, aFamilySymbol, aLevel, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                            }
                            else
                            {
                                Compute.InvalidLocationTypeWarning(genericObject, aElementType);
                            }
                            break;
                        case FamilyPlacementType.OneLevelBased:
                            if (aIGeometry is oM.Geometry.Point)
                            {
                                XYZ aXYZ = ToRevit((oM.Geometry.Point)aIGeometry, pushSettings);
                                aElement = document.Create.NewFamilyInstance(aXYZ, aFamilySymbol, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                            }
                            else
                            {
                                Compute.InvalidLocationTypeWarning(genericObject, aElementType);
                            }
                            break;
                        case FamilyPlacementType.CurveDrivenStructural:
                            if(aIGeometry is ICurve)
                            {
                                Level aLevel = ((ICurve)aIGeometry).BottomLevel(document, pushSettings.ConvertUnits);
                                if (aLevel == null)
                                    break;

                                Curve aCurve = ToRevitCurve((ICurve)aIGeometry, pushSettings);

                                Autodesk.Revit.DB.Structure.StructuralType aStructuralType = Autodesk.Revit.DB.Structure.StructuralType.NonStructural;

                                aBuiltInCategory = (BuiltInCategory)aFamilySymbol.Category.Id.IntegerValue;
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
                                    break;

                                aElement = document.Create.NewFamilyInstance(aCurve, aFamilySymbol, aLevel, aStructuralType);
                            }
                            else
                            {
                                Compute.InvalidLocationTypeWarning(genericObject, aElementType);
                            }
                            break;
                    }
                }
                else if(aElementType is WallType)
                {
                    IGeometry aIGeometry = genericObject.Location;
                    if(aIGeometry is ICurve)
                    {
                        Level aLevel = ((ICurve)aIGeometry).BottomLevel(document, pushSettings.ConvertUnits);
                        if (aLevel != null)
                        {
                            Curve aCurve = ToRevitCurve((ICurve)aIGeometry, pushSettings);
                            aElement = Wall.Create(document, aCurve, aLevel.Id, false);
                        }
                    }
                    else
                    {
                        Compute.InvalidLocationTypeWarning(genericObject, aElementType);
                    }
                }
            }

            aElement.CheckIfNullPush(genericObject);
            if (aElement == null)
                return null;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aElement, genericObject, null, pushSettings.ConvertUnits);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(genericObject, aElement);

            return aElement;
        }

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
    }
}