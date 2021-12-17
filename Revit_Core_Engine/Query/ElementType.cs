/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****             Interface methods             ****/
        /***************************************************/

        [Description("Returns the Revit element type to be used when converting a given BHoM object to Revit.")]
        [Input("bHoMObject", "BHoM object to find a correspondent Revit element type for.")]
        [Input("document", "Revit document to parse in search for the element type.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("elementType", "Revit element type to be used when converting the input BHoM object to Revit.")]
        public static ElementType IElementType(this IBHoMObject bHoMObject, Document document, RevitSettings settings = null)
        {
            return ElementType(bHoMObject as dynamic, document, settings);
        }


        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns the Revit wall type to be used when converting a given BHoM wall to Revit.")]
        [Input("wall", "BHoM wall to find a correspondent Revit element type for.")]
        [Input("document", "Revit document to parse in search for the element type.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("wallType", "Revit wall type to be used when converting the input BHoM object to Revit.")]
        public static WallType ElementType(this BH.oM.Physical.Elements.Wall wall, Document document, RevitSettings settings = null)
        {
            return wall.ElementType<WallType>(document);
        }

        /***************************************************/

        [Description("Returns the Revit floor type to be used when converting a given BHoM floor to Revit.")]
        [Input("floor", "BHoM floor to find a correspondent Revit element type for.")]
        [Input("document", "Revit document to parse in search for the element type.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("floorType", "Revit floor type to be used when converting the input BHoM object to Revit.")]
        public static FloorType ElementType(this BH.oM.Physical.Elements.Floor floor, Document document, RevitSettings settings = null)
        {
            return floor.ElementType<FloorType>(document);
        }

        /***************************************************/

        [Description("Returns the Revit roof type to be used when converting a given BHoM roof to Revit.")]
        [Input("roof", "BHoM roof to find a correspondent Revit element type for.")]
        [Input("document", "Revit document to parse in search for the element type.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("roofType", "Revit roof type to be used when converting the input BHoM object to Revit.")]
        public static RoofType ElementType(this BH.oM.Physical.Elements.Roof roof, Document document, RevitSettings settings = null)
        {
            return roof.ElementType<RoofType>(document);
        }

        /***************************************************/

        [Description("Returns the Revit rebar bar type to be used when converting a given BHoM reinforcing bar to Revit.")]
        [Input("bar", "BHoM reinforcing bar to find a correspondent Revit element type for.")]
        [Input("document", "Revit document to parse in search for the element type.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("rebarBarType", "Revit rebar bar type to be used when converting the input BHoM object to Revit.")]
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

        [Description("Returns the Revit host object attributes to be used when converting a given BHoM construction to Revit.")]
        [Input("construction", "BHoM construction to find a correspondent Revit element type for.")]
        [Input("document", "Revit document to parse in search for the element type.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("attributes", "Revit host object attributes to be used when converting the input BHoM object to Revit.")]
        public static HostObjAttributes ElementType(this oM.Physical.Constructions.IConstruction construction, Document document, IEnumerable<BuiltInCategory> builtInCategories, RevitSettings settings = null)
        {
            return construction.ElementType<HostObjAttributes>(document, builtInCategories);
        }

        /***************************************************/

        [Description("Returns the Revit element type to be used when converting a given BHoM object to Revit.")]
        [Input("bHoMObject", "BHoM object to find a correspondent Revit element type for.")]
        [Input("document", "Revit document to parse in search for the element type.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("elementType", "Revit element type to be used when converting the input BHoM object to Revit.")]
        public static ElementType ElementType(this IBHoMObject bHoMObject, Document document, RevitSettings settings = null)
        {
            return bHoMObject.ElementType(document, bHoMObject.BuiltInCategories(document), settings);
        }

        /***************************************************/

        [Description("Returns the Revit element type to be used when converting a given BHoM object to Revit.")]
        [Input("bHoMObject", "BHoM object to find a correspondent Revit element type for.")]
        [Input("document", "Revit document to parse in search for the element type.")]
        [Input("builtInCategories", "Revit built-in categories to parse in search for the element type. If left empty, all categories to be used.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("elementType", "Revit element type to be used when converting the input BHoM object to Revit.")]
        public static ElementType ElementType(this IBHoMObject bHoMObject, Document document, IEnumerable<BuiltInCategory> builtInCategories, RevitSettings settings = null)
        {
            if (bHoMObject == null || document == null)
                return null;

            string familyName, familyTypeName;
            bHoMObject.FamilyAndTypeNames(out familyName, out familyTypeName);

            return document.ElementType(familyName, familyTypeName, builtInCategories, settings);
        }

        /***************************************************/

        [Description("Returns the Revit element type matching the family and type name as well as a collection of built-in categories.")]
        [Input("document", "Revit document to parse in search for the element type.")]
        [Input("familyName", "Family name to search for.")]
        [Input("familyTypeName", "Family type name to search for.")]
        [Input("builtInCategories", "Revit built-in categories to parse in search for the element type. If left empty, all categories to be used.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("elementType", "Revit element type matching the input family and type name as well as a collection of built-in categories.")]
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
            familyName = bHoMObject.Name.FamilyName();
            familyTypeName = bHoMObject.Name.FamilyTypeName();

            if (string.IsNullOrEmpty(familyTypeName))
                familyTypeName = bHoMObject.Name;
        }

        /***************************************************/
    }
}



