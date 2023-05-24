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
using BH.Engine.Base;
using BH.Engine.Geometry;
using BH.Engine.Units;
using BH.oM.Base.Attributes;
using BH.oM.Geometry;
using System;
using System.ComponentModel;
using Face = Autodesk.Revit.DB.Face;
using Line = Autodesk.Revit.DB.Line;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Checks whether a planar Revit curve is based on a vertical plane.")]
        [Input("planarCurve", "A planar Revit curve.")]
        [Input("toleranceInDegree", "If the angle between the Z axis and the base plane of the input curve is greater than this value, consider it a vertical curve.")]
        [Output("isVertical", "True if the input Revit planar curve is based on a vertical plane.")]
        public static bool IsVertical(this Curve planarCurve, double toleranceInDegree)
        {
            Vector eVect = (planarCurve.GetEndPoint(1) - planarCurve.GetEndPoint(0)).Normalize().VectorFromRevit();

            return eVect.AcuteAngle(Vector.ZAxis) < toleranceInDegree.FromDegree();
        }

        /***************************************************/

        [Description("Checks whether a planar Revit face is based on a vertical plane.")]
        [Input("planarFace", "A planar Revit face.")]
        [Output("isVertical", "True if the input Revit planar face is based on a vertical plane.")]
        public static bool IsVertical(this Face planarFace)
        {
            XYZ fNormal = planarFace.ComputeNormal(new UV(0.5, 0.5)).Normalize();
            return fNormal.Z < Tolerance.Distance;
        }

        /***************************************************/
    }
}

