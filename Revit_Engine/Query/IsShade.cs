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

using BH.oM.Environment.Elements;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Cheks whether environment panel is shade element. Works only for environment panels pulled from analytical model with adjacency assigned.")]
        [Input("environmentPanel", "Environment panel pulled from Revit analytical model.")]
        [Output("isShade")]
        public static bool IsShade(this Panel environmentPanel)
        {
            if (environmentPanel == null)
                return false;

            if (environmentPanel.CustomData == null)
                return false;

            if (!environmentPanel.CustomData.ContainsKey(Convert.SpaceId))
                return false;

            if (!environmentPanel.CustomData.ContainsKey(Convert.AdjacentSpaceId))
                return false;

            int spaceId = environmentPanel.SpaceId();
            int adjacenetId = environmentPanel.AdjacentSpaceId();

            return spaceId == -1 && adjacenetId == -1;
        }

        /***************************************************/
    }
}
