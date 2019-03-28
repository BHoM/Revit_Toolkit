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

using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Generic;
using BH.oM.Base;
using BH.oM.Environment.Elements;
using BH.oM.DataManipulation.Queries;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static BuiltInCategory BuiltInCategory(this oM.Environment.Elements.PanelType panelType)
        {
            switch (panelType)
            {
                case (oM.Environment.Elements.PanelType.Ceiling):
                    return Autodesk.Revit.DB.BuiltInCategory.OST_Ceilings;
                case (oM.Environment.Elements.PanelType.Floor):
                    return Autodesk.Revit.DB.BuiltInCategory.OST_Floors;
                case (oM.Environment.Elements.PanelType.Roof):
                    return Autodesk.Revit.DB.BuiltInCategory.OST_Roofs;
                case (oM.Environment.Elements.PanelType.Wall):
                    return Autodesk.Revit.DB.BuiltInCategory.OST_Walls;
                case (oM.Environment.Elements.PanelType.Undefined):
                    return Autodesk.Revit.DB.BuiltInCategory.INVALID;
                default:
                    return Autodesk.Revit.DB.BuiltInCategory.INVALID;
            }
        }

        /***************************************************/

        public static BuiltInCategory BuiltInCategory(this oM.Environment.Elements.OpeningType openingType)
        {
            switch (openingType)
            {
                case (oM.Environment.Elements.OpeningType.Door):
                    return Autodesk.Revit.DB.BuiltInCategory.OST_Doors;
                case (oM.Environment.Elements.OpeningType.Window):
                    return Autodesk.Revit.DB.BuiltInCategory.OST_Windows;
                case (oM.Environment.Elements.OpeningType.Undefined):
                    return Autodesk.Revit.DB.BuiltInCategory.INVALID;
                default:
                    return Autodesk.Revit.DB.BuiltInCategory.INVALID;
            }
        }

        /***************************************************/

        public static BuiltInCategory BuiltInCategory(this IBHoMObject bHoMObject, Document document)
        {
            if (bHoMObject == null || document == null)
                return Autodesk.Revit.DB.BuiltInCategory.INVALID;

            BuiltInCategory aBuiltInCategory = Autodesk.Revit.DB.BuiltInCategory.INVALID;
            string aCategoryName = BH.Engine.Adapters.Revit.Query.CategoryName(bHoMObject);
            if (!string.IsNullOrEmpty(aCategoryName))
                aBuiltInCategory = BuiltInCategory(document, aCategoryName);

            if (aBuiltInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID)
            {
                string aFamilyName = BH.Engine.Adapters.Revit.Query.FamilyName(bHoMObject);
                if (string.IsNullOrEmpty(aFamilyName))
                    return Autodesk.Revit.DB.BuiltInCategory.INVALID;

                string aTypeName = BH.Engine.Adapters.Revit.Query.FamilyTypeName(bHoMObject);

                List<ElementType> aElementTypeList = new FilteredElementCollector(document).OfClass(typeof(ElementType)).Cast<ElementType>().ToList();

                ElementType aElementType = null;
                if (string.IsNullOrEmpty(aTypeName))
                    aElementType = aElementTypeList.Find(x => x.FamilyName == aFamilyName);
                else
                {
                    aElementType = aElementTypeList.Find(x => x.FamilyName == aFamilyName && x.Name == aTypeName);
                    if(aElementType == null)
                        aElementType = aElementTypeList.Find(x => x.FamilyName == aFamilyName);
                }

                if (aElementType != null && aElementType.Category != null)
                    aBuiltInCategory = (BuiltInCategory)aElementType.Category.Id.IntegerValue;

            }

            return aBuiltInCategory;
        }

        /***************************************************/

        public static BuiltInCategory BuiltInCategory(this oM.Adapters.Revit.Elements.Family family, Document document)
        {
            if (family == null || document == null)
                return Autodesk.Revit.DB.BuiltInCategory.INVALID;

            BuiltInCategory aBuiltInCategory = Autodesk.Revit.DB.BuiltInCategory.INVALID;
            string aCategoryName = BH.Engine.Adapters.Revit.Query.CategoryName(family);
            if (!string.IsNullOrEmpty(aCategoryName))
                aBuiltInCategory = BuiltInCategory(document, aCategoryName);

            if (aBuiltInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID)
            {
                string aFamilyName = BH.Engine.Adapters.Revit.Query.FamilyName(family);
                if (string.IsNullOrEmpty(aFamilyName))
                    return Autodesk.Revit.DB.BuiltInCategory.INVALID;

                List<Family> aFamilyList = new FilteredElementCollector(document).OfClass(typeof(Family)).Cast<Family>().ToList();
                Family aFamily = aFamilyList.Find(x => x.Name == aFamilyName);

                if (aFamily != null && aFamily.FamilyCategory != null)
                    aBuiltInCategory = (BuiltInCategory)aFamily.FamilyCategory.Id.IntegerValue;

            }

            return aBuiltInCategory;
        }

        /***************************************************/

        public static BuiltInCategory BuiltInCategory(this Document document, string categoryName)
        {
            if (document == null || string.IsNullOrEmpty(categoryName)|| document.Settings == null || document.Settings.Categories == null)
                return Autodesk.Revit.DB.BuiltInCategory.INVALID;


            foreach (Category aCategory in document.Settings.Categories)
                if (aCategory.Name == categoryName)
                    return (BuiltInCategory)aCategory.Id.IntegerValue;

            return Autodesk.Revit.DB.BuiltInCategory.INVALID;
        }

        /***************************************************/

        public static BuiltInCategory BuiltInCategory(this IBHoMObject bHoMObject, Document document, FamilyLibrary familyLibrary)
        {
            if (bHoMObject == null || document == null)
                return Autodesk.Revit.DB.BuiltInCategory.INVALID;

            BuiltInCategory aBuiltInCategory = Autodesk.Revit.DB.BuiltInCategory.INVALID;
            if(bHoMObject is oM.Adapters.Revit.Elements.Family)
                aBuiltInCategory = ((oM.Adapters.Revit.Elements.Family)bHoMObject).BuiltInCategory(document);
            else
                aBuiltInCategory = bHoMObject.BuiltInCategory(document);

            if (aBuiltInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID)
            {
                string aFamilyName = BH.Engine.Adapters.Revit.Query.FamilyName(bHoMObject);
                if (!string.IsNullOrEmpty(aFamilyName))
                {
                    string aCategoryName = document.CategoryName(aFamilyName);
                    if (!string.IsNullOrEmpty(aCategoryName))
                        aBuiltInCategory = document.BuiltInCategory(aCategoryName);
                }

                if (aBuiltInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID && familyLibrary != null)
                {
                    List<string> aCategoryNameList = BH.Engine.Adapters.Revit.Query.CategoryNames(familyLibrary, aFamilyName, BH.Engine.Adapters.Revit.Query.FamilyTypeName(bHoMObject));
                    if (aCategoryNameList != null && aCategoryNameList.Count > 0)
                        aBuiltInCategory = document.BuiltInCategory(aCategoryNameList.First());

                    if (aBuiltInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID)
                    {
                        aCategoryNameList = BH.Engine.Adapters.Revit.Query.CategoryNames(familyLibrary, aFamilyName);
                        if (aCategoryNameList != null && aCategoryNameList.Count > 0)
                            aBuiltInCategory = document.BuiltInCategory(aCategoryNameList.First());
                    }
                }

            }

            return aBuiltInCategory;
        }

        /***************************************************/

        public static BuiltInCategory BuiltInCategory(this IBHoMObject bHoMObject, Document document, FamilyLoadSettings familyLoadSettings)
        {
            if (bHoMObject == null || document == null)
                return Autodesk.Revit.DB.BuiltInCategory.INVALID;

            FamilyLibrary aFamilyLibrary = null;
            if (familyLoadSettings != null)
                aFamilyLibrary = familyLoadSettings.FamilyLibrary;

            return BuiltInCategory(bHoMObject, document, aFamilyLibrary);
        }

        /***************************************************/

        public static BuiltInCategory BuiltInCategory(this FilterQuery filterQuery, Document document)
        {
            if (document == null || document.Settings == null || document.Settings.Categories == null || filterQuery == null)
                return Autodesk.Revit.DB.BuiltInCategory.INVALID;

            string aCategoryName = BH.Engine.Adapters.Revit.Query.CategoryName(filterQuery);

            return BuiltInCategory(document, aCategoryName);
        }

        /***************************************************/
    }
}
