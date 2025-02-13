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
using BH.Engine.Adapters.Revit;
using BH.Engine.Verification;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Requests;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;
using BH.oM.Data.Requests;
using System;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****             Interface methods             ****/
        /***************************************************/

        [Description("Checks whether a given Revit Element passes the filtering criteria contained within the IRequest.")]
        [Input("element", "Revit Element to be checked against the IRequest.")]
        [Input("request", "IRequest containing the filtering criteria, against which the Revit element is checked.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the element against the filtering criteria.")]
        [Output("passes", "True if the input Revit Element passes the filtering criteria contained within the IRequest, otherwise false.")]
        public static bool IPasses(this Element element, IRequest request, Discipline discipline = Discipline.Undefined, RevitSettings settings = null)
        {
            if (!CheckIfNotNull(element, request))
                return false;

            return Passes(element, request as dynamic, discipline, settings);
        }


        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Checks whether a given element passes the filtering criteria contained within the FilterByCategory request.")]
        [Input("element", "Element to be checked against the FilterByCategory request.")]
        [Input("request", "FilterByCategory request containing the filtering criteria, against which the element is checked.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the element against the filtering criteria.")]
        [Output("passes", "True if the element passes the filtering criteria contained within the FilterByCategory request, otherwise false.")]
        public static bool Passes(this Element element, FilterByCategory request, Discipline discipline = Discipline.Undefined, RevitSettings settings = null)
        {
            if (!CheckIfNotNull(element, request))
                return false;

            string categoryName = element.Category.Name;
            string soughtName = request.CategoryName;
            if (!request.CaseSensitive)
            {
                categoryName = categoryName.ToLower();
                soughtName = soughtName.ToLower();
            }

            return categoryName == soughtName;
        }

        /***************************************************/

        [Description("Checks whether a given element passes the filtering criteria contained within the ConditionRequest request.")]
        [Input("element", "Element to be checked against the ConditionRequest request.")]
        [Input("request", "ConditionRequest request containing the filtering criteria, against which the element is checked.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the element against the filtering criteria.")]
        [Output("passes", "True if the element passes the filtering criteria contained within the ConditionRequest request, otherwise false.")]
        public static bool Passes(this Element element, ConditionRequest request, Discipline discipline = Discipline.Undefined, RevitSettings settings = null)
        {
            return element.IPasses(request.Condition) == true;
        }

        /***************************************************/

        [Description("Checks whether a given Revit Element passes the filtering criteria contained within the FilterEverything request.\n" +
                     "In practice always returns true.")]
        [Input("element", "Revit Element to be checked against the FilterEverything request.")]
        [Input("request", "FilterEverything request containing the filtering criteria, against which the Revit element is checked.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the element against the filtering criteria.")]
        [Output("passes", "Always true because FilterEverything request accepts all Revit elements.")]
        public static bool Passes(this Element element, FilterEverything request, Discipline discipline = Discipline.Undefined, RevitSettings settings = null)
        {
            return true;
        }

        /***************************************************/

        [Description("Checks whether a given Revit Element passes the filtering criteria contained within the FilterByParameterExistence request.")]
        [Input("element", "Revit Element to be checked against the FilterByParameterExistence request.")]
        [Input("request", "FilterByParameterExistence request containing the filtering criteria, against which the Revit element is checked.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the element against the filtering criteria.")]
        [Output("passes", "True if the input Revit Element passes the filtering criteria contained within the FilterByParameterExistence request, otherwise false.")]
        public static bool Passes(this Element element, FilterByParameterExistence request, Discipline discipline = Discipline.Undefined, RevitSettings settings = null)
        {
            if (!CheckIfNotNull(element, request))
                return false;

            if (discipline == Discipline.Undefined)
                discipline = Discipline.Physical;

            settings = settings.DefaultIfNull();

            Type bHoMType = element.BHoMType(discipline, settings);
            oM.Adapters.Revit.Mapping.ParameterMap parameterMap = settings?.MappingSettings?.ParameterMap(bHoMType);
            if (parameterMap != null)
                BH.Engine.Base.Compute.RecordWarning($"A parameter map has been found for the BHoM type {bHoMType.Name} and discipline {discipline} - FilterByParameterExistence request does not support parameter mapping so it was neglected.");

            return (element.LookupParameter(request.ParameterName) != null) == request.ParameterExists;
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        [Description("Checks whether either of the given Revit element and IRequest is null and raises error if so.")]
        [Input("element", "Revit Element to be checked against null value.")]
        [Input("request", "IRequest to be checked against null value.")]
        [Output("notNull", "If true, neither the input Revit Element nor the IRequest is null. Otherwise false.")]
        private static bool CheckIfNotNull(Element element, IRequest request)
        {
            if (element == null)
            {
                BH.Engine.Base.Compute.RecordError("The element cannot be checked against the request because it is null.");
                return false;
            }

            if (request == null)
            {
                BH.Engine.Base.Compute.RecordError("The element cannot be checked against the request because the request is null.");
                return false;
            }

            return true;
        }

        /***************************************************/
    }
}
