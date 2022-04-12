/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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

using BH.Engine.Geometry;
using BH.oM.Base.Attributes;
using BH.oM.Geometry;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Flip the input line of a BHoM Column object if its start point is higher than its endpoint. This allows pushing it to Revit")]
        [Input("line", "Column line to check.")]
        [Output("line", "The original column line if its start point is already below its endpoint. Otherwise, the same line flipped.")]
        public static Line SortColumEndpoints(this Line columnLine)
        {
            if (Math.Abs(columnLine.Start.Z - columnLine.End.Z) <= BH.oM.Geometry.Tolerance.Distance)
            {
                BH.Engine.Base.Compute.RecordError(string.Format("Column line's start and end points have the same elevation."));
                return null;
            }
            else if (columnLine.Start.Z > columnLine.End.Z)
            {
                BH.Engine.Base.Compute.RecordNote(string.Format("The input Column line's bottom was above its top. This line has been flipped to allow pushing to Revit."));
                columnLine = columnLine.Flip();
            }
            return columnLine;
        }

        /***************************************************/
    }
}

