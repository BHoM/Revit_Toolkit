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
using BH.oM.Revit.Enums;

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

            List<BH.oM.Revit.Enums.Category> values = Enum.GetValues(typeof(BH.oM.Revit.Enums.Category)).Cast<BH.oM.Revit.Enums.Category>().ToList();

            viewFilter.Categories = revitViewFilter.GetCategories().Select(catId => categories.Where(cat => cat.Id == catId).First())
                                                   .Select(cat => Enum.GetValues(typeof(BH.oM.Revit.Enums.Category))
                                                                    .Cast<BH.oM.Revit.Enums.Category>()
                                                                    .ToList()
                                                                    .Where(enumValue => ((int)enumValue).ToString()==cat.Id.ToString())
                                                                    .First())
                                                   .ToList();

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
            List<oM.Revit.FilterRules.FilterRule> bhomFilterRules = revitFilterRules.Select(revitRule => FilterRuleFromRevit(revitViewFilter, revitRule as dynamic))
                                                                                    .Cast< oM.Revit.FilterRules.FilterRule >().ToList();
            return bhomFilterRules;
        }




        private static oM.Revit.FilterRules.FilterStringRule FilterRuleFromRevit(this ParameterFilterElement revitViewFilter,
            Autodesk.Revit.DB.FilterStringRule revitRule)
        {
            // 1. EXTRACT DATA from the REVIT FILTERRULE object

            // Downcast Revit FilterRule obj to Revit FilterStringRule obj
            Autodesk.Revit.DB.FilterStringRule revitFilterStringRule = (Autodesk.Revit.DB.FilterStringRule)revitRule;
            // Extract name and value assigned to the parameter of the FilterStringRule obj
            string paramName = GetParameterById(revitViewFilter.Document, revitRule.GetRuleParameter()).Definition.Name;
            string paramValue = revitRule.RuleString;
            // Get the RuleEvaluator of the FilterStringRule (Class defining the way the string value 
            // assigned to the parameter is compared with the one assigned by the filter)
            FilterStringRuleEvaluator stringEvaluator = revitRule.GetEvaluator();

            // Convert the REVIT FilterStringEvaluator type into the BHOM TextComparisonType Enum 
            TextComparisonType bhomTextEvaluator = TextComparisonTypeFromRevit(stringEvaluator.GetType().Name);


            // 2. BUILD the BHOM FILTERRULE object

            oM.Revit.FilterRules.FilterStringRule bhomFilterStringRule;
            bhomFilterStringRule = new oM.Revit.FilterRules.FilterStringRule();
            bhomFilterStringRule.ParameterName = paramName;
            bhomFilterStringRule.Value = paramValue;
            bhomFilterStringRule.ComparisonType = bhomTextEvaluator;

            return bhomFilterStringRule;
        }


        private static oM.Revit.FilterRules.FilterDoubleRule FilterRuleFromRevit(this ParameterFilterElement revitViewFilter,
                    Autodesk.Revit.DB.FilterDoubleRule revitRule)
        {
            // 1. EXTRACT DATA from the REVIT FILTERRULE object

            // Downcast Revit FilterRule obj to Revit FilterDoubleRule obj
            Autodesk.Revit.DB.FilterDoubleRule revitFilterDoubleRule = (Autodesk.Revit.DB.FilterDoubleRule)revitRule;
            // Extract name and value assigned to the parameter of the FilterDoubleRule obj
            string paramName = GetParameterById(revitViewFilter.Document,revitRule.GetRuleParameter()).Definition.Name;
            ForgeTypeId paramTypeId = GetParameterById(revitViewFilter.Document, revitRule.GetRuleParameter()).GetUnitTypeId();
            string paramValue = UnitUtils.ConvertFromInternalUnits(revitRule.RuleValue,paramTypeId).ToString();
            // Get the RuleEvaluator of the FilterDoubleRule (Class defining the way the string value 
            // assigned to the parameter is compared with the one assigned by the filter)
            FilterNumericRuleEvaluator numericEvaluator = revitRule.GetEvaluator();

            // Convert the REVIT FilterNumericEvaluator type into the BHOM NumberComparisonType Enum 
            NumberComparisonType bhomNumericEvaluator = NumberComparisonTypeFromRevit(numericEvaluator.GetType().Name);


            // 2. BUILD the BHOM FILTERRULE object

            oM.Revit.FilterRules.FilterDoubleRule bhomFilterDoubleRule;
            bhomFilterDoubleRule = new BH.oM.Revit.FilterRules.FilterDoubleRule();
            bhomFilterDoubleRule.ParameterName = paramName;
            bhomFilterDoubleRule.Value = paramValue;
            bhomFilterDoubleRule.ComparisonType = bhomNumericEvaluator;

            return bhomFilterDoubleRule;
        }


        private static oM.Revit.FilterRules.FilterIntegerRule FilterRuleFromRevit(this ParameterFilterElement revitViewFilter,
                    Autodesk.Revit.DB.FilterIntegerRule revitRule)
        {                    
            // 1. EXTRACT DATA from the REVIT FILTERRULE object

            // Downcast Revit FilterRule obj to Revit FilterIntegerRule obj
            Autodesk.Revit.DB.FilterIntegerRule revitFilterIntegerRule = (Autodesk.Revit.DB.FilterIntegerRule)revitRule;
            // Extract name and value assigned to the parameter of the FilterIntegerRule obj
            string paramName = GetParameterById(revitViewFilter.Document, revitRule.GetRuleParameter()).Definition.Name;
            ForgeTypeId paramTypeId = GetParameterById(revitViewFilter.Document, revitRule.GetRuleParameter()).GetUnitTypeId();
            string paramValue = UnitUtils.ConvertFromInternalUnits(revitRule.RuleValue, paramTypeId).ToString();
            // Get the RuleEvaluator of the FilterIntegerRule (Class defining the way the string value 
            // assigned to the parameter is compared with the one assigned by the filter)
            FilterNumericRuleEvaluator numericEvaluator = revitRule.GetEvaluator();

            // Convert the REVIT FilterNumericEvaluator type into the BHOM NumberComparisonType Enum 
            NumberComparisonType bhomNumericEvaluator=NumberComparisonTypeFromRevit(numericEvaluator.GetType().Name);

            // 2. BUILD the BHOM FILTERRULE object

            oM.Revit.FilterRules.FilterIntegerRule bhomFilterIntegerRule;
            bhomFilterIntegerRule = new BH.oM.Revit.FilterRules.FilterIntegerRule();
                            bhomFilterIntegerRule.ParameterName = paramName;
                            bhomFilterIntegerRule.Value = paramValue;
                            bhomFilterIntegerRule.ComparisonType = bhomNumericEvaluator;

            return bhomFilterIntegerRule;
        }


        private static oM.Revit.FilterRules.FilterElementIdRule FilterRuleFromRevit(this ParameterFilterElement revitViewFilter,
                    Autodesk.Revit.DB.FilterElementIdRule revitRule)
        {
            // 1. EXTRACT DATA from the REVIT FILTERRULE object

            // Downcast Revit FilterRule obj to Revit FilterElementIdRule obj
            Autodesk.Revit.DB.FilterElementIdRule revitFilterElemIdRule = (Autodesk.Revit.DB.FilterElementIdRule)revitRule;
            // Extract name and value assigned to the parameter of the FilterElementIdRule obj
            string paramName = GetParameterById(revitViewFilter.Document, revitRule.GetRuleParameter()).Definition.Name;
            string paramValue = revitViewFilter.Document.GetElement(revitRule.RuleValue).Name.ToString();
            // Get the RuleEvaluator of the FilterElementIdRule (Class defining the way the string value 
            // assigned to the parameter is compared with the one assigned by the filter)
            FilterNumericRuleEvaluator numericEvaluator = revitRule.GetEvaluator();

            // Convert the REVIT FilterNumericEvaluator type into the BHOM NumberComparisonType Enum 
            NumberComparisonType bhomNumericEvaluator = NumberComparisonTypeFromRevit(numericEvaluator.GetType().Name);

            // 2. BUILD the BHOM FILTERRULE object

            oM.Revit.FilterRules.FilterElementIdRule bhomFilterElemIdRule;
            bhomFilterElemIdRule = new BH.oM.Revit.FilterRules.FilterElementIdRule();
            bhomFilterElemIdRule.ParameterName = paramName;
            bhomFilterElemIdRule.Value = paramValue;
            bhomFilterElemIdRule.ComparisonType = bhomNumericEvaluator;

            return bhomFilterElemIdRule;
        }


        private static oM.Revit.FilterRules.FilterCategoryRule FilterRuleFromRevit(this ParameterFilterElement revitViewFilter, Autodesk.Revit.DB.FilterCategoryRule revitRule)
        {
            // 1. EXTRACT DATA from the REVIT FILTERRULE object

            // Downcast Revit FilterRule obj to Revit FilterCategoryRule obj
            Autodesk.Revit.DB.FilterCategoryRule revitFilterCategoryRule = (Autodesk.Revit.DB.FilterCategoryRule)revitRule;
            // Extract name and value assigned to the parameter of the FilterElementIdRule obj
            List<string> categoryNames = revitRule.GetCategories().Select(elId => revitViewFilter.Document.GetElement(elId).Name).ToList();

            // 2. BUILD the BHOM FILTERRULE object

            oM.Revit.FilterRules.FilterCategoryRule bhomFilterCategoryRule;
            bhomFilterCategoryRule = new BH.oM.Revit.FilterRules.FilterCategoryRule();
            bhomFilterCategoryRule.CategoryNames = categoryNames;

            return bhomFilterCategoryRule;
        }


        private static oM.Revit.FilterRules.ParameterValuePresenceRule FilterRuleFromRevit(this ParameterFilterElement revitViewFilter, Autodesk.Revit.DB.ParameterValuePresenceRule revitRule)
        {
            // 1. EXTRACT DATA from the REVIT FILTERRULE object
            oM.Revit.FilterRules.ParameterValuePresenceRule bhomParamValuePresenceRule;
            bhomParamValuePresenceRule = new BH.oM.Revit.FilterRules.ParameterValuePresenceRule();

            // 2. BUILD the BHOM FILTERRULE object

            bhomParamValuePresenceRule.ParameterName = revitViewFilter.Document.GetElement((revitRule).Parameter).Name;
            bhomParamValuePresenceRule.IsPresent = (revitRule.GetType() == typeof(HasValueFilterRule)) ? true : false;

            return bhomParamValuePresenceRule;
        }


        private static oM.Revit.FilterRules.FilterRule FilterRuleFromRevit(this ParameterFilterElement revitViewFilter, Autodesk.Revit.DB.FilterInverseRule revitRule)
        {
            // 1. EXTRACT DATA from the REVIT FILTERRULE object

            // Downcast Revit FilterRule obj to Revit FilterInverseRule obj
            Autodesk.Revit.DB.FilterInverseRule revitFilterInverseRule = (Autodesk.Revit.DB.FilterInverseRule)revitRule;
            // Extract innerRule assigned to the Revit FilterInverseRule obj
            Autodesk.Revit.DB.FilterRule innerRule = revitRule.GetInnerRule();

            // Convert the REVIT InnerRule into the corresponding BHOM FilterRule obj 
            TextComparisonType bhomTextEvaluator = 0;
            NumberComparisonType bhomNumericEvaluator = 0;

            switch (innerRule.GetType().Name)
            {
                //FilterStringRule
                case nameof(Autodesk.Revit.DB.FilterStringRule):
                    {
                        bhomTextEvaluator = InverseTextComparisonTypeFromRevit(((Autodesk.Revit.DB.FilterStringRule)innerRule).GetEvaluator().GetType().Name);
                        oM.Revit.FilterRules.FilterStringRule bhomFilterStringRule = new BH.oM.Revit.FilterRules.FilterStringRule();
                        bhomFilterStringRule.ParameterName = GetParameterById(revitViewFilter.Document, innerRule.GetRuleParameter()).Definition.Name;
                        bhomFilterStringRule.Value = ((Autodesk.Revit.DB.FilterStringRule)innerRule).RuleString;
                        bhomFilterStringRule.ComparisonType = bhomTextEvaluator;
                        return (oM.Revit.FilterRules.FilterRule)bhomFilterStringRule;
                    }
                // FilterDoubleRule
                case nameof(Autodesk.Revit.DB.FilterDoubleRule):
                    {
                        bhomNumericEvaluator = InverseNumberComparisonTypeFromRevit(((Autodesk.Revit.DB.FilterNumericValueRule)innerRule).GetEvaluator().GetType().Name);
                        oM.Revit.FilterRules.FilterDoubleRule bhomFilterDoubleRule = new BH.oM.Revit.FilterRules.FilterDoubleRule();
                        bhomFilterDoubleRule.ParameterName = GetParameterById(revitViewFilter.Document, innerRule.GetRuleParameter()).Definition.Name;
                        bhomFilterDoubleRule.Value = ((Autodesk.Revit.DB.FilterDoubleRule)innerRule).RuleValue.ToString();
                        bhomFilterDoubleRule.ComparisonType = bhomNumericEvaluator;
                        return (oM.Revit.FilterRules.FilterRule)bhomFilterDoubleRule;
                    }
                // FilterIntegerRule
                case nameof(Autodesk.Revit.DB.FilterIntegerRule):
                    {
                        bhomNumericEvaluator = InverseNumberComparisonTypeFromRevit(((Autodesk.Revit.DB.FilterNumericValueRule)innerRule).GetEvaluator().GetType().Name);
                        oM.Revit.FilterRules.FilterIntegerRule bhomFilterIntegerRule = new BH.oM.Revit.FilterRules.FilterIntegerRule();
                        bhomFilterIntegerRule.ParameterName = GetParameterById(revitViewFilter.Document, innerRule.GetRuleParameter()).Definition.Name;
                        bhomFilterIntegerRule.Value = ((Autodesk.Revit.DB.FilterIntegerRule)innerRule).RuleValue.ToString();
                        bhomFilterIntegerRule.ComparisonType = bhomNumericEvaluator;
                        return (oM.Revit.FilterRules.FilterRule)bhomFilterIntegerRule;
                    }
                // FilterElementIdRule
                case nameof(Autodesk.Revit.DB.FilterElementIdRule):
                    {

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


        private static NumberComparisonType NumberComparisonTypeFromRevit(string innerRuleName)
        {
            NumberComparisonType? output = null;

            switch (innerRuleName)
            {
                case (nameof(Autodesk.Revit.DB.FilterNumericEquals)):
                    return NumberComparisonType.Equal;
                    break;
                case (nameof(Autodesk.Revit.DB.FilterNumericGreater)):
                    return NumberComparisonType.Greater;
                    break;
                case (nameof(Autodesk.Revit.DB.FilterNumericGreaterOrEqual)):
                    return NumberComparisonType.GreaterOrEqual;
                    break;
                case (nameof(Autodesk.Revit.DB.FilterNumericLess)):
                    return NumberComparisonType.Less;
                    break;
                case (nameof(Autodesk.Revit.DB.FilterNumericLessOrEqual)):
                    return NumberComparisonType.LessOrEqual;
                    break;
                default:
                    break;
            }

            return (NumberComparisonType)output;

        }


        private static NumberComparisonType InverseNumberComparisonTypeFromRevit(string innerRuleName) 
        {
            NumberComparisonType? output = null;

            switch (innerRuleName)
                {
                    case (nameof(Autodesk.Revit.DB.FilterNumericEquals)):
                        return NumberComparisonType.NotEqual;
                        break;
                    case (nameof(Autodesk.Revit.DB.FilterNumericGreater)):
                        return NumberComparisonType.Less;
                        break;
                    case (nameof(Autodesk.Revit.DB.FilterNumericGreaterOrEqual)):
                        return NumberComparisonType.NotEqual;
                        break;
                    case (nameof(Autodesk.Revit.DB.FilterNumericLess)):
                        return NumberComparisonType.Greater;
                        break;
                    case (nameof(Autodesk.Revit.DB.FilterNumericLessOrEqual)):
                        return NumberComparisonType.GreaterOrEqual;
                        break;
                    default:
                        break;
                }

            return (NumberComparisonType)output;

        }



        private static TextComparisonType TextComparisonTypeFromRevit(string innerRuleName)
        {
            TextComparisonType? output = null;

            switch (innerRuleName)
            {
                case (nameof(Autodesk.Revit.DB.FilterStringEquals)):
                    return TextComparisonType.Equal;
                    break;
                case (nameof(Autodesk.Revit.DB.FilterStringBeginsWith)):
                    return TextComparisonType.StartsWith;
                    break;
                case (nameof(Autodesk.Revit.DB.FilterStringEndsWith)):
                    return TextComparisonType.EndsWith;
                    break;
                case (nameof(Autodesk.Revit.DB.FilterStringContains)):
                    return TextComparisonType.Contains;
                    break;
                case nameof(Autodesk.Revit.DB.FilterStringGreater):
                    return TextComparisonType.Greater;
                    break;
                case nameof(Autodesk.Revit.DB.FilterStringGreaterOrEqual):
                    return TextComparisonType.GreaterOrEqual;
                    break;
                case nameof(Autodesk.Revit.DB.FilterStringLess):
                    return TextComparisonType.Less;
                    break;
                case nameof(Autodesk.Revit.DB.FilterStringLessOrEqual):
                    return TextComparisonType.LessOrEqual;
                    break;
                default:
                    break;
            }

            return (TextComparisonType)output;

        }


        private static TextComparisonType InverseTextComparisonTypeFromRevit(string innerRuleName) 
        {
            TextComparisonType? output = null;

            switch (innerRuleName)
                {
                    case (nameof(Autodesk.Revit.DB.FilterStringEquals)):
                        return  TextComparisonType.NotEqual;
                        break;
                    case (nameof(Autodesk.Revit.DB.FilterStringBeginsWith)):
                        return  TextComparisonType.NotStartsWith;
                        break;
                    case (nameof(Autodesk.Revit.DB.FilterStringEndsWith)):
                        return  TextComparisonType.NotEndsWith;
                        break;
                    case (nameof(Autodesk.Revit.DB.FilterStringContains)):
                        return  TextComparisonType.ContainsNot;
                        break;
                    default:
                        break;
                }

            return (TextComparisonType)output;

        }


    }
}





