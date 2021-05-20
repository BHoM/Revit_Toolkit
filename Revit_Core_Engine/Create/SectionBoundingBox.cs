/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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

using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.Revit.DB;
using BH.oM.Geometry;
using BH.oM.Reflection.Attributes;
using Line = BH.oM.Geometry.Line;
using Point = BH.oM.Geometry.Point;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates BoundingBoxXYZ to be used in the creation of Section or Elevation views from a curve.")]
        [Input("line", "The straight line to be used as a plane and direction of the view.")]
        [Input("bottomElevation", "The negative bottom elevation height of the view in relation to the line.")]
        [Input("height", "The height of the view in relation to the line.")]
        [Input("offset", "The offset value to apply to the resulting bounding box, enlarging or shrinking it.")]
        [Input("depth", "The depth of the view.")]
        [Output("sectionBoundingBox","The curve's BoundingBoxXYZ to be used when creating Section or Elevation views.")]
        public static BoundingBoxXYZ SectionBoundingBox(Autodesk.Revit.DB.Line line, double bottomElevation, double height, double offset = 0, double depth = 15)
        {
            BoundingBoxXYZ result = new BoundingBoxXYZ();

            //calculates the sizes of a BB
            double lengthOffseted = line.Length + offset;
            double heightOffseted = height + (offset / 2);
            double bottomOffseted = bottomElevation - (offset / 2);

            result.Enabled = true;

            result.Max = new XYZ(lengthOffseted / 2, heightOffseted, depth);
            result.Min = new XYZ(-lengthOffseted / 2, bottomOffseted, 0);

            //transform to view coordinates
            XYZ direction = (line.GetEndPoint(1) - line.GetEndPoint(0)).Normalize();
            XYZ up = XYZ.BasisZ;
            XYZ viewDirection = direction.CrossProduct(up);

            Transform transform = Transform.Identity;

            XYZ midPoint = line.Evaluate(0.5, true);

            transform.Origin = midPoint;
            transform.BasisX = direction;
            transform.BasisY = up;
            transform.BasisZ = viewDirection;

            result.Transform = transform;           

            return result;
        }

        /***************************************************/

    }
}