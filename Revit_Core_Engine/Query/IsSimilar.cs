/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/
        
        [Description("Checks whether two Revit curves are similar within the tolerance specified in the settings.")]
        [Input("curve1", "First Revit curve to compare.")]
        [Input("curve2", "Second Revit curve to compare.")]
        [Input("settings", "Revit adapter settings to be used while performing the comparison.")]
        [Input("flippedIsEqual", "If true, flipped curves will be considered as similar, otherwise not.")]
        [Output("similar", "True if the input Revit curves are similar within the tolerance, otherwise false.")]
        public static bool IsSimilar(this Curve curve1, Curve curve2, RevitSettings settings, bool flippedIsEqual = false)
        {
            if (curve2 == null || curve1 == null)
                return false;

            if (curve1.IsBound != curve2.IsBound)
                return false;

            if (Math.Abs(curve1.ApproximateLength - curve2.ApproximateLength) > settings.DistanceTolerance)
                return false;

            IList<XYZ> xyz1 = curve1.Tessellate();
            IList<XYZ> xyz2 = curve2.Tessellate();
            if (xyz1.Count != xyz2.Count)
                return false;
            
            for (int i = 0; i < xyz1.Count; i++)
            {
                if (!xyz1[i].IsAlmostEqualTo(xyz2[i], settings.DistanceTolerance) && (!flippedIsEqual || !xyz1[i].IsAlmostEqualTo(xyz2[xyz2.Count - 1 - i], settings.DistanceTolerance)))
                    return false;
            }

            return true;
        }

        /***************************************************/

        [Description("Checks whether two Revit planes are similar within the tolerance specified in the settings.")]
        [Input("plane1", "First Revit plane to compare.")]
        [Input("plane2", "Second Revit plane to compare.")]
        [Input("settings", "Revit adapter settings to be used while performing the comparison.")]
        [Input("flippedIsEqual", "If true, flipped planes will be considered as similar, otherwise not.")]
        [Output("similar", "True if the input Revit planes are similar within the tolerance, otherwise false.")]
        public static bool IsSimilar(this Plane plane1, Plane plane2, RevitSettings settings, bool flippedIsEqual = false)
        {
            double dotProduct = plane1.Normal.DotProduct(plane2.Normal);
            if (flippedIsEqual)
                dotProduct = Math.Abs(dotProduct);

            if (1 - dotProduct <= settings.AngleTolerance)
                return true;

            UV uv;
            double distance;
            plane1.Project(plane2.Origin, out uv, out distance);
            return distance <= settings.DistanceTolerance;
        }

        /***************************************************/
    }
}



