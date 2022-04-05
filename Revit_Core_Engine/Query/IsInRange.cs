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
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Intersection chceck between two bounding boxes.")]
        [Input("box1", "First bounding box to check the intersection for.")]
        [Input("box2", "Second bounding box to check the intersection for.")]
        [Input("tolerance", "Tolerance of interesction checking.")]
        [Output("bool", "Result of intersection checking. True if bouning boxes are interesecting, false if not.")]
        public static bool IsInRange(this BoundingBoxXYZ box1, BoundingBoxXYZ box2, double tolerance = BH.oM.Geometry.Tolerance.Distance)
        {
            if (box1 == null || box2 == null)
            {
                BH.Engine.Base.Compute.RecordError("Intersection of the bounding boxes could not be checked. One or both of the objects are null.");
                return false;
            }

            if (box1.Transform.IsTranslation == false || box2.Transform.IsTranslation == false)
            {
                BH.Engine.Base.Compute.RecordError("Intersection of the bounding boxes could not be checked. One or both of the bounding boxes has unsupported transformation.");
                return false;
            }

            if (!box1.Transform.IsIdentity)
            {
                BoundingBoxXYZ newBox1 = new BoundingBoxXYZ();
                newBox1.Min = box1.Min.Add(box1.Transform.Origin);
                newBox1.Max = box1.Max.Add(box1.Transform.Origin);
                box1 = newBox1;
            }
            if (!box2.Transform.IsIdentity)
            {
                BoundingBoxXYZ newBox2 = new BoundingBoxXYZ();
                newBox2.Min = box2.Min.Add(box2.Transform.Origin);
                newBox2.Max = box2.Max.Add(box2.Transform.Origin);
                box2 = newBox2;
            }

            return (box1.Min.X <= box2.Max.X + tolerance && box2.Min.X <= box1.Max.X + tolerance &&
                     box1.Min.Y <= box2.Max.Y + tolerance && box2.Min.Y <= box1.Max.Y + tolerance &&
                     box1.Min.Z <= box2.Max.Z + tolerance && box2.Min.Z <= box1.Max.Z + tolerance);
        }
    }
}