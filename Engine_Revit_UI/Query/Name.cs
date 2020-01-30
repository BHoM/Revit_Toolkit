/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static string Name(SpatialElement spatialElement)
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

