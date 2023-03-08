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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Extracts the perimeter curves from a given Revit spatial element and converts them to BHoM.")]
        [Input("spatialElement", "Revit spatial element to extract the perimeter curves from.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("curves", "BHoM perimeter curves extracted from the input Revit spatial element.")]
        public static XYZ ClosestPoint(this XYZ point, IEnumerable<XYZ> pointCloud)
        {
            XYZ result = null;
            double minDistance = double.MaxValue;

            foreach (XYZ cand in pointCloud)
            {
                double sqDistance = point.SquareDistance(cand);
                if (sqDistance < minDistance)
                {
                    minDistance = sqDistance;
                    result = cand;
                }
            }

            return result;
        }

        /***************************************************/
    }
}

