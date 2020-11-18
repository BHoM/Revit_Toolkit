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
        /****             Interface methods             ****/
        /***************************************************/

        public static ElementType IElementType(this IBHoMObject bHoMObject, Document document, RevitSettings settings = null)
        {
            return ElementType(bHoMObject as dynamic, document, settings);
        }


        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static WallType ElementType(this BH.oM.Physical.Elements.Wall wall, Document document, RevitSettings settings = null)
        {
            return wall.ElementType<WallType>(document);
        }

        /***************************************************/

        public static FloorType ElementType(this BH.oM.Physical.Elements.Floor floor, Document document, RevitSettings settings = null)
        {
            return floor.ElementType<FloorType>(document);
        }

        /***************************************************/

        public static RoofType ElementType(this BH.oM.Physical.Elements.Roof roof, Document document, RevitSettings settings = null)
        {
            return roof.ElementType<RoofType>(document);
        }

        /***************************************************/

        public static ElementType ElementType(this IBHoMObject bHoMObject, Document document, RevitSettings settings = null)
        {
            return bHoMObject.ElementType(document, bHoMObject.BuiltInCategories(document), settings);
        }

        /***************************************************/

        public static HostObjAttributes ElementType(this oM.Physical.Constructions.IConstruction construction, Document document, IEnumerable<BuiltInCategory> builtInCategories, RevitSettings settings = null)
        {
            return construction.ElementType<HostObjAttributes>(document, builtInCategories);
        }

        /***************************************************/

        public static ElementType ElementType(this IBHoMObject bHoMObject, Document document, IEnumerable<BuiltInCategory> builtInCategories, RevitSettings settings = null)
        {
            if (bHoMObject == null || document == null)
                return null;

            settings = settings.DefaultIfNull();

            string familyName, familyTypeName;
            bHoMObject.FamilyAndTypeNames(out familyName, out familyTypeName);

            if (string.IsNullOrEmpty(familyTypeName))
                return null;

            ElementType elementType = document.ElementType(familyName, familyTypeName, builtInCategories);
            if (elementType != null)
                return elementType;

            //Find ElementType in FamilyLibrary
            if (settings.FamilyLoadSettings != null && !string.IsNullOrWhiteSpace(familyName) && !string.IsNullOrWhiteSpace(familyTypeName))
            {
                foreach (BuiltInCategory builtInCategory in builtInCategories)
                {
                    if (builtInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID)
                        continue;

                    FamilySymbol familySymbol = settings.FamilyLoadSettings.LoadFamilySymbol(document, builtInCategory.CategoryName(document), familyName, familyTypeName);
                    if (familySymbol != null)
                        return familySymbol;
                }
            }
            
            return null;
        }

        /***************************************************/

        public static ElementType ElementType(this Document document, string familyName, string familyTypeName, IEnumerable<BuiltInCategory> builtInCategories)
        {
            if (document == null || string.IsNullOrEmpty(familyTypeName))
                return null;

            if (!string.IsNullOrWhiteSpace(familyName))
            {
                IEnumerable<Element> collector = new FilteredElementCollector(document).OfClass(typeof(Family));
                if (builtInCategories != null && builtInCategories.Any(x => x != Autodesk.Revit.DB.BuiltInCategory.INVALID))
                    collector = collector.Where(x => builtInCategories.Any(y => ((Family)x).FamilyCategoryId.IntegerValue == (int)y));

                Family fam = collector.FirstOrDefault(x => x.Name == familyName) as Family;
                if (fam != null)
                {
                    foreach (ElementId id in fam.GetFamilySymbolIds())
                    {
                        ElementType et = document.GetElement(id) as ElementType;
                        if (et?.Name == familyTypeName)
                            return et;
                    }
                }
            }
            else
            {
                FilteredElementCollector collector = new FilteredElementCollector(document).OfClass(typeof(ElementType));
                if (builtInCategories != null && builtInCategories.Any(x => x != Autodesk.Revit.DB.BuiltInCategory.INVALID))
                    collector = collector.WherePasses(new LogicalOrFilter(builtInCategories.Where(x => x != Autodesk.Revit.DB.BuiltInCategory.INVALID).Select(x => new ElementCategoryFilter(x) as ElementFilter).ToList()));

                return collector.FirstOrDefault(x => x.Name == familyTypeName) as ElementType;
            }

            return null;
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static T ElementType<T>(this IBHoMObject bHoMObject, Document document, IEnumerable<BuiltInCategory> builtInCategories = null) where T : HostObjAttributes
        {
            string familyName, familyTypeName;
            bHoMObject.FamilyAndTypeNames(out familyName, out familyTypeName);
            if (string.IsNullOrEmpty(familyTypeName))
                return null;

            FilteredElementCollector collector = new FilteredElementCollector(document).OfClass(typeof(T));
            if (builtInCategories != null && builtInCategories.Any(x => x != Autodesk.Revit.DB.BuiltInCategory.INVALID))
                collector = collector.WherePasses(new LogicalOrFilter(builtInCategories.Where(x => x != Autodesk.Revit.DB.BuiltInCategory.INVALID).Select(x => new ElementCategoryFilter(x) as ElementFilter).ToList()));

            if (!string.IsNullOrWhiteSpace(familyName))
                return collector.Cast<T>().FirstOrDefault(x => x.FamilyName == familyName && x.Name == familyTypeName);
            else
                return collector.FirstOrDefault(x => x.Name == familyTypeName) as T;
        }

        /***************************************************/

        private static void FamilyAndTypeNames(this IBHoMObject bHoMObject, out string familyName, out string familyTypeName)
        {
            familyName = bHoMObject.FamilyName();
            if (string.IsNullOrEmpty(familyName))
                familyName = bHoMObject.Name.FamilyName();

            familyTypeName = bHoMObject.FamilyTypeName();
            if (string.IsNullOrEmpty(familyTypeName))
                familyTypeName = bHoMObject.Name.FamilyTypeName();

            if (string.IsNullOrEmpty(familyTypeName))
                familyTypeName = bHoMObject.Name;
        }

        /***************************************************/
    }
}

