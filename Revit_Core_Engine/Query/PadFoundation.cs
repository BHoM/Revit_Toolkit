/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Extracts the outer rectangular boundary of a PadFoundation as a Polyline.")]
        [Input("element", "PadFoundation element whose boundary should be extracted.")]
        [Output("outline", "Polyline representing the PadFoundation external boundary.")]
        public static Polyline PadFoundationOutline(this PadFoundation element)
        {
            ICurve outline = element?.Location?.ExternalBoundary;
            if (outline == null)
                return null;

            if (element.Location.InternalBoundaries.Count != 0)
                BH.Engine.Base.Compute.RecordWarning($"PadFoundation with internal boundaries are currently not supported, holes were skipped. BHoM_Guid: {element.BHoM_Guid}");

            if (outline.ISubParts().Any(x => !(x is Line)))
            {
                BH.Engine.Base.Compute.RecordError($"PadFoundation boundary contains non-linear curve segments. Only linear segments are currently supported. BHoM_Guid: {element.BHoM_Guid}");
                return null;
            }

            return outline.IToPolyline();
        }

        /***************************************************/

        [Description("Computes the total thickness (sum of construction layers) of a PadFoundation.")]
        [Input("element", "PadFoundation element whose thickness should be computed.")]
        [Output("thickness", "Total thickness of all construction layers.")]
        public static double PadFoundationThickness(this PadFoundation element)
        {
            oM.Physical.Constructions.Construction construction = element?.Construction as oM.Physical.Constructions.Construction;
            if (construction?.Layers == null)
                return double.NaN;

            return construction.Layers.Sum(layer => layer.Thickness);
        }

        /***************************************************/

        [Description("Returns the centroid of the PadFoundation outer boundary as a Point.")]
        [Input("element", "PadFoundation element to compute the centroid for.")]
        [Output("centroid", "Centroid of the PadFoundation outer boundary point.")]
        public static Point PadFoundationCentroid(this PadFoundation element)
        {
            return element?.PadFoundationOutline()?.Centroid();
        }

        /***************************************************/
    }
}


