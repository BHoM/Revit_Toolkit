/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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

using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/
        
        public static bool IsSimilar(this Curve curve1, Curve curve2, double tolerance = oM.Geometry.Tolerance.MicroDistance)
        {
            if (curve1 == null && curve2 == null)
                return true;

            if (curve2 == null || curve1 == null)
                return false;

            if (curve1.GetType() != curve2.GetType())
                return false;

            if (curve1.IsBound != curve2.IsBound)
                return false;

            if (Math.Abs(curve1.ApproximateLength - curve2.ApproximateLength) > tolerance)
                return false;

            if(curve1 is Line && curve2 is Line)
            {
                if (curve1.GetEndPoint(0).IsAlmostEqualTo(curve2.GetEndPoint(0), tolerance) && curve1.GetEndPoint(1).IsAlmostEqualTo(curve2.GetEndPoint(1), tolerance))
                    return true;
                else
                    return false;
            }

            IList<XYZ> xyz1 = curve1.Tessellate();
            IList<XYZ> xyz2 = curve2.Tessellate();
            if (xyz1.Count != xyz2.Count)
                return false;

            for (int i = 0; i < xyz1.Count; i++)
                if (!xyz1[i].IsAlmostEqualTo(xyz2[i], tolerance))
                    return false;

            return true;
        }

        /***************************************************/
    }
}