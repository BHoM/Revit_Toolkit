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
using BH.Engine.Base;
using BH.Engine.Data;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.MEP.Equipment.Parts;
using BH.oM.Physical.Elements;
using BH.oM.Revit.Enums;
using BH.oM.Revit.FilterRules;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Document = Autodesk.Revit.DB.Document;
using FilterRule = BH.oM.Revit.FilterRules.FilterRule;
using FilterStringRule = BH.oM.Revit.FilterRules.FilterStringRule;

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
                            .GroupBy(rule => rule.GetType())
                            .ToDictionary(grp => grp.Key, grp => grp.ToList())
                            .ToList()
                            .Select(kvp =>{

                                List<Autodesk.Revit.DB.FilterRule> filterRules = new List<Autodesk.Revit.DB.FilterRule>();

                                if (kvp.Key.IsSubclassOf(typeof(oM.Revit.FilterRules.FilterCategoryRule))){
                                    filterRules = kvp.Value.Cast<oM.Revit.FilterRules.FilterCategoryRule>()
                                                        .Select(filterCategoryRule => filterCategoryRuleToRevit(document, filterCategoryRule))
                                                        .ToList();}
                                else if (kvp.Key.IsSubclassOf(typeof(oM.Revit.FilterRules.FilterValueRule))){
                                    filterRules = kvp.Value.Cast<oM.Revit.FilterRules.FilterValueRule>()
                                                         .Select(filterValueRule => filterValueRuleToRevit(document, filterValueRule))
                                                         .ToList();}
                                else if (kvp.Key.IsSubclassOf(typeof(FilterMaterialRule))){
                                    filterRules = kvp.Value.Cast<FilterMaterialRule>()
                                                         .Select(filterMaterialRule => filterMaterialRuleToRevit(document, filterMaterialRule))
                                                         .ToList();}
                                return filterRules; })
                            .Select(filterRulesList => new ElementParameterFilter(filterRulesList))
                            .Cast<ElementFilter>()
                            .ToList()));


            revitFilter.CopyParameters(filter, settings);
            refObjects.AddOrReplace(filter, revitFilter);
            return revitFilter;
        }


        /***************************************************/


        public static Autodesk.Revit.DB.FilterRule filterRuleToRevit(Document document, oM.Revit.FilterRules.FilterRule filterRule)
        {

            /* 1. CONVERT BHOM FILTERRULE INTO REVIT FILTERRULE */

            // FilterCategoryRule
            if (filterRule.GetType() == typeof(oM.Revit.FilterRules.FilterCategoryRule))
                { return filterCategoryRuleToRevit(document, (oM.Revit.FilterRules.FilterCategoryRule)filterRule); }
            // FilterMaterialRule
            else if (filterRule.GetType() == typeof(oM.Revit.FilterRules.FilterMaterialRule))
                { return filterMaterialRuleToRevit(document, (oM.Revit.FilterRules.FilterMaterialRule)filterRule); }
            // FilterLevelRule
            else if (filterRule.GetType() == typeof(oM.Revit.FilterRules.FilterLevelRule))
                { return filterLevelRuleToRevit(document, (oM.Revit.FilterRules.FilterLevelRule)filterRule); }
            // FilterValueRule
            else if (filterRule.GetType().IsSubclassOf(typeof(oM.Revit.FilterRules.FilterValueRule)))
                { return filterValueRuleToRevit(document, (oM.Revit.FilterRules.FilterValueRule)filterRule); }

            return null;

        }


        private static Autodesk.Revit.DB.FilterRule filterCategoryRuleToRevit(Document document, oM.Revit.FilterRules.FilterCategoryRule filterCategoryRule)
        {
            /* 1. INITIALIZE FILTERRULE */
            Autodesk.Revit.DB.FilterRule revitFilterRule = null;

            /* 2. GET THE ELEMENT IDS OF THE CATEGORIES STORED IN THE FILTERCATEGORYRULE */
            List<ElementId> categoryIds = new FilteredElementCollector(document)
                    // Get all the Categories (INTERMEDIATE OPERATION)
                    .OfClass(typeof(Autodesk.Revit.DB.Category))
                    // Retain only the Categories having name appearing in the filter's list (INTERMEDIATE OPERATION)
                    .Where(elem => filterCategoryRule.CategoryNames.Contains(elem.Name))
                    // Cast down to Category Class Instances (INTERMEDIATE OPERATION)
                    .Cast<Autodesk.Revit.DB.Category>()
                    // Get the ids of the retain categories (INTERMEDIATE OPERATION)
                    .Select(cat => cat.Id)
                    // Turn the Stream into a List (TERMINAL OPERATION)
                    .ToList();

            /* 3. CREATE THE FILTER RULE */
            revitFilterRule = new Autodesk.Revit.DB.FilterCategoryRule(categoryIds);

            return revitFilterRule;
        }

        /***************************************************/

        private static Autodesk.Revit.DB.FilterRule filterMaterialRuleToRevit(Document document, FilterMaterialRule filterMaterialRule)
        {
            /* 1. INITIALIZE FILTERRULE AND BUILTINPARAMETER INSTANCES */
            Autodesk.Revit.DB.FilterRule revitFilterRule = null;
            BuiltInParameter parameter = BuiltInParameter.MATERIAL_NAME;

            /* 2. CREATE THE FILTER RULE */
            //ParameterValueProvider provider = new ParameterValueProvider(new ElementId(parameter));
            revitFilterRule = ParameterFilterRuleFactory.CreateEqualsRule(new ElementId(parameter), filterMaterialRule.MaterialName, true);

            return revitFilterRule;
        }

        /***************************************************/

        private static Autodesk.Revit.DB.FilterRule filterLevelRuleToRevit(Document document, FilterLevelRule filterLevelRule)
        {
            /* 1. INITIALIZE FILTERRULE AND BUILTINPARAMETER INSTANCES */
            Autodesk.Revit.DB.FilterRule revitFilterRule = null;
            BuiltInParameter elevationParam = BuiltInParameter.LEVEL_ELEV;
            ElementId elevParamId=new ElementId(elevationParam);
            Double levelElevation;

            /* 2. GET ELEVATION OF LEVEL CORRESPONDING TO INPUT LEVEL NAME */
            // Via Streams and withing a Try-Catch statement to make sure the code is compact and if the level is not found, we prevent any error
            // being thrown when executing .First() while returning null instead.
            try {
                levelElevation=new FilteredElementCollector(document)
                                     .OfCategory(BuiltInCategory.OST_Levels)
                                     .Where(level => level.Name.ToUpper() == filterLevelRule.LevelName.ToUpper())
                                     .Cast<Level>()
                                     .Select(level => level.Elevation)
                                     .First();
            } catch (Exception ex){
                return null;}


            /* 3. CREATE FILTERS RULE */

            // Based on level's elevation and LevelComparisonType...
            switch (filterLevelRule.Evaluator)
            {
                case LevelComparisonType.Equal:
                    revitFilterRule = ParameterFilterRuleFactory
                        .CreateEqualsRule(elevParamId,(double)levelElevation, 0.001);
                    break;
                case LevelComparisonType.NotEqual:
                    revitFilterRule = ParameterFilterRuleFactory
                        .CreateNotEqualsRule(elevParamId, (double)levelElevation, 0.001);
                    break;
                case LevelComparisonType.Above:
                    revitFilterRule = ParameterFilterRuleFactory
                        .CreateGreaterRule(elevParamId, (double)levelElevation, 0.001);
                    break;
                case LevelComparisonType.AtOrAbove:
                    revitFilterRule = ParameterFilterRuleFactory
                        .CreateGreaterOrEqualRule(elevParamId, (double)levelElevation, 0.001);
                    break;
                case LevelComparisonType.Below:
                    revitFilterRule = ParameterFilterRuleFactory
                        .CreateLessRule(elevParamId, (double)levelElevation, 0.001);
                    break;
                case LevelComparisonType.AtOrBelow:
                    revitFilterRule = ParameterFilterRuleFactory
                        .CreateLessOrEqualRule(elevParamId, (double)levelElevation, 0.001);
                    break; 
                default:
                    break;
            }

            return revitFilterRule;
        }


        /***************************************************/


        private static Autodesk.Revit.DB.FilterRule filterValueRuleToRevit(Document document,oM.Revit.FilterRules.FilterValueRule filterValueRule) {

            /* 1. INITIALIZE FILTERRULE AND LOGICALFILTER CLASS INSTANCES */
            Autodesk.Revit.DB.FilterRule revitFilterRule = null;
            ElementClassFilter paramFilter = new ElementClassFilter(typeof(Parameter));
            ElementClassFilter builtInParamFilter = new ElementClassFilter(typeof(BuiltInParameter));
            LogicalOrFilter logicalOrFilter = new LogicalOrFilter(paramFilter, builtInParamFilter);

            /* 2. GET THE ELEMENT ID OF THE PARAMETER OBJECT */
            ElementId parameterId = new FilteredElementCollector(document)
                            .WherePasses(logicalOrFilter)
                            .Where(par => par.Name.Equals(filterValueRule.ParameterName))
                            .First()
                            .Id;

            /* 3. CREATE FILTER-RULE */

            // Based on FilterStringRule...
            if (filterValueRule.GetType().IsSubclassOf(typeof(FilterStringRule)))
            {
                FilterStringRule filterStringValueRule = (FilterStringRule)filterValueRule;

                switch (filterStringValueRule.Evaluator)
                {
                    case TextComparisonType.Equal:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateEqualsRule(parameterId, (string)filterStringValueRule.Value, false);
                        break;
                    case TextComparisonType.NotEqual:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateNotEqualsRule(parameterId, (string)filterStringValueRule.Value, false);
                        break;
                    case TextComparisonType.Contains:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateContainsRule(parameterId, (string)filterStringValueRule.Value, false);
                        break;
                    case TextComparisonType.ContainsNot:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateNotContainsRule(parameterId, (string)filterStringValueRule.Value, false);
                        break;
                    case TextComparisonType.StartsWith:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateBeginsWithRule(parameterId, (string)filterStringValueRule.Value, false);
                        break;
                    case TextComparisonType.NotStartsWith:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateNotBeginsWithRule(parameterId, (string)filterStringValueRule.Value, false);
                        break;
                    case TextComparisonType.EndsWith:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateEndsWithRule(parameterId, (string)filterStringValueRule.Value, false);
                        break;
                    case TextComparisonType.NotEndsWith:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateNotEndsWithRule(parameterId, (string)filterStringValueRule.Value, false);
                        break;
                    case TextComparisonType.Greater:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateGreaterRule(parameterId, (string)filterStringValueRule.Value, false);
                        break;
                    case TextComparisonType.GreaterOrEqual:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateGreaterOrEqualRule(parameterId, (string)filterStringValueRule.Value, false);
                        break;
                    case TextComparisonType.Less:
                        revitFilterRule=ParameterFilterRuleFactory
                            .CreateLessRule(parameterId,(string)filterStringValueRule.Value, false);
                        break;
                    case TextComparisonType.LessOrEqual:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateLessOrEqualRule(parameterId, (string)filterStringValueRule.Value, false);
                        break;
                    default:
                        break;
                }

            // Based on FilterNumericValueRule...
            } else if (filterValueRule.GetType().IsSubclassOf(typeof(oM.Revit.FilterRules.FilterNumericValueRule)))
            {
                oM.Revit.FilterRules.FilterNumericValueRule filterNumericValueRule = (oM.Revit.FilterRules.FilterNumericValueRule)filterValueRule;

                switch (filterNumericValueRule.Evaluator)
                {
                    case NumberComparisonType.Equal:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateEqualsRule(parameterId,(string)filterNumericValueRule.Value, false);
                        break;
                    case NumberComparisonType.NotEqual:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateNotEqualsRule(parameterId, (string)filterNumericValueRule.Value, false);
                        break;
                    case NumberComparisonType.Greater:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateGreaterRule(parameterId, (string)filterNumericValueRule.Value, false);
                        break;
                    case NumberComparisonType.GreaterOrEqual:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateGreaterOrEqualRule(parameterId, (string)filterNumericValueRule.Value, false);
                        break;
                    case NumberComparisonType.Less:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateLessRule(parameterId, (string)filterNumericValueRule.Value, false);
                        break;
                    case NumberComparisonType.LessOrEqual:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateLessOrEqualRule(parameterId, (string)filterNumericValueRule.Value, false);
                        break;
                    default:
                        break;
                }
            }

            // Based on ParameterValuePresenceRule...
            else if (filterValueRule.GetType().IsSubclassOf(typeof(oM.Revit.FilterRules.ParameterValuePresenceRule)))
            {
                oM.Revit.FilterRules.ParameterValuePresenceRule parameterValuePresenceRule = (oM.Revit.FilterRules.ParameterValuePresenceRule)filterValueRule;

                if (parameterValuePresenceRule.IsPresent) {
                    revitFilterRule = ParameterFilterRuleFactory.CreateHasValueParameterRule(parameterId);}
                else {
                    revitFilterRule = ParameterFilterRuleFactory.CreateHasNoValueParameterRule(parameterId);}


            } else { return null; }


            return revitFilterRule;
        }
    }



}

