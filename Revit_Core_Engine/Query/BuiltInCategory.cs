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
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Properties;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static BuiltInCategory BuiltInCategory(this Document document, string categoryName, bool caseSensitive = true)
        {
            if (string.IsNullOrEmpty(categoryName) || document?.Settings?.Categories == null)
                return Autodesk.Revit.DB.BuiltInCategory.INVALID;

            //TODO: use LabelUtils?!
            foreach (Category category in document.Settings.Categories)
            {
                if ((caseSensitive && category.Name == categoryName) || (!caseSensitive && category.Name.ToUpper() == categoryName.ToUpper()))
                    return (BuiltInCategory)category.Id.IntegerValue;
            }

            //TODO: why is that?
            switch (categoryName)
            {
                case "Space Type Settings":
                    return Autodesk.Revit.DB.BuiltInCategory.OST_HVAC_Load_Space_Types;
            }

            return Autodesk.Revit.DB.BuiltInCategory.INVALID;
        }

        /***************************************************/

        public static BuiltInCategory BuiltInCategory(this IInstance instance, Document document, bool caseSensitive = true)
        {
            return (instance?.Properties).BuiltInCategory(document, caseSensitive);
        }

        /***************************************************/

        public static BuiltInCategory BuiltInCategory(this InstanceProperties properties, Document document, bool caseSensitive = true)
        {
            return document.BuiltInCategory(properties?.CategoryName, caseSensitive);
        }

        /***************************************************/

        //public static BuiltInCategory BuiltInCategory(this IBHoMObject bHoMObject, Document document)
        //{
        //    if (bHoMObject == null || document == null)
        //        return Autodesk.Revit.DB.BuiltInCategory.INVALID;

        //    BuiltInCategory builtInCategory = document.BuiltInCategory(bHoMObject.CategoryName());

        //    if (builtInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID)
        //    {
        //        //TODO: use TryGetRevitNames? Or even ElementType()!
        //        string familyName = BH.Engine.Adapters.Revit.Query.FamilyName(bHoMObject);
        //        if (string.IsNullOrEmpty(familyName))
        //            return Autodesk.Revit.DB.BuiltInCategory.INVALID;

        //        string typeName = BH.Engine.Adapters.Revit.Query.FamilyTypeName(bHoMObject);

        //        List<ElementType> elementTypes = new FilteredElementCollector(document).OfClass(typeof(ElementType)).Cast<ElementType>().ToList();

        //        ElementType elementType = null;
        //        if (string.IsNullOrEmpty(typeName))
        //            elementType = elementTypes.Find(x => x.FamilyName == familyName);
        //        else
        //        {
        //            elementType = elementTypes.Find(x => x.FamilyName == familyName && x.Name == typeName);
        //            if(elementType == null)
        //                elementType = elementTypes.Find(x => x.FamilyName == familyName);
        //        }

        //        if (elementType != null && elementType.Category != null)
        //            builtInCategory = (BuiltInCategory)elementType.Category.Id.IntegerValue;
        //    }

        //    return builtInCategory;
        //}

        /***************************************************/

        //public static BuiltInCategory BuiltInCategory(this oM.Adapters.Revit.Elements.Family family, Document document)
        //{
        //    if (family == null || document == null)
        //        return Autodesk.Revit.DB.BuiltInCategory.INVALID;

        //    BuiltInCategory builtInCategory = Autodesk.Revit.DB.BuiltInCategory.INVALID;
        //    string categoryName = BH.Engine.Adapters.Revit.Query.CategoryName(family);
        //    if (!string.IsNullOrEmpty(categoryName))
        //        builtInCategory = BuiltInCategory(document, categoryName);

        //    if (builtInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID)
        //    {
        //        string familyName = BH.Engine.Adapters.Revit.Query.FamilyName(family);
        //        if (string.IsNullOrEmpty(familyName))
        //            return Autodesk.Revit.DB.BuiltInCategory.INVALID;

        //        List<Family> familyList = new FilteredElementCollector(document).OfClass(typeof(Family)).Cast<Family>().ToList();
        //        Family revitFamily = familyList.Find(x => x.Name == familyName);

        //        if (revitFamily != null && revitFamily.FamilyCategory != null)
        //            builtInCategory = (BuiltInCategory)revitFamily.FamilyCategory.Id.IntegerValue;
        //    }

        //    return builtInCategory;
        //}

        /***************************************************/

        //public static BuiltInCategory BuiltInCategory(this IBHoMObject bHoMObject, Document document, FamilyLibrary familyLibrary)
        //{
        //    if (bHoMObject == null || document == null)
        //        return Autodesk.Revit.DB.BuiltInCategory.INVALID;

        //    BuiltInCategory builtInCategory = Autodesk.Revit.DB.BuiltInCategory.INVALID;
        //    if(bHoMObject is oM.Adapters.Revit.Elements.Family)
        //        builtInCategory = ((oM.Adapters.Revit.Elements.Family)bHoMObject).BuiltInCategory(document);
        //    else
        //        builtInCategory = bHoMObject.BuiltInCategory(document);

        //    if (builtInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID)
        //    {
        //        string familyName = BH.Engine.Adapters.Revit.Query.FamilyName(bHoMObject);
        //        if (!string.IsNullOrEmpty(familyName))
        //        {
        //            string categoryName = document.CategoryName(familyName);
        //            if (!string.IsNullOrEmpty(categoryName))
        //                builtInCategory = document.BuiltInCategory(categoryName);
        //        }

        //        if (builtInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID && familyLibrary != null)
        //        {
        //            List<string> categoryNames = BH.Engine.Adapters.Revit.Query.CategoryNames(familyLibrary, familyName, BH.Engine.Adapters.Revit.Query.FamilyTypeName(bHoMObject));
        //            if (categoryNames != null && categoryNames.Count > 0)
        //                builtInCategory = document.BuiltInCategory(categoryNames.First());

        //            if (builtInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID)
        //            {
        //                categoryNames = BH.Engine.Adapters.Revit.Query.CategoryNames(familyLibrary, familyName);
        //                if (categoryNames != null && categoryNames.Count > 0)
        //                    builtInCategory = document.BuiltInCategory(categoryNames.First());
        //            }
        //        }
        //    }

        //    return builtInCategory;
        //}

        /***************************************************/

        //public static BuiltInCategory BuiltInCategory(this IBHoMObject bHoMObject, Document document, FamilyLoadSettings familyLoadSettings)
        //{
        //    if (bHoMObject == null || document == null)
        //        return Autodesk.Revit.DB.BuiltInCategory.INVALID;

        //    FamilyLibrary familyLibrary = null;
        //    if (familyLoadSettings != null)
        //        familyLibrary = familyLoadSettings.FamilyLibrary;

        //    return BuiltInCategory(bHoMObject, document, familyLibrary);
        //}

        /***************************************************/
    }
}

