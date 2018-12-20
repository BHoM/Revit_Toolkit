/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

using System.ComponentModel;

using BH.oM.Base;
using BH.oM.Reflection.Attributes;
using BH.oM.Environment.Elements;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Cheks whatever Building Element is external element. Works only for Building Elements pulled from analytical model and adjacency have been assigned.")]
        [Input("buildingElement", "BuildingElement pulled from Revit analytical model")]
        [Output("IsExternal")]
        public static bool IsExternal(this BuildingElement buildingElement)
        {
            if (buildingElement == null)
                return false;

            if (buildingElement.CustomData == null)
                return false;

            if (!buildingElement.CustomData.ContainsKey(Convert.SpaceId))
                return false;

            if (!buildingElement.CustomData.ContainsKey(Convert.AdjacentSpaceId))
                return false;

            int aSpaceId = buildingElement.SpaceId();
            int aAdjacentSpaceId = buildingElement.AdjacentSpaceId();

            return aSpaceId != -1 && aAdjacentSpaceId == -1;
        }

        /***************************************************/
    }
}