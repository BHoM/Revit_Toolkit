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
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Revit.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using FilterRule = BH.oM.Revit.Elements.FilterRule;
using OverrideGraphicSettings = BH.oM.Adapters.Revit.Elements.OverrideGraphicSettings;
using View = Autodesk.Revit.DB.View;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit View to BH.oM.Adapters.Revit.Elements.View.")]
        [Input("revitView", "Revit View to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("view", "BH.oM.Adapters.Revit.Elements.View resulting from converting the input Revit View.")]
        public static oM.Adapters.Revit.Elements.View ViewFromRevit(this View revitView, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            oM.Adapters.Revit.Elements.View view = refObjects.GetValue<oM.Adapters.Revit.Elements.View>(revitView.Id.IntegerValue);
            if (view != null)
                return view;

            /* 1. Transfer NAME */
            view = new oM.Adapters.Revit.Elements.View { Name = revitView.Name };

            List<ViewFilterWithOverrides> filtersWithOverrides;
            List<ViewFilter> viewFilters;
            List<OverrideGraphicSettings> overrides;

            List<List<string>> categoriesNames=revitView.GetFilters()
                .Select(elId => revitView.Document.GetElement(elId))
                .Cast<ParameterFilterElement>()
                .Select(pfe => pfe.GetCategories().ToList<ElementId>())
                .Select(catIdsList => catIdsList.Select(catId => revitView.Document.GetElement(catId).Name).ToList<string>())
                .ToList<List<string>>();

            List<FilterRule> filterRules = revitView.GetFilters()
                .Select(elId => revitView.Document.GetElement(elId))
                .Cast<ElementParameterFilter>()
                .Select(epf => epf.GetRules())
                .ToDictionary<System.Type, string[]>(fvr => fvr.GetType(),
                                                     fvr =>
                                                     {
                                                         Element param = revitView.Document.GetElement(fvr.GetRuleParameter());
                                                         string paramName = param.Name;
                                                         string value = fvr.ToString();
                                                         return new Array<string> { paramName, value };
                                                     })
                .ToList<KeyValuePair<System.Type, string[]>>()
                .Select(kvp => new FilterRule { RuleType = FilterRuleType.BEGINSWITH, 
                                                ParameterName = kvp.Value[0], 
                                                Value = kvp.Value[1] })
                .


            //TODO: here goes the convertion method

            //Set identifiers, parameters & custom data
            view.SetIdentifiers(revitView);
            view.CopyParameters(revitView, settings.MappingSettings);
            view.SetProperties(revitView, settings.MappingSettings);

            refObjects.AddOrReplace(revitView.Id, view);
            return view;
        }

        /***************************************************/
    }
}





