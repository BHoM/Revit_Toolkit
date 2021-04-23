/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using Autodesk.Revit.DB.Structure;
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

        public static RebarBarType ElementType(this oM.Physical.Reinforcement.IReinforcingBar bar, Document document, RevitSettings settings = null)
        {
            RebarBarType result = bar.ElementType<RebarBarType>(document);
            if (result == null)
            {
                string barTypeName = ((int)(bar.Diameter * 1000)).ToString();
                return new FilteredElementCollector(document).OfClass(typeof(RebarBarType)).Where(x => x.Name == barTypeName).FirstOrDefault() as RebarBarType;
            }

            return result;
        }

        /***************************************************/

        public static HostObjAttributes ElementType(this oM.Physical.Constructions.IConstruction construction, Document document, IEnumerable<BuiltInCategory> builtInCategories, RevitSettings settings = null)
        {
            return construction.ElementType<HostObjAttributes>(document, builtInCategories);
        }

        /***************************************************/

        public static ElementType ElementType(this IBHoMObject bHoMObject, Document document, RevitSettings settings = null)
        {
            return bHoMObject.ElementType(document, bHoMObject.BuiltInCategories(document), settings);
        }

        /***************************************************/

        public static ElementType ElementType(this IBHoMObject bHoMObject, Document document, IEnumerable<BuiltInCategory> builtInCategories, RevitSettings settings = null)
        {
            if (bHoMObject == null || document == null)
                return null;

            string familyName, familyTypeName;
            bHoMObject.FamilyAndTypeNames(out familyName, out familyTypeName);

            return document.ElementType(familyName, familyTypeName, builtInCategories);
        }

        /***************************************************/

        public static ElementType ElementType(this Document document, string familyName, string familyTypeName, IEnumerable<BuiltInCategory> builtInCategories = null, RevitSettings settings = null)
        {
            if (string.IsNullOrEmpty(familyTypeName))
                return null;

            settings = settings.DefaultIfNull();

            ElementType elementType = document.ElementType<ElementType>(familyName, familyTypeName, builtInCategories);
            if (elementType != null)
            {
                elementType.Activate();
                return elementType;
            }

            //Find ElementType in FamilyLibrary
            if (settings.FamilyLoadSettings?.FamilyLibrary?.Files != null)
            {
                if (builtInCategories != null && builtInCategories.Any(x => x != Autodesk.Revit.DB.BuiltInCategory.INVALID))
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
                else
                    return settings.FamilyLoadSettings.LoadFamilySymbol(document, null, familyName, familyTypeName);
            }

            return null;
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static T ElementType<T>(this IBHoMObject bHoMObject, Document document, IEnumerable<BuiltInCategory> builtInCategories = null) where T : ElementType
        {
            string familyName, familyTypeName;
            bHoMObject.FamilyAndTypeNames(out familyName, out familyTypeName);

            return document.ElementType<T>(familyName, familyTypeName, builtInCategories);
        }

        /***************************************************/

        private static T ElementType<T>(this Document document, string familyName, string familyTypeName, IEnumerable<BuiltInCategory> builtInCategories = null) where T : ElementType
        {
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


