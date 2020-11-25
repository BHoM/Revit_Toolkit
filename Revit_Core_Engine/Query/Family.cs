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
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Family Family(this IBHoMObject bHoMObject, IEnumerable<Family> families)
        {
            if (families == null || bHoMObject == null)
                return null;

            string familyName = BH.Engine.Adapters.Revit.Query.FamilyName(bHoMObject);
            if (string.IsNullOrEmpty(familyName) && !string.IsNullOrEmpty(bHoMObject.Name) && bHoMObject.Name.Contains(":"))
                familyName = BH.Engine.Adapters.Revit.Query.FamilyName(bHoMObject.Name);

            if (string.IsNullOrWhiteSpace(familyName))
                return null;

            foreach (Family family in families)
            {
                if (family.Name == familyName)
                    return family;
            }

            return null;
        }

        /***************************************************/

        public static Family Family(this oM.Adapters.Revit.Elements.Family family, Document document, IEnumerable<BuiltInCategory> builtInCategories, FamilyLoadSettings familyLoadSettings = null)
        {
            if (family == null || document == null || builtInCategories == null || builtInCategories.Count() == 0)
                return null;

            //Find Existing Family in Document
            foreach (BuiltInCategory builtInCategory in builtInCategories)
            {
                List<Family> familyList;
                if (builtInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID)
                    familyList = new FilteredElementCollector(document).OfClass(typeof(Family)).Cast<Family>().ToList();
                else
                    familyList = new FilteredElementCollector(document).OfClass(typeof(Family)).OfCategory(builtInCategory).Cast<Family>().ToList();

                Family revitFamily = Family(family, familyList);
                if (revitFamily != null)
                    return revitFamily;
            }

            string familyName = family.FamilyName();

            //Find ElementType in FamilyLibrary
            if (familyLoadSettings != null && !string.IsNullOrWhiteSpace(familyName))
            {
                foreach (BuiltInCategory builtInCategory in builtInCategories)
                {
                    if (builtInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID)
                        continue;
                    
                    string categoryName = builtInCategory.CategoryName(document);
                    if (string.IsNullOrEmpty(categoryName))
                        categoryName = family.CategoryName();

                    Family revitFamily = familyLoadSettings.LoadFamily(document, categoryName, familyName);
                    if (revitFamily != null)
                        return revitFamily;
                }
            }

            return null;
        }

        /***************************************************/
    }
}

