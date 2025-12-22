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

using Autodesk.Revit.DB;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Finds local coordinate system of an element, which is:" +
                     "\n- for FamilyInstance: coordinate system as defined in the object definition (result of GetTotalTransform method )" +
                     "\n- for everything else: coordinate system based on normals of the major planar faces of the element")]
        [Input("element", "Element to query for its local coordinate system.")]
        [Output("localCS", "Local coordinate system of the input element.")]
        public static Transform LocalCoordinateSystem(this Element element)
        {
            if (element == null)
                return null;

            if (element is FamilyInstance fi)
                return fi.GetTotalTransform();

            double tolerance = 1e-3;

            List<Line> lines = element.Curves(new Options(), null, true).OfType<Line>().ToList();
            Dictionary<XYZ, double> dirsWithLengths = new Dictionary<XYZ, double>();
            foreach (Line line in lines)
            {
                XYZ dir = dirsWithLengths.Keys.FirstOrDefault(x => 1 - Math.Abs(x.DotProduct(line.Direction)) <= tolerance);
                if (dir != null)
                    dirsWithLengths[dir] += line.Length;
                else
                    dirsWithLengths[line.Direction] = line.Length;
            }

            if (dirsWithLengths.Count == 0)
                return Transform.Identity;
            else if (dirsWithLengths.Count == 1)
            {
                XYZ first = dirsWithLengths.Keys.First();
                XYZ second;
                bool alongX = 1 - Math.Abs(first.DotProduct(XYZ.BasisX)) <= tolerance;
                if (alongX)
                    second = XYZ.BasisY;
                else
                    second = first.CrossProduct(XYZ.BasisX).CrossProduct(first);

                Transform transform = Transform.Identity;
                transform.BasisX = first;
                transform.BasisY = second;
                transform.BasisZ = first.CrossProduct(second);
                return transform;
            }
            else
            {
                XYZ first, second;
                List<XYZ> keys = dirsWithLengths.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();
                first = keys[0];
                second = null;
                double minDotProduct = 2;
                Transform transform = Transform.Identity;

                foreach (XYZ key in keys.Skip(1))
                {
                    double dotProduct = Math.Abs(first.DotProduct(key));
                    if (dotProduct <= tolerance)
                    {
                        second = key;
                        transform.BasisX = first;
                        transform.BasisY = second;
                        transform.BasisZ = first.CrossProduct(second);
                        return transform;
                    }
                    else if (dotProduct < minDotProduct)
                    {
                        minDotProduct = dotProduct;
                        second = key;
                    }
                }

                transform.BasisX = first;
                transform.BasisZ = first.CrossProduct(second).Normalize();
                transform.BasisY = transform.BasisZ.CrossProduct(first);
                return transform;
            }
        }

        /***************************************************/
    }
}

