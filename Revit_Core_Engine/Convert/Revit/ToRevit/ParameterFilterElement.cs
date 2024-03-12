/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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

using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.MEP.Equipment.Parts;
using BH.oM.Physical.Elements;
using BH.oM.Revit.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Document = Autodesk.Revit.DB.Document;
using FilterRule = BH.oM.Revit.Views.FilterRule;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts BH.oM.Adapters.Revit.Elements.ViewFilter to a Revit ParameterFilterElement." +
                     "\nThe Filter Rules are assigned after its creation due to Revit API limitations.")]
        [Input("filter", "BH.oM.Adapters.Revit.Elements.ViewFilter to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("filter", "Revit ParameterFilterElement resulting from converting the input BH.oM.Adapters.Revit.Elements.ViewFilter.")]
        public static ParameterFilterElement ToRevitParameterFilterElement(this oM.Adapters.Revit.Elements.ViewFilter filter, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            ParameterFilterElement revitFilter = refObjects.GetValue<ParameterFilterElement>(document, filter.BHoM_Guid);
            if (revitFilter != null)
                return revitFilter;

            // 1. CREATE PARAMETERFILTERELEMENT with ASSIGNED CATEGORIES LIST

            /* 1.1 Turn list of Category Names into List of Category ElementIds via use of Streams */

            List<ElementId> categoryIdsList = filter.Categories
                             // Format the string name of the categories
                             .Select(catName => catName.ToUpper().Replace(" ", ""))
                             // Get the corresponding BuiltInCategories
                             .Select(catName => { List<string> builtInCatNames = Enum.GetNames(typeof(BuiltInCategory))
                                                        .Select(builtInCategoryName => builtInCategoryName.ToUpper().Replace("ost_", ""))
                                                        .ToList();
                                                  return (BuiltInCategory)(((BuiltInCategory[])Enum.GetValues(typeof(BuiltInCategory)))[builtInCatNames.IndexOf(catName)]);})
                             // Get the ElementIds of the BuiltInCategories
                             .Select(builtInCat => new ElementId(builtInCat))
                             // Turn the Stream into a List of ElementIds
                             .ToList<ElementId>();
            
            /* 1.2 Create the ParameterFilterElement in the current Revit Document */
            revitFilter = ParameterFilterElement.Create(document, filter.Name, categoryIdsList);


            // 2. BUILD THE REVIT FILTER RULES and ASSIGN THEM TO THE PARAMETERFILTERELEMENT

            /* Via use of Streams*/
            revitFilter.SetElementFilter(new LogicalAndFilter(filter.Rules
                            .Select(filterRule => filterRuleToRevit(document, filterRule))
                            .Select(revitFilterRule => new ElementParameterFilter(revitFilterRule))
                            .Cast<ElementFilter>()
                            .ToList()));

            revitFilter.CopyParameters(filter, settings);
            refObjects.AddOrReplace(filter, revitFilter);
            return revitFilter;
        }

        /***************************************************/

        public static Autodesk.Revit.DB.FilterRule filterRuleToRevit(Document document,FilterRule filterRule) {

            Autodesk.Revit.DB.FilterRule revitFilterRule = null;

            ElementClassFilter paramFilter = new ElementClassFilter(typeof(Parameter));
            ElementClassFilter builtInParamFilter = new ElementClassFilter(typeof(BuiltInParameter));
            LogicalOrFilter logicalOrFilter = new LogicalOrFilter(paramFilter, builtInParamFilter);


            ElementId parameterId = new FilteredElementCollector(document)
                            .WherePasses(logicalOrFilter)
                            .Where(par => par.Name.Equals(filterRule.ParameterName))
                            .First()
                            .Id;

            switch (filterRule.RuleType) { 
                case FilterRuleType.BEGINSWITH:
                    revitFilterRule=ParameterFilterRuleFactory
                        .CreateBeginsWithRule(parameterId, (string) filterRule.Value, false);
                    break;
                case FilterRuleType.CONTAINS:
                    revitFilterRule = ParameterFilterRuleFactory
                        .CreateContainsRule(parameterId, (string) filterRule.Value, false);
                    break;
                case FilterRuleType.ENDSWITH:
                    revitFilterRule = ParameterFilterRuleFactory
                        .CreateEndsWithRule(parameterId, (string) filterRule.Value, false);
                    break;
                case FilterRuleType.EQUALS:
                    revitFilterRule = ParameterFilterRuleFactory
                        .CreateEqualsRule(parameterId, (int) filterRule.Value);
                    break;
                case FilterRuleType.GREATER:
                    revitFilterRule = ParameterFilterRuleFactory
                        .CreateGreaterRule(parameterId, (ElementId) filterRule.Value);
                    break;
                case FilterRuleType.GREATER_OR_EQUAL:
                    revitFilterRule = ParameterFilterRuleFactory
                        .CreateGreaterOrEqualRule(parameterId, (ElementId) filterRule.Value);
                    break;
                case FilterRuleType.LESS:
                    revitFilterRule = ParameterFilterRuleFactory
                        .CreateLessRule(parameterId, (ElementId) filterRule.Value);
                    break;
                case FilterRuleType.LESS_OR_EQUAL:
                    revitFilterRule = ParameterFilterRuleFactory
                        .CreateLessOrEqualRule(parameterId, (ElementId) filterRule.Value);
                    break;
                case FilterRuleType.NOT_BEGINSWITH:
                    revitFilterRule = ParameterFilterRuleFactory
                        .CreateNotBeginsWithRule(parameterId, (string) filterRule.Value, false);
                    break;
                case FilterRuleType.NOT_CONTAINS:
                    revitFilterRule = ParameterFilterRuleFactory
                        .CreateNotContainsRule(parameterId, (string) filterRule.Value, false);
                    break;
                case FilterRuleType.NOT_ENDSWITH:
                    revitFilterRule = ParameterFilterRuleFactory
                        .CreateNotEndsWithRule(parameterId, (string) filterRule.Value, false);
                    break;
                default:
                    break;}

            return revitFilterRule;
        }
    }



}

