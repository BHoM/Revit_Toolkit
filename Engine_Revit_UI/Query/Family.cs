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
using BH.oM.Base;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Family Family(this IBHoMObject bHoMObject, Document document)
        {
            if (document == null || bHoMObject == null)
                return null;

            string aFamilyName = BH.Engine.Adapters.Revit.Query.FamilyName(bHoMObject);
            if (string.IsNullOrEmpty(aFamilyName) && !string.IsNullOrEmpty(bHoMObject.Name) && bHoMObject.Name.Contains(":"))
                aFamilyName = BH.Engine.Adapters.Revit.Query.FamilyName(bHoMObject.Name);

            if (string.IsNullOrWhiteSpace(aFamilyName))
                return null;

            List<Family> aFamilyList = new FilteredElementCollector(document).OfClass(typeof(Family)).Cast<Family>().ToList();
            return aFamilyList.Find(x => x.Name == aFamilyName);
        }

        /***************************************************/

        public static Family Family(this IBHoMObject bHoMObject, IEnumerable<Family> families)
        {
            if (families == null || bHoMObject == null)
                return null;

            string aFamilyName = BH.Engine.Adapters.Revit.Query.FamilyName(bHoMObject);
            if (string.IsNullOrEmpty(aFamilyName) && !string.IsNullOrEmpty(bHoMObject.Name) && bHoMObject.Name.Contains(":"))
                aFamilyName = BH.Engine.Adapters.Revit.Query.FamilyName(bHoMObject.Name);

            if (string.IsNullOrWhiteSpace(aFamilyName))
                return null;

            foreach (Family aFamily in families)
                if (aFamily.Name == aFamilyName)
                    return aFamily;

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
                List<Family> aFamilyList;
                if (builtInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID)
                    aFamilyList = new FilteredElementCollector(document).OfClass(typeof(Family)).Cast<Family>().ToList();
                else
                    aFamilyList = new FilteredElementCollector(document).OfClass(typeof(Family)).OfCategory(builtInCategory).Cast<Family>().ToList();

                Family aFamily = Family(family, aFamilyList);
                if (aFamily != null)
                    return aFamily;
            }

            string aFamilyName = family.FamilyName();

            //Find ElementType in FamilyLibrary
            if (familyLoadSettings != null && !string.IsNullOrWhiteSpace(aFamilyName))
            {
                foreach (BuiltInCategory builtInCategory in builtInCategories)
                {
                    if (builtInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID)
                        continue;

                    string aCategoryName = builtInCategory.CategoryName(document);
                    if (string.IsNullOrEmpty(aCategoryName))
                        aCategoryName = family.CategoryName();

                    Family aFamily = familyLoadSettings.LoadFamily(document, aCategoryName, aFamilyName);
                    if (aFamily != null)
                        return aFamily;
                }
            }

            return null;
        }

        /***************************************************/

        public static Family Family(this oM.Adapters.Revit.Elements.Family family, Document document, BuiltInCategory builtInCategory, FamilyLoadSettings familyLoadSettings = null)
        {
            return Family(family, document, new BuiltInCategory[] { builtInCategory }, familyLoadSettings);
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/



        /***************************************************/
    }
}
