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
                BH.Engine.Base.Compute.RecordError("Intersection of the bounding boxes could not be checked. Only translation transformation is currently supported.");
                return false;
            }

            XYZ min1 = box1.Min;
            XYZ max1 = box1.Max;
            XYZ min2 = box2.Min;
            XYZ max2 = box2.Max;

            if (!box1.Transform.IsIdentity)
            {
                min1 += box1.Transform.Origin;
                max1 += box1.Transform.Origin;
            }
            if (!box2.Transform.IsIdentity)
            {
                min2 += box2.Transform.Origin;
                max2 += box2.Transform.Origin;
            }

            return (min1.X <= max2.X + tolerance && min2.X <= max1.X + tolerance &&
                     min1.Y <= max2.Y + tolerance && min2.Y <= max1.Y + tolerance &&
                     min1.Z <= max2.Z + tolerance && min2.Z <= max1.Z + tolerance);
        }
    }
}
