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
 
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using BH.oM.Base;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Interface;
using BH.oM.Data.Requests;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static IEnumerable<ElementId> ElementIdsByBHoMType(this Document document, Type type, IEnumerable<ElementId> ids = null)
        {
            if (document == null || type == null)
                return null;

            IEnumerable<Type> types = null;
            IEnumerable<BuiltInCategory> builtInCategories = null;

            //TODO: this could raise wrong type error and return straight away? Or even better: if the type does not inherit IBHoMObject, raise error and return null.
            if (type.IsAssignableFromByFullName(typeof(Element)))
                types = new List<Type>() { type };
            else if (typeof(IBHoMObject).IsAssignableFrom(type))
            {
                types = RevitTypes(type);
                builtInCategories = BuiltInCategories(type);
            }

            //TODO: the list by default should contain types that inherit from Element, is that relevant?
            if (types != null)
                types = types.ToList().FindAll(x => x.IsAssignableFromByFullName(typeof(Element)));

            IEnumerable<ElementId> elementIDs = null;

            FilteredElementCollector collector = ids == null ? new FilteredElementCollector(document) : new FilteredElementCollector(document, ids.ToList());
            if (types == null || types.Count() == 0)
            {
                if ((builtInCategories != null && builtInCategories.Count() != 0))
                    elementIDs = collector.WherePasses(new LogicalOrFilter(builtInCategories.ToList().ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter))).ToElementIds();
                else
                    return null;
            }

            //Some of the Revit API types do not exist in the object model of Revit - they cannot be filtered...
            List<Type> supportedAPITypes = new List<Type>();
            List<Tuple<Type, Type>> unsupportedAPITypes = new List<Tuple<Type, Type>>();
            foreach (Type t in types)
            {
                Type supportedAPIType = t.SupportedAPIType();
                if (supportedAPIType != t)
                    unsupportedAPITypes.Add(new Tuple<Type, Type>(t, supportedAPIType));
                else
                    supportedAPITypes.Add(t);
            }

            if (supportedAPITypes.Count != 0)
            {
                if ((builtInCategories == null || builtInCategories.Count() == 0))
                    elementIDs = collector.WherePasses(new LogicalOrFilter(supportedAPITypes.ToList().ConvertAll(x => new ElementClassFilter(x) as ElementFilter))).ToElementIds();
                else
                    elementIDs = collector.WherePasses(new LogicalAndFilter(new LogicalOrFilter(supportedAPITypes.ToList().ConvertAll(x => new ElementClassFilter(x) as ElementFilter)), new LogicalOrFilter(builtInCategories.ToList().ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter)))).ToElementIds();
            }

            if (unsupportedAPITypes.Count != 0)
            {
                HashSet<ElementId> extraElementIds = new HashSet<ElementId>();
                foreach (Tuple<Type, Type> unsupportedAPIType in unsupportedAPITypes)
                {
                    IEnumerable<ElementId> elementIds;
                    if ((builtInCategories == null || builtInCategories.Count() == 0))
                        elementIds = collector.WherePasses(new ElementClassFilter(unsupportedAPIType.Item2)).ToElementIds();
                    else
                        elementIds = collector.WherePasses(new LogicalAndFilter(new ElementClassFilter(unsupportedAPIType.Item2), new LogicalOrFilter(builtInCategories.ToList().ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter)))).ToElementIds();

                    foreach (ElementId id in elementIds)
                    {
                        if (unsupportedAPIType.Item1.IsAssignableFrom(document.GetElement(id).GetType()))
                            extraElementIds.Add(id);
                    }
                }

                foreach (ElementId id in elementIDs)
                {
                    extraElementIds.Add(id);
                }

                elementIDs = extraElementIds;
            }

            //Special Cases
            if (elementIDs != null && elementIDs.Count() != 0)
            {
                if (type == typeof(BH.oM.Adapters.Revit.Elements.ModelInstance))
                {
                    //OST_DetailComponents BuiltInCategory is wrongly set to CategoryType.Model.
                    int detailComponentsCategoryId = Category.GetCategory(document, Autodesk.Revit.DB.BuiltInCategory.OST_DetailComponents).Id.IntegerValue;

                    List<ElementId> newElementIds = new List<ElementId>();
                    foreach (ElementId id in elementIDs)
                    {
                        Category category = document.GetElement(id).Category;
                        if ((category.CategoryType == CategoryType.AnalyticalModel || category.CategoryType == CategoryType.Model) && category.Id.IntegerValue != detailComponentsCategoryId)
                            newElementIds.Add(id);
                    }

                    elementIDs = newElementIds;
                }

                if (type == typeof(oM.Physical.Elements.Window))
                {
                    //Revit returns additional "parent" Autodesk.Revit.DB.Panel with no geometry when pulling all panels from model. This part of the code filter them out
                    List<ElementId> elementIDList = new List<ElementId>();
                    foreach (ElementId elementID in elementIDs)
                    {
                        Panel panel = document.GetElement(elementID) as Panel;
                        if (panel != null)
                        {
                            ElementId hostID = panel.FindHostPanel();
                            if (hostID != null && hostID != Autodesk.Revit.DB.ElementId.InvalidElementId)
                                continue;
                        }

                        elementIDList.Add(elementID);
                    }

                    elementIDs = elementIDList;
                }
            }

            return elementIDs;
        }

        /***************************************************/

    }
}