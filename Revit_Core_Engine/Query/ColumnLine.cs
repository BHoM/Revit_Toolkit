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

using BH.Engine.Geometry;
using BH.oM.Base.Attributes;
using BH.oM.Geometry;
using BH.oM.Physical.Elements;
using System;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Extracts the location line of a BHoM Column object in preparation to push to Revit. Includes validity checks and flipping reversed nodes.")]
        [Input("column", "A BHoM Column object to extract the line from.")]
        [Output("line", "Preprocessed location line of the BHoM Column object to be used on push to Revit.")]
        public static Line ColumnLine(this Column column)
        {
            if (column == null)
            {
                BH.Engine.Base.Compute.RecordError(string.Format("Cannot read location line because column is null. BHoM_Guid: {0}", column.BHoM_Guid));
                return null;
            }

            Line columnLine = column.Location as BH.oM.Geometry.Line;
            if (columnLine == null)
            {
                BH.Engine.Base.Compute.RecordError(string.Format("Invalid column line. Only linear columns are allowed in Revit. BHoM_Guid: {0}", column.BHoM_Guid));
                return null;
            }

            if (Math.Abs(columnLine.Start.Z - columnLine.End.Z) <= BH.oM.Geometry.Tolerance.Distance)
            {
                BH.Engine.Base.Compute.RecordError(string.Format("Column line's start and end points have the same elevation. BHoM_Guid: {0}", column.BHoM_Guid));
                return null;
            }
            
            if (columnLine.Start.Z > columnLine.End.Z)
            {
                BH.Engine.Base.Compute.RecordNote(string.Format("The input column line's bottom was above its top. This line has been flipped to allow pushing to Revit. BHoM_Guid: {0}", column.BHoM_Guid));
                columnLine = columnLine.Flip();
            }
            
            return columnLine;
        }

        /***************************************************/
    }
}


