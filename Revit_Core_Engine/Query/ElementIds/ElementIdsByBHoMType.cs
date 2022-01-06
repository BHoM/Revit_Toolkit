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
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Properties;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Physical.Elements;
using BH.oM.Physical.FramingProperties;
using BH.oM.Physical.Materials;
using BH.oM.Reflection.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that will be converted to a given BHoM type for given discipline.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("type", "Target BHoM type, to which all filtered Revit elements will be converted.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIdsByBHoMType(this Document document, Type type, RevitSettings settings, IEnumerable<ElementId> ids = null)
        {
            if (document == null || type == null)
                return null;

            settings = settings.DefaultIfNull();

            HashSet<ElementId> elementIds = new HashSet<ElementId>();
            if (!typeof(IBHoMObject).IsAssignableFrom(type))
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("Input type {0} is not a BHoM type.", type));
                return elementIds;
            }

            if (type == typeof(ModelInstance) || type == typeof(IInstance) || type == typeof(InstanceProperties) || type == typeof(IBHoMObject) || type == typeof(BHoMObject) || type == typeof(IMaterialProperties))
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("It is not allowed to pull elements of type {0} because it is too general, please try to narrow the filter down.", type));
                return elementIds;
            }

            if (typeof(IFramingElementProperty).IsAssignableFrom(type))
            {
                BH.Engine.Reflection.Compute.RecordError("It is impossible to pull objects of types that inherit from IFramingElementProperty because they do not have equivalents in Revit. To pull Revit framing types, try pulling IProfiles instead.");
                return elementIds;
            }

            if (ids != null && ids.Count() == 0)
                return elementIds;

            FilteredElementCollector collector = ids == null ? new FilteredElementCollector(document) : new FilteredElementCollector(document, ids.ToList());

            //Special case - filter drafting instances
            if (type == typeof(DraftingInstance))
            {
                elementIds.UnionWith(collector.WhereElementIsNotElementType().WherePasses(new ElementCategoryFilter(Autodesk.Revit.DB.BuiltInCategory.OST_SketchLines, true)).Where(x => x.ViewSpecific && (x is FilledRegion || x.Location is LocationPoint || x.Location is LocationCurve)).Select(x => x.Id));
                return elementIds;
            }

            // Else go the regular way
            IEnumerable<Type> types = type.RevitTypes();
            if (types == null || types.Count() == 0)
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("Type {0} is not coupled with any Revit type that could be converted.", type));
                return elementIds;
            }

            Discipline discipline = type.Discipline();
            if (discipline == Discipline.Undefined)
                discipline = Discipline.Physical;

            List<ElementFilter> filters = new List<ElementFilter>();
            List<Tuple<Type, Type>> unsupportedAPITypes = new List<Tuple<Type, Type>>();
            foreach (Type t in types)
            {
                if (t.IsSupportedAPIType())
                    filters.Add(new ElementClassFilter(t));
                else
                    unsupportedAPITypes.Add(new Tuple<Type, Type>(t, t.SupportedAPIType()));
            }
            
            elementIds.UnionWith(collector.WherePasses(new LogicalOrFilter(filters)).ToElementIds());
            foreach (Tuple<Type, Type> unsupportedAPIType in unsupportedAPITypes)
            {
                elementIds.UnionWith(collector.OfClass(unsupportedAPIType.Item2).Where(x => x.GetType() == unsupportedAPIType.Item1).Select(x => x.Id));
            }

            // Only take the Revit elements that will actually be converted to requested BHoM type
            elementIds = new HashSet<ElementId>(elementIds.Where(x => type.IsAssignableFrom(document.GetElement(x).IBHoMType(discipline, settings))));

            // Filter out analytical spaces that are already pulled as non-analytical
            if (type == typeof(BH.oM.Environment.Elements.Space) || type == typeof(BH.oM.Environment.Elements.Panel))
                elementIds = new HashSet<ElementId>(elementIds.RemoveDuplicateAnalyticalElements(document));

            // Filter out walls that are parts of stacked wall
            if (types.Any(x => typeof(Autodesk.Revit.DB.Wall).IsAssignableFrom(x)))
                elementIds = elementIds.RemoveStackedWallParts(document);

            //Revit returns additional "parent" Autodesk.Revit.DB.Panel with no geometry when pulling all panels from model. This part of the code filter them out
            if (type == typeof(Window))
                elementIds = elementIds.RemoveEmptyPanelIds(document);

            type.CheckFamilyMapUsage(document, settings.MappingSettings);

            return elementIds;
        }

        /***************************************************/

    }
}

