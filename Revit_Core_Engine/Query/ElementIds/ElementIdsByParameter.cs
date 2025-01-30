/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using BH.Engine.Verification;
using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Mapping;
using BH.oM.Adapters.Revit.Requests;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;
using BH.oM.Data.Requests;
using BH.oM.Revit.Requests;
using BH.oM.Verification.Conditions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using BH.Engine.Adapters.Revit;
using System.Linq;
using BH.Engine.Base;
using BH.Engine.Diffing;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****             Interface methods             ****/
        /***************************************************/

        //[Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given IParameterRequest.")]
        //[Input("document", "Revit Document queried for the filtered elements.")]
        //[Input("request", "IParameterRequest containing the filtering criteria, against which the elements in the Revit document are checked.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        //[Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        //[Output("elementIds", "Collection of filtered Revit ElementIds.")]
        //public static IEnumerable<ElementId> IElementIdsByParameter(this Document document, IParameterRequest request, Discipline discipline, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        //{
        //    if (document == null)
        //        return null;

        //    if (ids != null && !ids.Any())
        //        return new List<ElementId>();

        //    FilteredElementCollector collector = ids == null ? new FilteredElementCollector(document) : new FilteredElementCollector(document, ids.ToList());
        //    collector = collector.WherePasses(new LogicalOrFilter(new ElementIsElementTypeFilter(), new ElementIsElementTypeFilter(true)));
        //    return collector.Where(x => x.IPasses(request, discipline, settings)).Select(x => x.Id).ToList();
        //}

        public static List<ElementId> ElementIdsByCondition(this Document document, ConditionRequest request, Discipline discipline, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            if (document == null)
                return null;

            if (ids != null && !ids.Any())
                return new List<ElementId>();

            FilteredElementCollector collector = ids == null ? new FilteredElementCollector(document) : new FilteredElementCollector(document, ids.ToList());
            List<Element> elements = collector.WherePasses(new LogicalOrFilter(new ElementIsElementTypeFilter(), new ElementIsElementTypeFilter(true))).ToList();

            if (request.Condition is IValueCondition valueCondition && valueCondition.ValueSource is ParameterValueSource pvs)
            {
                Dictionary<ICondition, List<Element>> withMapping = AddParameterMapping(valueCondition, elements, discipline, settings);
                return withMapping.SelectMany(x => x.Value.Where(y => y.IPasses(x.Key) == true)).Select(x => x.Id).ToList();
            }
            else
                return elements.Where(x => x.IPasses(request.Condition) == true).Select(x => x.Id).ToList();
        }

        /***************************************************/

        private static Dictionary<ICondition, List<Element>> AddParameterMapping(IValueCondition valueCondition, IEnumerable<Element> elements, Discipline discipline, RevitSettings settings)
        {
            string paramName = ((ParameterValueSource)valueCondition.ValueSource).ParameterName;
            Dictionary<Element, Type> bhomTypes = elements.ToDictionary(x => x, x => x.IBHoMType(discipline));
            Dictionary<Type, ICondition> conditionByType = new Dictionary<Type, ICondition>();
            foreach (Type bhomType in bhomTypes.Values.Distinct())
            {
                ICondition condition = valueCondition;
                oM.Adapters.Revit.Mapping.ParameterMap parameterMap = settings?.MappingSettings?.ParameterMap(bhomType);
                if (parameterMap != null)
                {
                    List<ParameterValueSource> mappedParams = new List<ParameterValueSource>();
                    IEnumerable<string> elementParameterNames = parameterMap.ParameterLinks.Where(x => x is ElementParameterLink && x.PropertyName == paramName).SelectMany(x => x.ParameterNames);
                    mappedParams.AddRange(elementParameterNames.Select(x => new ParameterValueSource { ParameterName = x }));

                    List<string> typeParameterNames = parameterMap.ParameterLinks.Where(x => x is ElementTypeParameterLink && x.PropertyName == paramName).SelectMany(x => x.ParameterNames).ToList();
                    mappedParams.AddRange(typeParameterNames.Select(x => new ParameterValueSource { ParameterName = x, FromType = true }));

                    List<ICondition> conditions = new List<ICondition> { valueCondition };
                    foreach (ParameterValueSource mappedParam in mappedParams)
                    {
                        IValueCondition newCondition = valueCondition.ShallowClone();
                        newCondition.ValueSource = mappedParam;
                        conditions.Add(newCondition);
                    }

                    condition = new LogicalOrCondition { Conditions = conditions };
                }

                conditionByType[bhomType] = condition;
            }
         
            Dictionary<ICondition, List<Element>> result = new Dictionary<ICondition, List<Element>>();
            foreach (var kvp in conditionByType)
            {
                List<Element> elementsOfType = bhomTypes.Where(x => x.Value == kvp.Key).Select(x => x.Key).ToList();
                ICondition existingKey = result.Keys.FirstOrDefault(x => x.IsEqual(kvp.Value));
                if (existingKey == null)
                    result[kvp.Value] = new List<Element>();

                result[kvp.Value].AddRange(elementsOfType);
            }

            return result;
        }
    }
}




