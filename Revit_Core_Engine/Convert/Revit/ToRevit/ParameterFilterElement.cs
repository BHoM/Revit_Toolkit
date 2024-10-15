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
                             .Select(catObj => catObj.ToString().ToUpper().Replace(" ", ""))
                             // Get the corresponding BuiltInCategories
                             .Select(catName => { List<string> builtInCatNames = Enum.GetNames(typeof(BuiltInCategory))
                                                        .Select(builtInCategoryName => builtInCategoryName.ToUpper().Replace("OST_", "").Replace("_", ""))
                                                        .ToList();
                                 return (BuiltInCategory)(((BuiltInCategory[])Enum.GetValues(typeof(BuiltInCategory)))[builtInCatNames.IndexOf(catName)]); })
                             // Get the ElementIds of the BuiltInCategories
                             .Select(builtInCat => new ElementId(builtInCat))
                             // Turn the Stream into a List of ElementIds
                             .ToList<ElementId>();

            /* 1.2 Create the ParameterFilterElement in the current Revit Document */
            revitFilter = ParameterFilterElement.Create(document, filter.Name, categoryIdsList);


            // 2. BUILD THE REVIT FILTER RULES and ASSIGN THEM TO THE PARAMETERFILTERELEMENT

            /* Via use of Streams*/
            if (filter.Rules.Count != 0)
            {
                ElementFilter elFilter = new LogicalAndFilter(filter.Rules
                                .GroupBy(rule => rule.GetType())
                                .ToDictionary(grp => grp.Key, grp => grp.ToList())
                                .ToList()
                                .Select(kvp =>
                                {

                                    List<Autodesk.Revit.DB.FilterRule> filterRules = new List<Autodesk.Revit.DB.FilterRule>();

                                    if (kvp.Key.Name == "FilterCategoryRule")
                                    {
                                        filterRules = kvp.Value.Cast<oM.Revit.FilterRules.FilterCategoryRule>()
                                                            .Select(filterCategoryRule => filterCategoryRuleToRevit(document, filterCategoryRule))
                                                            .ToList();
                                    }
                                    else if (kvp.Key.Name == "FilterLevelRule")
                                    {
                                        filterRules = kvp.Value.Cast<FilterLevelRule>()
                                                             .Select(filterLevelRule => filterLevelRuleToRevit(document, filterLevelRule))
                                                             .ToList();
                                    }
                                    else if (kvp.Key.Name == "FilterMaterialRule")
                                    {
                                        filterRules = kvp.Value.Cast<FilterMaterialRule>()
                                                             .Select(filterMaterialRule => filterMaterialRuleToRevit(document, filterMaterialRule))
                                                             .ToList();
                                    }
                                    else if (kvp.Key.Name == "FilterStringRule" || kvp.Key.Name == "FilterDoubleRule" ||
                                                kvp.Key.Name == "FilterIntegerRule" || kvp.Key.Name == "FilterElementIdRule")
                                    {
                                        filterRules = kvp.Value.Cast<oM.Revit.FilterRules.FilterValueRule>()
                                                             .Select(filterValueRule => filterValueRuleToRevit(document, filterValueRule))
                                                             .ToList();
                                    }
                                    return filterRules;
                                })
                                .Select(filterRulesList => new ElementParameterFilter(filterRulesList))
                                .Cast<ElementFilter>()
                                .ToList());

                revitFilter.SetElementFilter(elFilter);
            }

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

            /* 2. EXTRACT CATEGORIES */
            List<Autodesk.Revit.DB.Category> categories = new List<Autodesk.Revit.DB.Category>();
            foreach (Autodesk.Revit.DB.Category cat in document.Settings.Categories) { categories.Add(cat); }

            /* 2. GET THE ELEMENT IDS OF THE CATEGORIES STORED IN THE FILTERCATEGORYRULE */
            List<ElementId> categoryIds = categories
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
            BuiltInParameter parameter = BuiltInParameter.STRUCTURAL_MATERIAL_PARAM;

            /* 2. CREATE THE FILTER RULE */

            //ParameterValueProvider provider = new ParameterValueProvider(new ElementId(parameter));
            //revitFilterRule = ParameterFilterRuleFactory.CreateEqualsRule(new ElementId(parameter), filterMaterialRule.MaterialName, true);

            FilteredElementCollector collector = new FilteredElementCollector(document);
            Element mat = collector.OfClass(typeof(Material)).Where(material => material.Name == filterMaterialRule.MaterialName).First();
            revitFilterRule = ParameterFilterRuleFactory.CreateEqualsRule(new ElementId(parameter), mat.Id);

            return revitFilterRule;
        }

        /***************************************************/

        private static Autodesk.Revit.DB.FilterRule filterLevelRuleToRevit(Document document, FilterLevelRule filterLevelRule)
        {
            /* 1. INITIALIZE FILTERRULE AND BUILTINPARAMETER INSTANCES */
            Autodesk.Revit.DB.FilterRule revitFilterRule = null;
            BuiltInParameter levParam = BuiltInParameter.SCHEDULE_LEVEL_PARAM;
            ElementId levParamId = new ElementId(levParam);
            ElementId levelId;

            /* 2. GET ELEVATION OF LEVEL CORRESPONDING TO INPUT LEVEL NAME */
            // Via Streams and withing a Try-Catch statement to make sure the code is compact and if the level is not found, we prevent any error
            // being thrown when executing .First() while returning null instead.
            try
            {
                levelId = new FilteredElementCollector(document)
                                     .OfCategory(BuiltInCategory.OST_Levels)
                                     .Where(level => level.Name.ToUpper() == filterLevelRule.LevelName.ToUpper())
                                     .Cast<Level>()
                                     .Select(level => level.Id)
                                     .First();
            }
            catch (Exception ex)
            {
                return null;
            }



            /* 3. CREATE FILTERS RULE */

            // Based on level's elevation and LevelComparisonType...
            switch (filterLevelRule.ComparisonType)
            {
                case LevelComparisonType.Equal:
                    revitFilterRule = ParameterFilterRuleFactory
                        .CreateEqualsRule(levParamId, levelId);
                    break;
                case LevelComparisonType.NotEqual:
                    revitFilterRule = ParameterFilterRuleFactory
                        .CreateNotEqualsRule(levParamId, levelId);
                    break;
                case LevelComparisonType.Above:
                    revitFilterRule = ParameterFilterRuleFactory
                        .CreateGreaterRule(levParamId, levelId);
                    break;
                case LevelComparisonType.AtOrAbove:
                    revitFilterRule = ParameterFilterRuleFactory
                        .CreateGreaterOrEqualRule(levParamId, levelId);
                    break;
                case LevelComparisonType.Below:
                    revitFilterRule = ParameterFilterRuleFactory
                        .CreateLessRule(levParamId, levelId);
                    break;
                case LevelComparisonType.AtOrBelow:
                    revitFilterRule = ParameterFilterRuleFactory
                        .CreateLessOrEqualRule(levParamId, levelId);
                    break;
                default:
                    break;
            }

            return revitFilterRule;
        }


        /***************************************************/

        private static Autodesk.Revit.DB.FilterRule filterValueRuleToRevit(Document document, oM.Revit.FilterRules.FilterValueRule filterValueRule) {

        /* 1. INITIALIZE FILTERRULE AND LOGICALFILTER CLASS INSTANCES */
        Autodesk.Revit.DB.FilterRule revitFilterRule = null;

        /* 2. GET the PARAMETER OBJECT and the ELEMENT ID of the PARAMETER OBJECT */
        ElementId parameterId = GetParameterIdByName(document, filterValueRule.ParameterName);
        Parameter parameter = GetParameterByName(document, filterValueRule.ParameterName);

        /* 3. CREATE FILTER-RULE */

        // Based on FilterStringRule...
        if (filterValueRule.GetType() == typeof(FilterStringRule) ||
            filterValueRule.GetType().IsSubclassOf(typeof(FilterStringRule)))
        {
            FilterStringRule filterStringValueRule = (FilterStringRule)filterValueRule;

            switch (filterStringValueRule.ComparisonType)
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
                    revitFilterRule = ParameterFilterRuleFactory
                        .CreateLessRule(parameterId, (string)filterStringValueRule.Value, false);
                    break;
                case TextComparisonType.LessOrEqual:
                    revitFilterRule = ParameterFilterRuleFactory
                        .CreateLessOrEqualRule(parameterId, (string)filterStringValueRule.Value, false);
                    break;
                default:
                    break;
            }

            // Based on FilterNumericValueRule...
        } else if (filterValueRule.GetType() == typeof(oM.Revit.FilterRules.FilterNumericValueRule) ||
                   filterValueRule.GetType().IsSubclassOf(typeof(oM.Revit.FilterRules.FilterNumericValueRule)))
        {

            if ((filterValueRule.GetType() == typeof(oM.Revit.FilterRules.FilterDoubleRule)))
            {
                /* 1. Downcast to subclass */
                oM.Revit.FilterRules.FilterDoubleRule filterDoubleRule = (oM.Revit.FilterRules.FilterDoubleRule)filterValueRule;

                /* 2. Convert input value to target data type */
                Double doubleValue = 0.0;
                Boolean boolParam = Double.TryParse(filterDoubleRule.Value, out doubleValue);

                if (!boolParam)
                {
                    BH.Engine.Base.Compute.RecordError("The Input Value of the FilterDoubleRule is not a Double Type value.");
                }

                /* 3. Convert units of input value to internal units */
                double convertedValue = UnitUtils.ConvertToInternalUnits(doubleValue, parameter.GetUnitTypeId());

                /* 4. Convert Evaluator from Revit to BHoM */
                switch (filterDoubleRule.ComparisonType)
                {
                    case NumberComparisonType.Equal:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateEqualsRule(parameterId, convertedValue, 0.01);
                        break;
                    case NumberComparisonType.NotEqual:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateNotEqualsRule(parameterId, convertedValue, 0.01);
                        break;
                    case NumberComparisonType.Greater:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateGreaterRule(parameterId, convertedValue, 0.01);
                        break;
                    case NumberComparisonType.GreaterOrEqual:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateGreaterOrEqualRule(parameterId, convertedValue, 0.01);
                        break;
                    case NumberComparisonType.Less:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateLessRule(parameterId, convertedValue, 0.01);
                        break;
                    case NumberComparisonType.LessOrEqual:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateLessOrEqualRule(parameterId, convertedValue, 0.01);
                        break;
                    default:
                        break;
                }
            }
            else if (filterValueRule.GetType() == typeof(oM.Revit.FilterRules.FilterIntegerRule))
            {
                /* 1. Downcast to subclass */
                oM.Revit.FilterRules.FilterIntegerRule filterIntegerRule = (oM.Revit.FilterRules.FilterIntegerRule)filterValueRule;

                /* 2. Convert input value to target data type */
                int intValue = 0;
                Boolean boolParam = int.TryParse(filterIntegerRule.Value, out intValue);

                if (!boolParam)
                {
                    BH.Engine.Base.Compute.RecordError("The Input Value of the FilterIntegerRule is not an Integer Type value.");
                }

                /* 3. Convert units of input value to internal units */
                int convertedValue = (int)UnitUtils.ConvertToInternalUnits((Double)intValue, parameter.GetUnitTypeId());

                /* 4. Convert Evaluator from Revit to BHoM */
                switch (filterIntegerRule.ComparisonType)
                {
                    case NumberComparisonType.Equal:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateEqualsRule(parameterId, convertedValue);
                        break;
                    case NumberComparisonType.NotEqual:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateNotEqualsRule(parameterId, convertedValue);
                        break;
                    case NumberComparisonType.Greater:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateGreaterRule(parameterId, convertedValue);
                        break;
                    case NumberComparisonType.GreaterOrEqual:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateGreaterOrEqualRule(parameterId, convertedValue);
                        break;
                    case NumberComparisonType.Less:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateLessRule(parameterId, convertedValue);
                        break;
                    case NumberComparisonType.LessOrEqual:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateLessOrEqualRule(parameterId, convertedValue);
                        break;
                    default:
                        break;
                }
            }
            else if (filterValueRule.GetType() == typeof(oM.Revit.FilterRules.FilterElementIdRule))
            {
                /* 1. Downcast to subclass */
                oM.Revit.FilterRules.FilterElementIdRule filterElementIdRule = (oM.Revit.FilterRules.FilterElementIdRule)filterValueRule;

                /* 2. Convert input value to target data type */
                Boolean boolParam = int.TryParse(filterElementIdRule.Value, out int intValue);
                ElementId elId = new ElementId(intValue);

                if (!boolParam)
                {
                    BH.Engine.Base.Compute.RecordError("The Input Value of the FilterIntegerRule is not an Integer Type value.");
                }

                /* 3. Convert Evaluator from Revit to BHoM */
                switch (filterElementIdRule.ComparisonType)
                {
                    case NumberComparisonType.Equal:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateEqualsRule(parameterId, elId);
                        break;
                    case NumberComparisonType.NotEqual:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateNotEqualsRule(parameterId, elId);
                        break;
                    case NumberComparisonType.Greater:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateGreaterRule(parameterId, elId);
                        break;
                    case NumberComparisonType.GreaterOrEqual:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateGreaterOrEqualRule(parameterId, elId);
                        break;
                    case NumberComparisonType.Less:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateLessRule(parameterId, elId);
                        break;
                    case NumberComparisonType.LessOrEqual:
                        revitFilterRule = ParameterFilterRuleFactory
                            .CreateLessOrEqualRule(parameterId, elId);
                        break;
                    default:
                        break;
                }
            }

        } else { return null; }

        return revitFilterRule;
    }




        /***************************************************/

        private static Autodesk.Revit.DB.FilterRule parameterValuePresenceRuleToRevit(Document document, oM.Revit.FilterRules.ParameterValuePresenceRule paramValuePresenceRule)
        {
            /* 1. INITIALIZE FILTERRULE AND LOGICALFILTER CLASS INSTANCES */
            Autodesk.Revit.DB.FilterRule revitFilterRule = null;

            /* 2. GET the PARAMETER OBJECT and the ELEMENT ID of the PARAMETER OBJECT */
            ElementId parameterId = GetParameterIdByName(document, paramValuePresenceRule.ParameterName);
            Parameter parameter = GetParameterByName(document, paramValuePresenceRule.ParameterName);

            if (paramValuePresenceRule.IsPresent)
            {
                revitFilterRule = ParameterFilterRuleFactory.CreateHasValueParameterRule(parameterId);
            }
            else
            {
                revitFilterRule = ParameterFilterRuleFactory.CreateHasNoValueParameterRule(parameterId);
            }

            return revitFilterRule;

        }

        private static ElementId GetParameterIdByName(Document doc, string parameterName)
     {
        // Get all elements in the document
        FilteredElementCollector collector = new FilteredElementCollector(doc);
        collector.WhereElementIsNotElementType();

        // Iterate through all elements
        foreach (Element element in collector)
        {
            // Get the parameter by name
            Parameter param = element.LookupParameter(parameterName);
            if (param != null)
            {
                // Return the ElementId of the parameter
                return param.Id;
            }
        }

        // If the parameter is not found, return InvalidElementId
        return ElementId.InvalidElementId;
    }


    private static Parameter GetParameterByName(Document doc, string parameterName)
    {
        // Get all elements in the document
        FilteredElementCollector collector = new FilteredElementCollector(doc);
        collector.WhereElementIsNotElementType();

        // Iterate through all elements
        foreach (Element element in collector)
        {
            // Get the parameter by name
            Parameter param = element.LookupParameter(parameterName);
            if (param != null)
            {
                // Return the ElementId of the parameter
                return param;
            }
        }

        // If the parameter is not found, return InvalidElementId
        return null;
    }

    }

}

