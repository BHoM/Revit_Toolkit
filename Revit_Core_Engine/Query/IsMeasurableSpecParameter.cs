/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Private fields               ****/
        /***************************************************/

#if (REVIT2018 || REVIT2019 || REVIT2020 || REVIT2021)
        private static List<ParameterType> m_MeasurableParamTypes = new List<ParameterType>()
        {
            ParameterType.Integer,
            ParameterType.Number,
            ParameterType.Length,
            ParameterType.Area,
            ParameterType.Volume,
            ParameterType.Angle
        };
#endif

        /***************************************************/
        /****             Public Methods              ****/
        /***************************************************/

        [Description("Check if a parameter stores a values of a measurable type, such as Length or Area. Its value usually represents the same measurement but may show differently in different unit systems.")]
        [Input("parameter", "A parameter to check if it stores a values of a measurable type.")]

#if (REVIT2018 || REVIT2019 || REVIT2020 || REVIT2021)
        public static bool IsMeasurableSpecParameter(this Parameter parameter)
        {
            return m_MeasurableParamTypes.Contains(parameter.Definition.ParameterType);
        }
#else
        public static bool IsMeasurableSpecParameter(this Parameter parameter)
        {
            return UnitUtils.IsMeasurableSpec(parameter.Definition.GetDataType());
        }
#endif

        /***************************************************/
    }
}
