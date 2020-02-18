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

using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates a pull action-specific configuration used for adapter interaction with Revit.")]
        [Input("discipline", "Discipline used on push/pull action. Default is Physical.")]
        [Input("includeClosedWorksets", "Elements from closed worksets will be processed if true.")]
        [Input("pullEdges", "If true, edges of elements will be pulled and stored under Revit_edges in CustomData.")]
        [Input("includeNonVisible", "Invisible element edges will be pulled and passed to CustomData if true. PullEdges switched to true needed for this to activate.")]
        [Output("RevitPullConfig")]
        public static RevitPullConfig RevitPullConfig(Discipline discipline = Discipline.Undefined, bool includeClosedWorksets = false, bool pullEdges = false, bool includeNonVisible = false)
        {
            return new RevitPullConfig { Discipline = discipline, IncludeClosedWorksets = includeClosedWorksets, PullEdges = pullEdges, IncludeNonVisible = includeNonVisible };
        }

        /***************************************************/
    }
}