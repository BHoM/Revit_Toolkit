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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;
using System;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Checks whether a given Revit curve is nonlinear and vertical (planar in a vertical plane).")]
        [Input("revitCurve", "Revit curve to be checked.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("verticalNonLinear", "True if the input Revit curve is nonlinear and vertical, otherwise false.")]
        public static bool IsVerticalNonLinearCurve(this Curve revitCurve, RevitSettings settings)
        {
            if (!(revitCurve is Line))
            {
                CurveLoop curveLoop = CurveLoop.Create(new Curve[] { revitCurve });
                if (curveLoop.HasPlane())
                {
                    Plane curvePlane = curveLoop.GetPlane();
                    //Orientation angles are handled slightly differently for framing elements that have a curve fits in a plane that contains the z-vector
                    if (Math.Abs(curvePlane.Normal.DotProduct(XYZ.BasisZ)) < settings.AngleTolerance)
                        return true;
                }
            }

            return false;
        }

        /***************************************************/
    }
}


