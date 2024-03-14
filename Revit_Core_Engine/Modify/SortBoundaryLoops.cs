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
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Sort a list of curve loops (from a spatial element) so that the first one corresponds to the main loop that contains all others.")]
        [Input("loops", "Boundary curve loops from a spatial element like a room or space.")]
        [Output("normal", "Normal vector of the surface the curve loops come from, usually come from face.ComputeNormal or face.FaceNormal if it's a planar face.")]
        public static void SortBoundaryLoops(this List<CurveLoop> loops, XYZ normal)
        {
            if (loops.Count < 2)
                return;

            //By convention, the main boundary must be counter-clockwise relative to the normal vector.
            while (loops[0].IsCounterclockwise(normal) == false)
            {
                //Shift in reverse because if the main boundary isn't at the list start, it tends to be at the end.
                loops.ShiftListInPlace(-1);
            }
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static void ShiftListInPlace<T>(this List<T> list, int offset)
        {
            //Get the number of full shifts (0 if Abs(offset) < list.Count)
            int fullShiftsCount = offset / list.Count;
            if (offset < 0)
                fullShiftsCount--;

            //Get the effective offset within the list's range
            offset -= fullShiftsCount * list.Count;

            List<T> elementsToShift = list.GetRange(0, offset);
            list.AddRange(elementsToShift);
            list.RemoveRange(0, offset);
        }

        /***************************************************/
    }
}





