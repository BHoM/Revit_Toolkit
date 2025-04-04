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
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/
        [Description("Queries the name (consisting of Name parameter value and number) of a given Revit spatial element.")]
        [Input("spatialElement", "Revit spatial element to query the name from.")]
        [Output("name", "Name of the input Revit spatial element consisting of Name parameter value and number.")]
        public static string Name(this SpatialElement spatialElement)
        {
            if (spatialElement == null)
                return null;

            string name = null;
            string number = spatialElement.Number;

            Parameter parameter = spatialElement.get_Parameter(BuiltInParameter.ROOM_NAME);
            if (parameter != null)
                name = parameter.AsString();

            string result = null;
            if (!string.IsNullOrEmpty(name))
                result = name;

            if (!string.IsNullOrEmpty(number))
            {
                if (string.IsNullOrEmpty(result))
                    result = number;
                else
                    result = string.Format("{0} {1}", number, result);
            }

            return result;
        }

        /***************************************************/
    }
}



