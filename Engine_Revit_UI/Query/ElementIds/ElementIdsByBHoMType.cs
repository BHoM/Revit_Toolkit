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
using BH.oM.Adapters.Revit.Interface;
using BH.oM.Adapters.Revit.Properties;
using BH.oM.Base;
using BH.oM.Physical.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

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

            HashSet<ElementId> elementIDs = new HashSet<ElementId>();
            if (!typeof(IBHoMObject).IsAssignableFrom(type))
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("Input type {0} is not a BHoM type", type));
                return elementIDs;
            }

            if (type == typeof(ModelInstance) || type == typeof(IInstance) || type == typeof(InstanceProperties) || type == typeof(IBHoMObject) || type == typeof(BHoMObject))
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("It is not allowed to pull elements of type {0} because it is too general, please try to narrow the filter down.", type));
                return elementIDs;
            }

            if (ids != null && ids.Count() == 0)
                return elementIDs;

            List<ElementFilter> filters = new List<ElementFilter>();
            List<Tuple<Type, Type>> unsupportedAPITypes = new List<Tuple<Type, Type>>();
            IEnumerable<Type> types = type.RevitTypes();
            if (types == null || types.Count() == 0)
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("Type {0} is not coupled with any Revit type that could be converted.", type));
                return elementIDs;
            }

            foreach (Type t in types)
            {
                if (t == typeof(FamilyInstance) || t == typeof(FamilySymbol))
                {
                    IEnumerable<BuiltInCategory> builtInCategories = type.BuiltInCategories();
                    filters.Add(new LogicalAndFilter(new ElementClassFilter(t), new LogicalOrFilter(builtInCategories.Select(x => new ElementCategoryFilter(x) as ElementFilter).ToList())));
                }
                else if (t.IsSupportedAPIType())
                    filters.Add(new ElementClassFilter(t));
                else
                    unsupportedAPITypes.Add(new Tuple<Type, Type>(t, t.SupportedAPIType()));
            }

            FilteredElementCollector collector = ids == null ? new FilteredElementCollector(document) : new FilteredElementCollector(document, ids.ToList());
            elementIDs.UnionWith(collector.WherePasses(new LogicalOrFilter(filters)).ToElementIds());
            foreach (Tuple<Type, Type> unsupportedAPIType in unsupportedAPITypes)
            {
                elementIDs.UnionWith(collector.OfClass(unsupportedAPIType.Item2).Where(x => x.GetType() == unsupportedAPIType.Item1).Select(x => x.Id));
            }

            // Filter drafting instances
            if (type == typeof(DraftingInstance))
                elementIDs = new HashSet<ElementId>(elementIDs.Where(x => document.GetElement(x).ViewSpecific));

            //Revit returns additional "parent" Autodesk.Revit.DB.Panel with no geometry when pulling all panels from model. This part of the code filter them out
            if (type == typeof(Window))
                elementIDs = elementIDs.RemoveEmptyPanelIds(document);

            return elementIDs;
        }

        /***************************************************/

    }
}