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
using FilterRule = Autodesk.Revit.DB.FilterRule;
using System.CodeDom;
using System;
using Autodesk.Revit.DB.Architecture;

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
            viewFilter.Categories = revitViewFilter.GetCategories().Select(catId => revitViewFilter.Document.GetElement(catId).Name).ToList<string>();
            /* 3. Transfer List of FILTER RULES */
            //viewFilter.Rules
            List<string> parameterNames= ((ElementParameterFilter)revitViewFilter.GetElementFilter()).GetRules()
                        .Select(rule => revitViewFilter.Document.GetElement(rule.GetRuleParameter()).Name.ToString())
                        .ToList<string>();
            List<Autodesk.Revit.DB.FilterRule> revitFilterRules = ((ElementParameterFilter)revitViewFilter.GetElementFilter()).GetRules().ToList();
            List<BH.oM.Revit.Elements.FilterRule> bhomFilterRules = revitFilterRules.Select(revitRule =>
                                                                {
                                                                    // FILTER STRING RULE
                                                                    if (revitRule.GetType().IsSubclassOf(typeof(Autodesk.Revit.DB.FilterStringRule)))
                                                                    {
                                                                        Autodesk.Revit.DB.FilterStringRule revitFilterStringRule = (Autodesk.Revit.DB.FilterStringRule)revitRule;
                                                                        string paramName = revitViewFilter.Document.GetElement(revitFilterStringRule.GetRuleParameter()).Name;
                                                                        string paramValue = revitFilterStringRule.RuleString;
                                                                        FilterStringRuleEvaluator stringEvaluator = revitFilterStringRule.GetEvaluator();

                                                                        TextComparisonType bhomEvaluator;

                                                                        switch (stringEvaluator.GetType())
                                                                        {
                                                                            case typeof(Autodesk.Revit.DB.FilterStringBeginsWith):
                                                                                bhomEvaluator = TextComparisonType.StartsWith;
                                                                                break;
                                                                            case typeof(Autodesk.Revit.DB.FilterStringEndsWith):
                                                                                bhomEvaluator = TextComparisonType.EndsWith;
                                                                                break;
                                                                            case typeof(Autodesk.Revit.DB.FilterStringEquals):
                                                                                bhomEvaluator = TextComparisonType.Equal;
                                                                                break;
                                                                            case typeof(Autodesk.Revit.DB.FilterStringContains):
                                                                                bhomEvaluator = TextComparisonType.Contains;
                                                                                break;
                                                                            case typeof(Autodesk.Revit.DB.FilterStringGreater):
                                                                                bhomEvaluator = TextComparisonType.Greater;
                                                                                break;
                                                                            case typeof(Autodesk.Revit.DB.FilterStringGreaterOrEqual):
                                                                                bhomEvaluator = TextComparisonType.GreaterOrEqual;
                                                                                break;
                                                                            case typeof(Autodesk.Revit.DB.FilterStringLess):
                                                                                bhomEvaluator = TextComparisonType.Less;
                                                                                break;
                                                                            case typeof(Autodesk.Revit.DB.FilterStringLessOrEqual):
                                                                                bhomEvaluator = TextComparisonType.LessOrEqual;
                                                                                break;
                                                                            default:
                                                                                break;
                                                                        }

                                                                        BH.oM.Revit.Elements.FilterStringRule bhomFilterStringRule;
                                                                        bhomFilterStringRule = new oM.Revit.Elements.FilterStringRule();
                                                                        bhomFilterStringRule.ParameterName = paramName;
                                                                        bhomFilterStringRule.Value = paramValue;
                                                                        bhomFilterStringRule.Evaluator = bhomEvaluator;

                                                                        return bhomFilterStringRule;
                                                                    }

                                                                    // FILTER DOUBLE RULE
                                                                    else if (revitRule.GetType().IsSubclassOf(typeof(Autodesk.Revit.DB.FilterDoubleRule)))
                                                                    {
                                                                        Autodesk.Revit.DB.FilterDoubleRule revitFilterDoubleRule = (Autodesk.Revit.DB.FilterDoubleRule)revitRule;
                                                                        string paramName = revitViewFilter.Document.GetElement(revitFilterDoubleRule.GetRuleParameter()).Name;
                                                                        string paramValue = revitFilterDoubleRule.RuleValue.ToString();
                                                                        FilterNumericRuleEvaluator numericEvaluator = revitFilterDoubleRule.GetEvaluator();

                                                                        NumberComparisonType bhomEvaluator;

                                                                        switch (numericEvaluator.GetType())
                                                                        {
                                                                            case typeof(Autodesk.Revit.DB.FilterNumericEquals):
                                                                                bhomEvaluator = NumberComparisonType.Equal;
                                                                                break;
                                                                            case typeof(Autodesk.Revit.DB.FilterNumericGreater):
                                                                                bhomEvaluator = NumberComparisonType.Greater;
                                                                                break;
                                                                            case typeof(Autodesk.Revit.DB.FilterNumericGreaterOrEqual):
                                                                                bhomEvaluator = NumberComparisonType.GreaterOrEqual;
                                                                                break;
                                                                            case typeof(Autodesk.Revit.DB.FilterNumericLess):
                                                                                bhomEvaluator = NumberComparisonType.Less;
                                                                                break;
                                                                            case typeof(Autodesk.Revit.DB.FilterNumericLessOrEqual):
                                                                                bhomEvaluator = NumberComparisonType.LessOrEqual;
                                                                                break;
                                                                            default:
                                                                                break;
                                                                        }

                                                                        BH.oM.Revit.Elements.FilterDoubleRule bhomFilterDoubleRule;
                                                                        bhomFilterDoubleRule = new oM.Revit.Elements.FilterDoubleRule();
                                                                        bhomFilterDoubleRule.ParameterName = paramName;
                                                                        bhomFilterDoubleRule.Value = paramValue;
                                                                        bhomFilterDoubleRule.Evaluator = bhomEvaluator;

                                                                        return bhomFilterDoubleRule;
                                                                    }

                                                                    // FILTER INTEGER RULE
                                                                    else if (revitRule.GetType().IsSubclassOf(typeof(Autodesk.Revit.DB.FilterIntegerRule)))
                                                                    {
                                                                        Autodesk.Revit.DB.FilterIntegerRule revitFilterIntegerRule = (Autodesk.Revit.DB.FilterIntegerRule)revitRule;
                                                                        string paramName = revitViewFilter.Document.GetElement(revitFilterIntegerRule.GetRuleParameter()).Name;
                                                                        string paramValue = revitFilterIntegerRule.RuleValue.ToString();
                                                                        FilterNumericRuleEvaluator numericEvaluator = revitFilterIntegerRule.GetEvaluator();

                                                                        NumberComparisonType bhomEvaluator;

                                                                        switch (numericEvaluator.GetType())
                                                                        {
                                                                            case typeof(Autodesk.Revit.DB.FilterNumericEquals):
                                                                                bhomEvaluator = NumberComparisonType.Equal;
                                                                                break;
                                                                            case typeof(Autodesk.Revit.DB.FilterNumericGreater):
                                                                                bhomEvaluator = NumberComparisonType.Greater;
                                                                                break;
                                                                            case typeof(Autodesk.Revit.DB.FilterNumericGreaterOrEqual):
                                                                                bhomEvaluator = NumberComparisonType.GreaterOrEqual;
                                                                                break;
                                                                            case typeof(Autodesk.Revit.DB.FilterNumericLess):
                                                                                bhomEvaluator = NumberComparisonType.Less;
                                                                                break;
                                                                            case typeof(Autodesk.Revit.DB.FilterNumericLessOrEqual):
                                                                                bhomEvaluator = NumberComparisonType.LessOrEqual;
                                                                                break;
                                                                            default:
                                                                                break;
                                                                        }

                                                                        BH.oM.Revit.Elements.FilterIntegerRule bhomFilterIntegerRule;
                                                                        bhomFilterIntegerRule = new oM.Revit.Elements.FilterIntegerRule();
                                                                        bhomFilterIntegerRule.ParameterName = paramName;
                                                                        bhomFilterIntegerRule.Value = paramValue;
                                                                        bhomFilterIntegerRule.Evaluator = bhomEvaluator;

                                                                        return bhomFilterIntegerRule;
                                                                    }

                                                                    // FILTER CATEGORY RULE
                                                                    else if (revitRule.GetType().IsSubclassOf(typeof(Autodesk.Revit.DB.FilterCategoryRule)))
                                                                    {
                                                                        Autodesk.Revit.DB.FilterCategoryRule revitFilterCategoryRule = (Autodesk.Revit.DB.FilterCategoryRule)revitRule;
                                                                        List<string> categoryNames = revitFilterCategoryRule.GetCategories().Select(elId => revitViewFilter.Document.GetElement(elId).Name).ToList();

                                                                        BH.oM.Revit.Elements.FilterCategoryRule bhomFilterCategoryRule;
                                                                        bhomFilterCategoryRule = new oM.Revit.Elements.FilterCategoryRule();
                                                                        bhomFilterCategoryRule.CategoryNames = categoryNames;

                                                                        return bhomFilterCategoryRule;
                                                                    }

                                                                    // FILTER INVERSE RULE
                                                                    else if (revitRule.GetType().IsSubclassOf(typeof(Autodesk.Revit.DB.FilterInverseRule)))
                                                                    {
                                                                        Autodesk.Revit.DB.FilterInverseRule revitFilterInverseRule = (Autodesk.Revit.DB.FilterInverseRule)revitRule;
                                                                        Autodesk.Revit.DB.FilterRule innerRule = revitFilterInverseRule.GetInnerRule();
                                                                        TextComparisonType bhomTextEvaluator;
                                                                        NumberComparisonType bhomNumberEvaluator;
                    
                                                                        switch (innerRule.GetType())
                                                                        {
                                                                            case (typeof(Autodesk.Revit.DB.FilterStringRule)):
                                                                                switch (((Autodesk.Revit.DB.FilterStringRule)innerRule).GetEvaluator().GetType()) {
                                                                                            case (typeof(FilterStringEquals)):
                                                                                                bhomTextEvaluator = TextComparisonType.NotEqual;
                                                                                                break;
                                                                                            case (typeof(FilterStringBeginsWith)):
                                                                                                bhomTextEvaluator = TextComparisonType.NotStartsWith;
                                                                                                break;
                                                                                            case (typeof(FilterStringEndsWith)):
                                                                                                bhomTextEvaluator = TextComparisonType.NotEndsWith;
                                                                                                break;
                                                                                            case (typeof(FilterStringContains)):
                                                                                                bhomTextEvaluator = TextComparisonType.ContainsNot;
                                                                                                break;
                                                                                            default:
                                                                                                break;}
                                                                                BH.oM.Revit.Elements.FilterStringRule bhomFilterStringRule = new oM.Revit.Elements.FilterStringRule;

                                                                        }

                                                                        return ;
                                                                    }

                                                                });
            //Set identifiers, parameters & custom data
            viewFilter.SetIdentifiers(revitViewFilter);
            viewFilter.CopyParameters(revitViewFilter, settings.MappingSettings);
            viewFilter.SetProperties(revitViewFilter, settings.MappingSettings);
            refObjects.AddOrReplace(revitViewFilter.Id, viewFilter);
            return viewFilter;
        }

        /***************************************************/
    }
}





