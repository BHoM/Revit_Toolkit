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

using System.ComponentModel;

namespace BH.oM.Adapters.Revit
{
    public static class Tolerance
    {
        /***************************************************/
        /**** Constants                                 ****/
        /***************************************************/

        [Description("Maximum distance between two vertices to consider them identical - value taken from Autodesk.Revit.ApplicationServices.Application.VertexTolerance constant.")]
        public const double Vertex = 0.0005233832795 * 0.3048;

        [Description("Minimum distance of a curve to consider it valid - value taken from Autodesk.Revit.ApplicationServices.Application.ShortCurveTolerance constant.")]
        public const double ShortCurve = 0.00256026455729167 * 0.3048;

        [Description("Maximum difference between two angles to consider them identical - value taken from Autodesk.Revit.ApplicationServices.Application.AngleTolerance constant.")]
        public const double Angle = 0.00174532925199433;

        /***************************************************/
    }
}

