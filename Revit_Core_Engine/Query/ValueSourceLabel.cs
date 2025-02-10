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

using BH.oM.Base.Attributes;
using BH.oM.Revit.Parameters;
using BH.oM.Verification.Reporting;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Generates a human readable label for a value source based on provided value condition reporting config.")]
        [Input("valueSource", "Value source to get the label for.")]
        [Input("reportingConfig", "Reporting config to apply when generating the label.")]
        [Output("label", "Human readable label generated for the input value source.")]
        public static string ValueSourceLabel(this ParameterValueSource valueSource, IValueConditionReportingConfig reportingConfig = null)
        {
            if (valueSource == null)
            {
                BH.Engine.Base.Compute.RecordError("Can't find label of a null value source.");
                return null;
            }

            if (!string.IsNullOrWhiteSpace(reportingConfig?.ValueSourceLabelOverride))
                return reportingConfig.ValueSourceLabelOverride;
            else
                return $"Parameter {valueSource.ParameterName}";
        }

        /***************************************************/
    }
}
