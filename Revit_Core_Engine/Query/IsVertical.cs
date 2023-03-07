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
using BH.oM.Base.Attributes;
using BH.oM.Geometry;
using System;
using System.ComponentModel;
using Line = Autodesk.Revit.DB.Line;
using Face = Autodesk.Revit.DB.Face;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Checks whether a Revit point lies on a face of a Revit element.")]
        [Input("point", "Revit point to be checked whether it lies on a face of an element.")]
        [Input("face", "Face of a Revit element to check the point against.")]
        [Input("tolerance", "Distance tolerance to be used in the query.")]
        [Output("onFace", "True if the input point lies on the input face within the tolerance, otherwise false.")]
        public static bool IsVertical(this Curve crv, double angleTolerance)
        {
            if (crv is Line)
            {
                XYZ dirVect = ((Line)crv).Direction;
                if (dirVect.IsAlmostEqualTo(XYZ.BasisZ) || dirVect.IsAlmostEqualTo(-XYZ.BasisZ))
                {
                    return true;
                }
            }

            XYZ eVect = (crv.GetEndPoint(1) - crv.GetEndPoint(0)).Normalize();
            double angle1 = eVect.AngleTo(XYZ.BasisZ);
            double angle2 = eVect.AngleTo(-XYZ.BasisZ);

            if (Math.Min(angle1, angle2) < angleTolerance.ToRadians())
            {
                return true;
            }

            return false;
        }

        /***************************************************/

        public static bool IsVertical(this Face face)
        {
            XYZ fNormal = face.ComputeNormal(new UV(0.5, 0.5)).Normalize();
            return fNormal.Z < Tolerance.Distance;
        }

        /***************************************************/

        private static double ToRadians(this double degrees, double pi = Math.PI)
        {
            return degrees * (pi / 180);
        }


        /***************************************************/
    }
}

