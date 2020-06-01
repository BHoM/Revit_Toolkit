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
using BH.oM.Base;
using BH.oM.Physical.Elements;
using BH.oM.Physical.Materials;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
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

            HashSet<ElementId> elementIds = new HashSet<ElementId>();
            if (!typeof(IBHoMObject).IsAssignableFrom(type))
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("Input type {0} is not a BHoM type", type));
                return elementIds;
            }

            if (type == typeof(ModelInstance) || type == typeof(IInstance) || type == typeof(InstanceProperties) || type == typeof(IBHoMObject) || type == typeof(BHoMObject) || type == typeof(IMaterialProperties))
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("It is not allowed to pull elements of type {0} because it is too general, please try to narrow the filter down.", type));
                return elementIds;
            }

            if (ids != null && ids.Count() == 0)
                return elementIds;

            // Special case - filter drafting instances
            if (type == typeof(DraftingInstance))
                return new FilteredElementCollector(document).WhereElementIsNotElementType().WherePasses(new ElementCategoryFilter(Autodesk.Revit.DB.BuiltInCategory.OST_SketchLines, true)).Where(x => x.ViewSpecific && (x is FilledRegion || x.Location is LocationPoint || x.Location is LocationCurve)).Select(x => x.Id);
            
            // Else go the regular way
            List<ElementFilter> filters = new List<ElementFilter>();
            List<Tuple<Type, Type>> unsupportedAPITypes = new List<Tuple<Type, Type>>();
            IEnumerable<Type> types = type.RevitTypes();
            if (types == null || types.Count() == 0)
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("Type {0} is not coupled with any Revit type that could be converted.", type));
                return elementIds;
            }

            foreach (Type t in types)
            {
                if (t == typeof(FamilyInstance) || t == typeof(FamilySymbol))
                {
                    IEnumerable<BuiltInCategory> builtInCategories = type.BuiltInCategories();
                    ElementFilter filter = new LogicalAndFilter(new ElementClassFilter(t), new LogicalOrFilter(builtInCategories.Select(x => new ElementCategoryFilter(x) as ElementFilter).ToList()));

                    // Accommodate the fact that bracing can sit in category OST_StructuralFraming etc.
                    filter = filter.StructuralTypeFilter(type);
                    filters.Add(filter);
                }
                else if (t.IsSupportedAPIType())
                    filters.Add(new ElementClassFilter(t));
                else
                    unsupportedAPITypes.Add(new Tuple<Type, Type>(t, t.SupportedAPIType()));
            }

            FilteredElementCollector collector = ids == null ? new FilteredElementCollector(document) : new FilteredElementCollector(document, ids.ToList());
            elementIds.UnionWith(collector.WherePasses(new LogicalOrFilter(filters)).ToElementIds());
            foreach (Tuple<Type, Type> unsupportedAPIType in unsupportedAPITypes)
            {
                elementIds.UnionWith(collector.OfClass(unsupportedAPIType.Item2).Where(x => x.GetType() == unsupportedAPIType.Item1).Select(x => x.Id));
            }
            
            // Filter only structural bars
            if (type == typeof(BH.oM.Structure.Elements.Bar))
                elementIds = elementIds.RemoveNonStructuralFraming(document);

            // Filter only structural panels
            if (type == typeof(BH.oM.Structure.Elements.Panel))
                elementIds = elementIds.RemoveNonStructuralPanels(document);

            // Filter out analytical spaces that are already pulled as non-analytical
            if (type == typeof(BH.oM.Environment.Elements.Space) || type == typeof(BH.oM.Environment.Elements.Panel))
                elementIds = new HashSet<ElementId>(elementIds.RemoveDuplicateAnalyticalElements(document));

            // Filter out walls that are parts of stacked wall
            if (types.Any(x => typeof(Autodesk.Revit.DB.Wall).IsAssignableFrom(x)))
                elementIds = elementIds.RemoveStackedWallParts(document);

            //Revit returns additional "parent" Autodesk.Revit.DB.Panel with no geometry when pulling all panels from model. This part of the code filter them out
            if (type == typeof(Window))
                elementIds = elementIds.RemoveEmptyPanelIds(document);

            return elementIds;
        }

        /***************************************************/

    }
}