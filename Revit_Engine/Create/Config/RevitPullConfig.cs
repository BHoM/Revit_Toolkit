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
        [InputFromProperty("discipline")]
        [InputFromProperty("includeClosedWorksets")]
        [InputFromProperty("includeNestedElements")]
        [InputFromProperty("geometryConfig")]
        [InputFromProperty("representationConfig")]
        [InputFromProperty("pullMaterialTakeOff")]
        [Output("revitPullConfig")]
        public static RevitPullConfig RevitPullConfig(Discipline discipline = Discipline.Undefined, bool includeClosedWorksets = false, bool includeNestedElements = true, PullGeometryConfig geometryConfig = null, PullRepresentationConfig representationConfig = null, bool pullMaterialTakeOff = false)
        {
            return new RevitPullConfig { Discipline = discipline, IncludeClosedWorksets = includeClosedWorksets, IncludeNestedElements = includeNestedElements, GeometryConfig = geometryConfig, RepresentationConfig = representationConfig, PullMaterialTakeOff = pullMaterialTakeOff };
        }

        /***************************************************/
        /****            Deprecated methods             ****/
        /***************************************************/

        [ToBeRemoved("4.0", "Replaced with a new version with updated number and order of parameters.")]
        [Description("Creates a pull action-specific configuration used for adapter interaction with Revit.")]
        [InputFromProperty("discipline")]
        [InputFromProperty("includeClosedWorksets")]
        [InputFromProperty("geometryConfig")]
        [InputFromProperty("representationConfig")]
        [InputFromProperty("includeNestedElements")]
        [Output("revitPullConfig")]
        public static RevitPullConfig RevitPullConfig(Discipline discipline = Discipline.Undefined, bool includeClosedWorksets = false, PullGeometryConfig geometryConfig = null, PullRepresentationConfig representationConfig = null, bool includeNestedElements = true)
        {
            return new RevitPullConfig { Discipline = discipline, IncludeClosedWorksets = includeClosedWorksets, GeometryConfig = geometryConfig, RepresentationConfig = representationConfig, IncludeNestedElements = includeNestedElements };
        }

        [Deprecated("3.3", "Inputs of this method have changed.")]
        [InputFromProperty("discipline")]
        [InputFromProperty("includeClosedWorksets")]
        [InputFromProperty("geometryConfig")]
        [InputFromProperty("representationConfig")]
        [Output("revitPullConfig")]
        public static RevitPullConfig RevitPullConfig(Discipline discipline = Discipline.Undefined, bool includeClosedWorksets = false, PullGeometryConfig geometryConfig = null, PullRepresentationConfig representationConfig = null)
        {
            return new RevitPullConfig { Discipline = discipline, IncludeClosedWorksets = includeClosedWorksets, GeometryConfig = geometryConfig, RepresentationConfig = representationConfig };
        }

        /***************************************************/

        [ToBeRemoved("3.2", "Non-applicable due to the changes in RevitPullConfig class.")]
        [Description("Creates a pull action-specific configuration used for adapter interaction with Revit.")]
        [InputFromProperty("discipline")]
        [InputFromProperty("includeClosedWorksets")]
        [InputFromProperty("pullEdges")]
        [InputFromProperty("includeNonVisible")]
        [Output("revitPullConfig")]
        public static RevitPullConfig RevitPullConfig(Discipline discipline = Discipline.Undefined, bool includeClosedWorksets = false, bool pullEdges = false, bool pullSurfaces = false, bool includeNonVisible = false)
        {
            PullGeometryConfig geometryConfig = new PullGeometryConfig { PullEdges = pullEdges, PullSurfaces = pullSurfaces, IncludeNonVisible = includeNonVisible };
            PullRepresentationConfig representationConfig = new PullRepresentationConfig();
            return new RevitPullConfig { Discipline = discipline, IncludeClosedWorksets = includeClosedWorksets, GeometryConfig = geometryConfig, RepresentationConfig = representationConfig };
        }

        /***************************************************/
    }
}