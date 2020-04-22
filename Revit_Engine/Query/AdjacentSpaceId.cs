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

using BH.oM.Base;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Deprecated("3.2", "BH.Engine.Adapters.Revit.Query.AdjacentSpaceId is not used any more.")]
        [Description("Gets integer representation of adjacent space Revit ElementId (stored in CustomData) for given BHoMObject.")]
        [Input("bHoMObject", "BHoMObject to be queried.")]
        [Output("elementId")]
        public static int AdjacentSpaceId(this IBHoMObject bHoMObject)
        {
            if (bHoMObject == null)
                return -1;

            object value = null;
            if (bHoMObject.CustomData.TryGetValue("AdjacentSpaceID", out value))
            {
                if (value is string)
                {
                    int num = -1;
                    if (int.TryParse((string)value, out num))
                        return num;
                }
                else if (value is int)
                {
                    return (int)value;
                }
                else
                {
                    return -1;
                }
            }

            return -1;
        }

        /***************************************************/
    }
}
