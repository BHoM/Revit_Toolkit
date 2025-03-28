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

using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Adapters.Revit.Requests;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Verification;
using BH.oM.Verification.Conditions;
using System;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates IRequest that filters elements by given parameter value criterion.")]
        [Input("parameterName", "Name of the parameter to query for value.")]
        [Input("idElement", "BHoMObject that represents the pulled Revit element to extract the ElementId from (contains its ElementId in RevitIdentifiers).")]
        [Output("request", "Created request.")]
        public static ConditionRequest FilterByParameterElementId(string parameterName, IBHoMObject idElement)
        {
            int elementId = idElement.ElementId();
            if (elementId == -1)
            {
                BH.Engine.Base.Compute.RecordError(String.Format("Valid ElementId has not been found. BHoM Guid: {0}", idElement.BHoM_Guid));
                return null;
            }
            else
            {
                return new ConditionRequest
                {
                    Condition = new ValueCondition
                    {
                        ValueSource = new ParameterValueSource
                        {
                            ParameterName = parameterName
                        },
                        ReferenceValue = elementId,
                        ComparisonType = ValueComparisonType.EqualTo
                    }
                };
            }
        }

        /***************************************************/
    }
}
