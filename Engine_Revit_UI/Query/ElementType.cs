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

using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static ElementType ElementType(this IBHoMObject bHoMObject, IEnumerable<ElementType> elementTypes, bool exactMatch = true)
        {
            if (elementTypes == null || bHoMObject == null)
                return null;

            string familyName = null;
            string familyTypeName = null;

            if (!TryGetRevitNames(bHoMObject, out familyName, out familyTypeName))
                return null;

            ElementType result = null;
            if (!string.IsNullOrEmpty(familyTypeName))
            {
                foreach (ElementType elementType in elementTypes)
                {
                    if ((elementType.FamilyName == familyName && elementType.Name == familyTypeName) || (string.IsNullOrEmpty(familyName) && elementType.Name == familyTypeName))
                    {
                        result = elementType;
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(familyName) && !exactMatch)
            {
                foreach (ElementType elementType in elementTypes)
                {
                    if (elementType.FamilyName == familyName)
                    {
                        result = elementType;
                        break;
                    }
                }
            }

            return result;
        }

        /***************************************************/

        public static ElementType ElementType(this IBHoMObject bHoMObject, Document document, IEnumerable<BuiltInCategory> builtInCategories, FamilyLoadSettings familyLoadSettings = null, bool duplicateTypeIfNotExists = true)
        {
            if (bHoMObject == null || document == null || builtInCategories == null || builtInCategories.Count() == 0)
                return null;

            //Find Existing ElementType in Document
            foreach (BuiltInCategory builtInCategory in builtInCategories)
            {
                List<ElementType> elementTypes;
                if (builtInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID)
                    elementTypes = new FilteredElementCollector(document).OfClass(typeof(ElementType)).Cast<ElementType>().ToList();
                else
                    elementTypes = new FilteredElementCollector(document).OfClass(typeof(ElementType)).OfCategory(builtInCategory).Cast<ElementType>().ToList();
       
                ElementType elementType = bHoMObject.ElementType(elementTypes, true);
                if (elementType != null)
                {
                    if(elementType is FamilySymbol)
                    {
                        FamilySymbol familySymbol = (FamilySymbol)elementType;

                        if (!familySymbol.IsActive)
                            familySymbol.Activate();
                    }

                    return elementType;
                }  
            }

            string familyName = null;
            string familyTypeName = null;
            TryGetRevitNames(bHoMObject, out familyName, out familyTypeName);

            //Find ElementType in FamilyLibrary
            if (familyLoadSettings != null && !string.IsNullOrWhiteSpace(familyName) && !string.IsNullOrWhiteSpace(familyTypeName))
            {
                foreach (BuiltInCategory builtInCategory in builtInCategories)
                {
                    if (builtInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID)
                        continue;

                    string categoryName = builtInCategory.CategoryName(document);
                    if (string.IsNullOrEmpty(categoryName))
                        categoryName = bHoMObject.CategoryName();

                    FamilySymbol familySymbol = familyLoadSettings.LoadFamilySymbol(document, categoryName, familyName, familyTypeName);
                    if (familySymbol != null)
                    {
                        if (!familySymbol.IsActive)
                            familySymbol.Activate();

                        return familySymbol;
                    }
                }
            }

            //Duplicate if not exists
            if (duplicateTypeIfNotExists && !string.IsNullOrWhiteSpace(familyTypeName))
            {
                foreach (BuiltInCategory builtInCategory in builtInCategories)
                {
                    if (builtInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID)
                        continue;

                    List<ElementType> elementTypes = new FilteredElementCollector(document).OfClass(typeof(ElementType)).OfCategory(builtInCategory).Cast<ElementType>().ToList();

                    if (elementTypes.Count > 0 && !(elementTypes.First() is FamilySymbol))
                    {
                        // Duplicate Type for object which is not Family Symbol

                        if (!string.IsNullOrEmpty(familyName))
                            elementTypes.RemoveAll(x => x.FamilyName != familyName);

                        if (elementTypes.Count == 0)
                            continue;

                        ElementType elementType = elementTypes.First().Duplicate(familyTypeName);
                        return elementType;
                    }
                    else
                    {
                        // Duplicate Type for object which is Family Symbol

                        Family family = bHoMObject.Family(document);
                        if (family == null && builtInCategory != Autodesk.Revit.DB.BuiltInCategory.INVALID)
                        {
                            // Load and Duplicate Type from not existsing Family

                            string categoryName = builtInCategory.CategoryName(document);
                            if (string.IsNullOrEmpty(categoryName))
                                categoryName = bHoMObject.CategoryName();

                            if (familyLoadSettings != null)
                            {
                                if (!string.IsNullOrEmpty(familyName))
                                {
                                    FamilySymbol familySymbol = familyLoadSettings.LoadFamilySymbol(document, categoryName, familyName);
                                    if (familySymbol != null)
                                    {
                                        if (!familySymbol.IsActive)
                                            familySymbol.Activate();

                                        familySymbol.Name = familyTypeName;
                                        return familySymbol;
                                    }

                                }
                            }
                        }
                        else
                        {
                            // Duplicate from existing family

                            ISet<ElementId> elementIDs = family.GetFamilySymbolIds();
                            if (elementIDs != null && elementIDs.Count > 0)
                            {
                                FamilySymbol familySymbol = document.GetElement(elementIDs.First()) as FamilySymbol;
                                if (familySymbol != null)
                                {
                                    if (!familySymbol.IsActive)
                                        familySymbol.Activate();

                                    return familySymbol.Duplicate(familyTypeName);
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        /***************************************************/

        public static ElementType ElementType(this IBHoMObject bHoMObject, Document document, BuiltInCategory builtInCategory, FamilyLoadSettings familyLoadSettings = null, bool duplicateTypeIfNotExists = true)
        {
            return ElementType(bHoMObject, document, new BuiltInCategory[] { builtInCategory }, familyLoadSettings, duplicateTypeIfNotExists);
        }


        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        static private bool TryGetRevitNames(this IBHoMObject bHoMObject, out string familyName, out string familyTypeName)
        {
            familyTypeName = bHoMObject.FamilyTypeName();
            if (string.IsNullOrEmpty(familyTypeName))
            {
                familyTypeName = bHoMObject.Name;
                if (familyTypeName != null && familyTypeName.Contains(":"))
                    familyTypeName = BH.Engine.Adapters.Revit.Query.FamilyTypeName(familyTypeName);
            }

            familyName = bHoMObject.FamilyName();
            if (string.IsNullOrEmpty(familyName))
            {
                string famName = bHoMObject.Name;
                if (famName != null && famName.Contains(":"))
                    familyName = BH.Engine.Adapters.Revit.Query.FamilyName(famName);
            }

            return !string.IsNullOrWhiteSpace(familyName) || !string.IsNullOrWhiteSpace(familyTypeName);
        }

        /***************************************************/
    }
}

