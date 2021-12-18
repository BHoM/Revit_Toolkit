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

using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using BH.oM.Reflection.Attributes;
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
        public static List<PolyCurve> Perimeter(this SpatialElement spatialElement, RevitSettings settings = null)
        {
            if (spatialElement == null)
                return null;

            IList<IList<BoundarySegment>> boundarySegments = spatialElement.GetBoundarySegments(new SpatialElementBoundaryOptions());
            if (boundarySegments == null)
                return null;

            List<PolyCurve> results = new List<PolyCurve>();

            foreach (IList<BoundarySegment> boundarySegmentList in boundarySegments)
            {
                if (boundarySegmentList == null)
                    continue;

                List<BH.oM.Geometry.ICurve> curves = new List<ICurve>();
                foreach (BoundarySegment boundarySegment in boundarySegmentList)
                {
                    curves.Add(boundarySegment.GetCurve().IFromRevit());
                }

                results.Add(BH.Engine.Geometry.Create.PolyCurve(curves));
            }

            return results;
        }
        
        /***************************************************/
    }
}
