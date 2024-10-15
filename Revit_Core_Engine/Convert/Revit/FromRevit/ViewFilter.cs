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

using Autodesk.Revit.DB;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Revit.Views;
using BH.oM.Adapters.Revit.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.CodeDom;
using System;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.Creation;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit ParameterFilterElement to BH.oM.Adapters.Revit.Elements.ViewFilter.")]
        [Input("revitViewFilter", "Revit ParameterFilterElement to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("viewFilter", "BH.oM.Adapters.Revit.Elements.ViewFilter resulting from converting the input Revit ParameterFilterElement.")]
        public static oM.Adapters.Revit.Elements.ViewFilter ViewFilterFromRevit(this ParameterFilterElement revitViewFilter, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            oM.Adapters.Revit.Elements.ViewFilter viewFilter = refObjects.GetValue<oM.Adapters.Revit.Elements.ViewFilter>(revitViewFilter.Id.IntegerValue);
            if (viewFilter != null)
                return viewFilter;

            /* 1. Transfer Filter NAME */
            viewFilter = new oM.Adapters.Revit.Elements.ViewFilter { Name = revitViewFilter.Name };

            /* 2. Transfer List of CATEGORY NAMES */
            List<Autodesk.Revit.DB.Category> categories = new List<Autodesk.Revit.DB.Category>();
            foreach (Autodesk.Revit.DB.Category cat in revitViewFilter.Document.Settings.Categories) { categories.Add(cat); }
            viewFilter.Categories = revitViewFilter.GetCategories().Select(catId => categories.Where(cat=>cat.Id==catId).First()).Cast<BH.oM.Revit.Enums.Category>().ToList();

            /* 3. Transfer List of FILTER RULES */
            // If the Filter is assigned with any rules.....
            if (revitViewFilter.GetElementFilter()!=null)
            {
                //Extract the name of all the parameters affected by the rules of the Revit View Filter object (ElementParameterFilter) - via STREAMS
                List<FilterRule> filterRules = ((ElementLogicalFilter)revitViewFilter.GetElementFilter()).GetFilters()
                        .Select(filter => ((ElementParameterFilter)filter).GetRules())
                        .SelectMany(list => list)
                        .ToList();
                //Extract the Revit Filter Rule objects defined in the Revit View Filter object (ElementParameterFilter)
                List<Autodesk.Revit.DB.FilterRule> revitFilterRules = (filterRules);
                //Convert the Revit Filter Rule objects into corresponding BHoM FilterRule objects and assign them to the BHoM ViewFilter objects
                viewFilter.Rules = FilterRulesFromRevit(revitViewFilter, revitFilterRules);
            }

            //Set identifiers, parameters & custom data
            viewFilter.SetIdentifiers(revitViewFilter);
            viewFilter.CopyParameters(revitViewFilter, settings.MappingSettings);
            viewFilter.SetProperties(revitViewFilter, settings.MappingSettings);
            refObjects.AddOrReplace(revitViewFilter.Id, viewFilter);
            return viewFilter;
        }

        /***************************************************/

        //Convert the Revit Filter Rule objects into corresponding BHoM FilterRule objects and store them in a List - via STREAMS
        private static List<oM.Revit.FilterRules.FilterRule> FilterRulesFromRevit(this ParameterFilterElement revitViewFilter, List<Autodesk.Revit.DB.FilterRule> revitFilterRules)
        {
            List<oM.Revit.FilterRules.FilterRule> bhomFilterRules = revitFilterRules.Select(revitRule =>
            {
                TextComparisonType bhomTextEvaluator = 0;
                NumberComparisonType bhomNumericEvaluator = 0;

                // FILTER STRING RULE
                if (revitRule.GetType()== typeof(Autodesk.Revit.DB.FilterStringRule))
                { return (oM.Revit.FilterRules.FilterRule)FilterStringRuleFromRevit(revitViewFilter, revitRule, bhomTextEvaluator); }
                // FILTER DOUBLE RULE
                else if (revitRule.GetType()==typeof(Autodesk.Revit.DB.FilterDoubleRule))
                { return (oM.Revit.FilterRules.FilterRule)FilterDoubleRuleFromRevit(revitViewFilter, revitRule, bhomNumericEvaluator); }
                // FILTER INTEGER RULE
                else if (revitRule.GetType() == typeof(Autodesk.Revit.DB.FilterIntegerRule))
                { return (oM.Revit.FilterRules.FilterRule)FilterIntegerRuleFromRevit(revitViewFilter, revitRule, bhomNumericEvaluator); }
                // FILTER ELEMENTID RULE
                else if (revitRule.GetType() == typeof(Autodesk.Revit.DB.FilterElementIdRule))
                { return (oM.Revit.FilterRules.FilterRule)FilterElementIdRuleFromRevit(revitViewFilter, revitRule, bhomNumericEvaluator); }
                // FILTER CATEGORY RULE
                else if (revitRule.GetType() == typeof(Autodesk.Revit.DB.FilterCategoryRule))
                { return (oM.Revit.FilterRules.FilterRule)FilterCategoryRuleFromRevit(revitViewFilter, revitRule); }
                // FILTER PARAMETER VALUE PRESENCE RULE
                else if (revitRule.GetType() == typeof(Autodesk.Revit.DB.ParameterValuePresenceRule))
                { return (oM.Revit.FilterRules.FilterRule)ParameterValuePresenceRuleFromRevit(revitViewFilter, revitRule); }
                // FILTER INVERSE RULE
                else if (revitRule.GetType() == typeof(Autodesk.Revit.DB.FilterInverseRule))
                { return FilterInverseRuleFromRevit(revitViewFilter, revitRule, bhomTextEvaluator, bhomNumericEvaluator); }
                return null;
            }).ToList();

            return bhomFilterRules;
        }


        private static oM.Revit.FilterRules.FilterStringRule FilterStringRuleFromRevit(this ParameterFilterElement revitViewFilter,
            Autodesk.Revit.DB.FilterRule revitRule, TextComparisonType bhomTextEvaluator)
        {
            // 1. EXTRACT DATA from the REVIT FILTERRULE object

            // Downcast Revit FilterRule obj to Revit FilterStringRule obj
            Autodesk.Revit.DB.FilterStringRule revitFilterStringRule = (Autodesk.Revit.DB.FilterStringRule)revitRule;
            // Extract name and value assigned to the parameter of the FilterStringRule obj
            string paramName = GetParameterById(revitViewFilter.Document, revitFilterStringRule.GetRuleParameter()).Definition.Name;
            string paramValue = revitFilterStringRule.RuleString;
            // Get the RuleEvaluator of the FilterStringRule (Class defining the way the string value 
            // assigned to the parameter is compared with the one assigned by the filter)
            FilterStringRuleEvaluator stringEvaluator = revitFilterStringRule.GetEvaluator();

            // Convert the REVIT FilterStringEvaluator type into the BHOM TextComparisonType Enum 
            switch (stringEvaluator.GetType().ToString())
            {
                case "Autodesk.Revit.DB.FilterStringBeginsWith":
                    bhomTextEvaluator = TextComparisonType.StartsWith;
                    break;
                case "Autodesk.Revit.DB.FilterStringEndsWith":
                    bhomTextEvaluator = TextComparisonType.EndsWith;
                    break;
                case "Autodesk.Revit.DB.FilterStringEquals":
                    bhomTextEvaluator = TextComparisonType.Equal;
                    break;
                case "Autodesk.Revit.DB.FilterStringContains":
                    bhomTextEvaluator = TextComparisonType.Contains;
                    break;
                case "Autodesk.Revit.DB.FilterStringGreater":
                    bhomTextEvaluator = TextComparisonType.Greater;
                    break;
                case "Autodesk.Revit.DB.FilterStringGreaterOrEqual":
                    bhomTextEvaluator = TextComparisonType.GreaterOrEqual;
                    break;
                case "Autodesk.Revit.DB.FilterStringLess":
                    bhomTextEvaluator = TextComparisonType.Less;
                    break;
                case "Autodesk.Revit.DB.FilterStringLessOrEqual":
                    bhomTextEvaluator = TextComparisonType.LessOrEqual;
                    break;
                default:
                    break;
            }

            // 2. BUILD the BHOM FILTERRULE object

            oM.Revit.FilterRules.FilterStringRule bhomFilterStringRule;
            bhomFilterStringRule = new oM.Revit.FilterRules.FilterStringRule();
            bhomFilterStringRule.ParameterName = paramName;
            bhomFilterStringRule.Value = paramValue;
            bhomFilterStringRule.ComparisonType = bhomTextEvaluator;

            return bhomFilterStringRule;
        }


        private static oM.Revit.FilterRules.FilterDoubleRule FilterDoubleRuleFromRevit(this ParameterFilterElement revitViewFilter,
                    Autodesk.Revit.DB.FilterRule revitRule, NumberComparisonType bhomNumericEvaluator)
        {
            // 1. EXTRACT DATA from the REVIT FILTERRULE object

            // Downcast Revit FilterRule obj to Revit FilterDoubleRule obj
            Autodesk.Revit.DB.FilterDoubleRule revitFilterDoubleRule = (Autodesk.Revit.DB.FilterDoubleRule)revitRule;
            // Extract name and value assigned to the parameter of the FilterDoubleRule obj
            string paramName = GetParameterById(revitViewFilter.Document,revitFilterDoubleRule.GetRuleParameter()).Definition.Name;
            ForgeTypeId paramTypeId = GetParameterById(revitViewFilter.Document, revitFilterDoubleRule.GetRuleParameter()).GetUnitTypeId();
            string paramValue = UnitUtils.ConvertFromInternalUnits(revitFilterDoubleRule.RuleValue,paramTypeId).ToString();
            // Get the RuleEvaluator of the FilterDoubleRule (Class defining the way the string value 
            // assigned to the parameter is compared with the one assigned by the filter)
            FilterNumericRuleEvaluator numericEvaluator = revitFilterDoubleRule.GetEvaluator();

            // Convert the REVIT FilterNumericEvaluator type into the BHOM NumberComparisonType Enum 
            switch (numericEvaluator.GetType().ToString())
            {
                case "Autodesk.Revit.DB.FilterNumericEquals":
                    bhomNumericEvaluator = NumberComparisonType.Equal;
                    break;
                case "Autodesk.Revit.DB.FilterNumericGreater":
                    bhomNumericEvaluator = NumberComparisonType.Greater;
                    break;
                case "Autodesk.Revit.DB.FilterNumericGreaterOrEqual":
                    bhomNumericEvaluator = NumberComparisonType.GreaterOrEqual;
                    break;
                case "Autodesk.Revit.DB.FilterNumericLess":
                    bhomNumericEvaluator = NumberComparisonType.Less;
                    break;
                case "Autodesk.Revit.DB.FilterNumericLessOrEqual":
                    bhomNumericEvaluator = NumberComparisonType.LessOrEqual;
                    break;
                default:
                    break;
            }

            // 2. BUILD the BHOM FILTERRULE object

            oM.Revit.FilterRules.FilterDoubleRule bhomFilterDoubleRule;
            bhomFilterDoubleRule = new BH.oM.Revit.FilterRules.FilterDoubleRule();
            bhomFilterDoubleRule.ParameterName = paramName;
            bhomFilterDoubleRule.Value = paramValue;
            bhomFilterDoubleRule.ComparisonType = bhomNumericEvaluator;

            return bhomFilterDoubleRule;
        }


        private static oM.Revit.FilterRules.FilterIntegerRule FilterIntegerRuleFromRevit(this ParameterFilterElement revitViewFilter,
                    Autodesk.Revit.DB.FilterRule revitRule, NumberComparisonType bhomNumericEvaluator)
        {                    
            // 1. EXTRACT DATA from the REVIT FILTERRULE object

            // Downcast Revit FilterRule obj to Revit FilterIntegerRule obj
            Autodesk.Revit.DB.FilterIntegerRule revitFilterIntegerRule = (Autodesk.Revit.DB.FilterIntegerRule)revitRule;
            // Extract name and value assigned to the parameter of the FilterIntegerRule obj
            string paramName = GetParameterById(revitViewFilter.Document, revitFilterIntegerRule.GetRuleParameter()).Definition.Name;
            ForgeTypeId paramTypeId = GetParameterById(revitViewFilter.Document, revitFilterIntegerRule.GetRuleParameter()).GetUnitTypeId();
            string paramValue = UnitUtils.ConvertFromInternalUnits(revitFilterIntegerRule.RuleValue, paramTypeId).ToString();
            // Get the RuleEvaluator of the FilterIntegerRule (Class defining the way the string value 
            // assigned to the parameter is compared with the one assigned by the filter)
            FilterNumericRuleEvaluator numericEvaluator = revitFilterIntegerRule.GetEvaluator();

            // Convert the REVIT FilterNumericEvaluator type into the BHOM NumberComparisonType Enum 
            switch (numericEvaluator.GetType().ToString())
            {
                case "Autodesk.Revit.DB.FilterNumericEquals":
                    bhomNumericEvaluator = NumberComparisonType.Equal;
                    break;
                case "Autodesk.Revit.DB.FilterNumericGreater":
                    bhomNumericEvaluator = NumberComparisonType.Greater;
                    break;
                case "Autodesk.Revit.DB.FilterNumericGreaterOrEqual":
                    bhomNumericEvaluator = NumberComparisonType.GreaterOrEqual;
                    break;
                case "Autodesk.Revit.DB.FilterNumericLess":
                    bhomNumericEvaluator = NumberComparisonType.Less;
                    break;
                case "Autodesk.Revit.DB.FilterNumericLessOrEqual":
                    bhomNumericEvaluator = NumberComparisonType.LessOrEqual;
                    break;
                default:
                    break;
            }

            // 2. BUILD the BHOM FILTERRULE object

            oM.Revit.FilterRules.FilterIntegerRule bhomFilterIntegerRule;
            bhomFilterIntegerRule = new BH.oM.Revit.FilterRules.FilterIntegerRule();
                            bhomFilterIntegerRule.ParameterName = paramName;
                            bhomFilterIntegerRule.Value = paramValue;
                            bhomFilterIntegerRule.ComparisonType = bhomNumericEvaluator;

            return bhomFilterIntegerRule;
        }


        private static oM.Revit.FilterRules.FilterElementIdRule FilterElementIdRuleFromRevit(this ParameterFilterElement revitViewFilter,
                    Autodesk.Revit.DB.FilterRule revitRule, NumberComparisonType bhomNumericEvaluator)
        {
            // 1. EXTRACT DATA from the REVIT FILTERRULE object

            // Downcast Revit FilterRule obj to Revit FilterElementIdRule obj
            Autodesk.Revit.DB.FilterElementIdRule revitFilterElemIdRule = (Autodesk.Revit.DB.FilterElementIdRule)revitRule;
            // Extract name and value assigned to the parameter of the FilterElementIdRule obj
            string paramName = GetParameterById(revitViewFilter.Document, revitFilterElemIdRule.GetRuleParameter()).Definition.Name;
            string paramValue = revitViewFilter.Document.GetElement(revitFilterElemIdRule.RuleValue).Name.ToString();
            // Get the RuleEvaluator of the FilterElementIdRule (Class defining the way the string value 
            // assigned to the parameter is compared with the one assigned by the filter)
            FilterNumericRuleEvaluator numericEvaluator = revitFilterElemIdRule.GetEvaluator();

            // Convert the REVIT FilterNumericEvaluator type into the BHOM NumberComparisonType Enum 
            switch (numericEvaluator.GetType().ToString())
            {
                case "Autodesk.Revit.DB.FilterNumericEquals":
                    bhomNumericEvaluator = NumberComparisonType.Equal;
                    break;
                case "Autodesk.Revit.DB.FilterNumericGreater":
                    bhomNumericEvaluator = NumberComparisonType.Greater;
                    break;
                case "Autodesk.Revit.DB.FilterNumericGreaterOrEqual":
                    bhomNumericEvaluator = NumberComparisonType.GreaterOrEqual;
                    break;
                case "Autodesk.Revit.DB.FilterNumericLess":
                    bhomNumericEvaluator = NumberComparisonType.Less;
                    break;
                case "Autodesk.Revit.DB.FilterNumericLessOrEqual":
                    bhomNumericEvaluator = NumberComparisonType.LessOrEqual;
                    break;
                default:
                    break;
            }

            // 2. BUILD the BHOM FILTERRULE object

            oM.Revit.FilterRules.FilterElementIdRule bhomFilterElemIdRule;
            bhomFilterElemIdRule = new BH.oM.Revit.FilterRules.FilterElementIdRule();
            bhomFilterElemIdRule.ParameterName = paramName;
            bhomFilterElemIdRule.Value = paramValue;
            bhomFilterElemIdRule.ComparisonType = bhomNumericEvaluator;

            return bhomFilterElemIdRule;
        }


        private static oM.Revit.FilterRules.FilterCategoryRule FilterCategoryRuleFromRevit(this ParameterFilterElement revitViewFilter, Autodesk.Revit.DB.FilterRule revitRule)
        {
            // 1. EXTRACT DATA from the REVIT FILTERRULE object

            // Downcast Revit FilterRule obj to Revit FilterCategoryRule obj
            Autodesk.Revit.DB.FilterCategoryRule revitFilterCategoryRule = (Autodesk.Revit.DB.FilterCategoryRule)revitRule;
            // Extract name and value assigned to the parameter of the FilterElementIdRule obj
            List<string> categoryNames = revitFilterCategoryRule.GetCategories().Select(elId => revitViewFilter.Document.GetElement(elId).Name).ToList();

            // 2. BUILD the BHOM FILTERRULE object

            oM.Revit.FilterRules.FilterCategoryRule bhomFilterCategoryRule;
            bhomFilterCategoryRule = new BH.oM.Revit.FilterRules.FilterCategoryRule();
            bhomFilterCategoryRule.CategoryNames = categoryNames;

            return bhomFilterCategoryRule;
        }


        private static oM.Revit.FilterRules.ParameterValuePresenceRule ParameterValuePresenceRuleFromRevit(this ParameterFilterElement revitViewFilter, Autodesk.Revit.DB.FilterRule revitRule)
        {
            // 1. EXTRACT DATA from the REVIT FILTERRULE object
            oM.Revit.FilterRules.ParameterValuePresenceRule bhomParamValuePresenceRule;
            bhomParamValuePresenceRule = new BH.oM.Revit.FilterRules.ParameterValuePresenceRule();

            // 2. BUILD the BHOM FILTERRULE object

            bhomParamValuePresenceRule.ParameterName = revitViewFilter.Document.GetElement(((Autodesk.Revit.DB.ParameterValuePresenceRule)revitRule).Parameter).Name;
            bhomParamValuePresenceRule.IsPresent = (revitRule.GetType() == typeof(HasValueFilterRule)) ? true : false;

            return bhomParamValuePresenceRule;
        }


        private static oM.Revit.FilterRules.FilterRule FilterInverseRuleFromRevit(this ParameterFilterElement revitViewFilter, 
            Autodesk.Revit.DB.FilterRule revitRule, TextComparisonType bhomTextEvaluator, NumberComparisonType bhomNumericEvaluator)
        {
            // 1. EXTRACT DATA from the REVIT FILTERRULE object

            // Downcast Revit FilterRule obj to Revit FilterInverseRule obj
            Autodesk.Revit.DB.FilterInverseRule revitFilterInverseRule = (Autodesk.Revit.DB.FilterInverseRule)revitRule;
            // Extract innerRule assigned to the Revit FilterInverseRule obj
            Autodesk.Revit.DB.FilterRule innerRule = revitFilterInverseRule.GetInnerRule();

            // Convert the REVIT InnerRule into the corresponding BHOM FilterRule obj 
            switch (innerRule.GetType().ToString())
            {
                //FilterStringRule
                case ("Autodesk.Revit.DB.FilterStringRule"):
                    {
                        switch (((Autodesk.Revit.DB.FilterStringRule)innerRule).GetEvaluator().GetType().ToString())
                        {
                            case ("Autodesk.Revit.DB.FilterStringEquals"):
                                bhomTextEvaluator = TextComparisonType.NotEqual;
                                break;
                            case ("Autodesk.Revit.DB.FilterStringBeginsWith"):
                                bhomTextEvaluator = TextComparisonType.NotStartsWith;
                                break;
                            case ("Autodesk.Revit.DB.FilterStringEndsWith"):
                                bhomTextEvaluator = TextComparisonType.NotEndsWith;
                                break;
                            case ("Autodesk.Revit.DB.FilterStringContains"):
                                bhomTextEvaluator = TextComparisonType.ContainsNot;
                                break;
                            default:
                                break;
                        }
                        oM.Revit.FilterRules.FilterStringRule bhomFilterStringRule = new BH.oM.Revit.FilterRules.FilterStringRule();
                        bhomFilterStringRule.ParameterName = GetParameterById(revitViewFilter.Document, innerRule.GetRuleParameter()).Definition.Name;
                        bhomFilterStringRule.Value = ((Autodesk.Revit.DB.FilterStringRule)innerRule).RuleString;
                        bhomFilterStringRule.ComparisonType = bhomTextEvaluator;
                        return (oM.Revit.FilterRules.FilterRule)bhomFilterStringRule;
                    }
                // FilterDoubleRule
                case ("Autodesk.Revit.DB.FilterDoubleRule"):
                    {
                        switch (((Autodesk.Revit.DB.FilterNumericValueRule)innerRule).GetEvaluator().GetType().ToString())
                        {
                            case ("Autodesk.Revit.DB.FilterNumericEquals"):
                                bhomNumericEvaluator = NumberComparisonType.NotEqual;
                                break;
                            case ("Autodesk.Revit.DB.FilterNumericGreater"):
                                bhomNumericEvaluator = NumberComparisonType.Less;
                                break;
                            case ("Autodesk.Revit.DB.FilterNumericGreaterOrEqual"):
                                bhomNumericEvaluator = NumberComparisonType.NotEqual;
                                break;
                            case ("Autodesk.Revit.DB.FilterNumericLess"):
                                bhomNumericEvaluator = NumberComparisonType.Greater;
                                break;
                            case ("Autodesk.Revit.DB.FilterNumericLessOrEqual"):
                                bhomNumericEvaluator = NumberComparisonType.GreaterOrEqual;
                                break;
                        }
                        oM.Revit.FilterRules.FilterDoubleRule bhomFilterDoubleRule = new BH.oM.Revit.FilterRules.FilterDoubleRule();
                        bhomFilterDoubleRule.ParameterName = GetParameterById(revitViewFilter.Document, innerRule.GetRuleParameter()).Definition.Name;
                        bhomFilterDoubleRule.Value = ((Autodesk.Revit.DB.FilterDoubleRule)innerRule).RuleValue.ToString();
                        bhomFilterDoubleRule.ComparisonType = bhomNumericEvaluator;
                        return (oM.Revit.FilterRules.FilterRule)bhomFilterDoubleRule;
                    }
                // FilterIntegerRule
                case ("Autodesk.Revit.DB.FilterIntegerRule"):
                    {
                        switch (((Autodesk.Revit.DB.FilterNumericValueRule)innerRule).GetEvaluator().GetType().ToString())
                        {
                            case ("Autodesk.Revit.DB.FilterNumericEquals"):
                                bhomNumericEvaluator = NumberComparisonType.NotEqual;
                                break;
                            case ("Autodesk.Revit.DB.FilterNumericGreater"):
                                bhomNumericEvaluator = NumberComparisonType.Less;
                                break;
                            case ("Autodesk.Revit.DB.FilterNumericGreaterOrEqual"):
                                bhomNumericEvaluator = NumberComparisonType.NotEqual;
                                break;
                            case ("Autodesk.Revit.DB.FilterNumericLess"):
                                bhomNumericEvaluator = NumberComparisonType.Greater;
                                break;
                            case ("Autodesk.Revit.DB.FilterNumericLessOrEqual"):
                                bhomNumericEvaluator = NumberComparisonType.GreaterOrEqual;
                                break;
                        }
                        oM.Revit.FilterRules.FilterIntegerRule bhomFilterIntegerRule = new BH.oM.Revit.FilterRules.FilterIntegerRule();
                        bhomFilterIntegerRule.ParameterName = GetParameterById(revitViewFilter.Document, innerRule.GetRuleParameter()).Definition.Name;
                        bhomFilterIntegerRule.Value = ((Autodesk.Revit.DB.FilterIntegerRule)innerRule).RuleValue.ToString();
                        bhomFilterIntegerRule.ComparisonType = bhomNumericEvaluator;
                        return (oM.Revit.FilterRules.FilterRule)bhomFilterIntegerRule;
                    }
                // FilterElementIdRule
                case ("Autodesk.Revit.DB.FilterElementIdRule"):
                    {
                        switch (((Autodesk.Revit.DB.FilterElementIdRule)innerRule).GetEvaluator().GetType().ToString())
                        {
                            case ("Autodesk.Revit.DB.FilterNumericEquals"):
                                bhomNumericEvaluator = NumberComparisonType.NotEqual;
                                break;
                            case ("Autodesk.Revit.DB.FilterNumericGreater"):
                                bhomNumericEvaluator = NumberComparisonType.Less;
                                break;
                            case ("Autodesk.Revit.DB.FilterNumericGreaterOrEqual"):
                                bhomNumericEvaluator = NumberComparisonType.NotEqual;
                                break;
                            case ("Autodesk.Revit.DB.FilterNumericLess"):
                                bhomNumericEvaluator = NumberComparisonType.Greater;
                                break;
                            case ("Autodesk.Revit.DB.FilterNumericLessOrEqual"):
                                bhomNumericEvaluator = NumberComparisonType.GreaterOrEqual;
                                break;
                        }
                        oM.Revit.FilterRules.FilterElementIdRule bhomFilterElementIdRule = new BH.oM.Revit.FilterRules.FilterElementIdRule();
                        bhomFilterElementIdRule.ParameterName = GetParameterById(revitViewFilter.Document, innerRule.GetRuleParameter()).Definition.Name;
                        bhomFilterElementIdRule.Value = ((Autodesk.Revit.DB.FilterElementIdRule)innerRule).RuleValue.ToString();
                        bhomFilterElementIdRule.ComparisonType = bhomNumericEvaluator;
                        return (oM.Revit.FilterRules.FilterRule)bhomFilterElementIdRule;
                    }
                default:
                    { return null; }

            }
        }


        /* UTILITY METHODS */

        private static Parameter GetParameterById(Autodesk.Revit.DB.Document doc, ElementId parameterId)
        {
            // Get all elements in the document
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.WhereElementIsNotElementType();

                // Iterate through all elements
                foreach (Element element in collector)
                {
                    // Get the parameter by its own Id
                    List<Parameter> matchingParams=element.Parameters().Where(prm => prm.Id == parameterId).ToList();

                    if (matchingParams.Count!=0)
                        {return matchingParams.First();} 

                }

            // If the parameter is not found, return InvalidElementId
            return null;
        }

    }
}





