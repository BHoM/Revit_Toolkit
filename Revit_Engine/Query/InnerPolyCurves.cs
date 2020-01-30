/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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

using System.ComponentModel;

using BH.oM.Geometry;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Reflection.Attributes;
using BH.oM.Environment.Elements;
using System.Collections.Generic;
using System.Linq;
using System;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns the inner PolyCurves from list. QUICK CHECK (BOUNDING BOX)")]
        [Input("polyCurve", "PolyCurve")]
        [Input("polyCurves", "PolyCurves")]
        [Output("PolyCurves")]
        public static List<PolyCurve> InnerPolyCurves(this PolyCurve polyCurve, IEnumerable<PolyCurve> polyCurves)
        {
            if (polyCurves == null || polyCurves.Count() == 0 || polyCurve == null)
                return null;

            BoundingBox bbox = Geometry.Query.Bounds(polyCurve);

            List<PolyCurve> result = new List<PolyCurve>();
            foreach (PolyCurve pcurve in polyCurves)
            {
                BoundingBox boundingBox = Geometry.Query.Bounds(pcurve);

                if (boundingBox.Min.Equals(boundingBox.Min) && boundingBox.Max.Equals(bbox.Max))
                    continue;

                if (!Geometry.Query.IsContaining(bbox, boundingBox))
                    continue;

                Point centroid = Geometry.Query.Centroid(pcurve);
                if (!Geometry.Query.IsContaining(polyCurve, new List<Point>() { centroid }))
                    continue;

                result.Add(pcurve);
            }

            return result;
        }

        /***************************************************/
    }
}


