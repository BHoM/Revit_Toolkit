/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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

        [Description("Gets the parameter type of a Revit parameter definition.")]
        [Input("definition", "Revit parameter definition to extract the parameter type from.")]
        [Output("parameterType", "Parameter type extracted from the input Revit parameter definition.")]
#if (REVIT2018 || REVIT2019 || REVIT2020)
        public static ParameterType? ParameterType(this Definition definition)
        {
            if (definition.ParameterType == Autodesk.Revit.DB.ParameterType.Invalid)
                return null;
            else
                return definition.ParameterType;
        }
#else
        public static ForgeTypeId ParameterType(this Definition definition)
        {
            ForgeTypeId result = definition?.GetDataType();
            if (string.IsNullOrWhiteSpace(result?.TypeId))
                result = null;

            return null;
        }

        /***************************************************/
#endif
    }
}




