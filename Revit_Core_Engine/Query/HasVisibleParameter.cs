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
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns true if an element contains the given parameter among its visible parameters.")]
        [Input("element", "Revit element to check whether it has a given parameter.")]
        [Input("parameter", "Parameter to search for.")]
        [Output("hasParameter", "True if the input element contains the given parameter among its visible parameters, otherwise false.")]
        public static bool HasVisibleParameter(this Element element, BuiltInParameter parameter)
        {
            if (element == null)
                return false;

            foreach(Parameter param in element.Parameters)
            {
                if ((param.Definition as InternalDefinition)?.BuiltInParameter == parameter)
                    return true;
            }

            return false;
        }

        /***************************************************/

        [Description("Returns true if an element contains a parameter with the give name among its visible parameters.")]
        [Input("element", "Revit element to check whether it has a parameter with the given name.")]
        [Input("parameterName", "Parameter name to search for.")]
        [Output("hasParameter", "True if the input element contains a parameter with the given name among its visible parameters, otherwise false.")]
        public static bool HasVisibleParameter(this Element element, string parameterName)
        {
            if (element == null)
                return false;

            foreach (Parameter param in element.Parameters)
            {
                if (param.Definition.Name == parameterName)
                    return true;
            }

            return false;
        }

        /***************************************************/
    }
}

