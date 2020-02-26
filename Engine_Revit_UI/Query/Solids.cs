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

using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit.Settings;
using System.Collections.Generic;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/
        
        public static List<Solid> Solids(this GeometryElement geometryElement, Transform transform = null, RevitSettings settings = null)
        {
            if (geometryElement == null)
                return null;

            List<Solid> result = new List<Solid>();
            foreach (GeometryObject geomObject in geometryElement)
            {
                if (geomObject is GeometryInstance)
                {
                    GeometryInstance geomInstance = (GeometryInstance)geomObject;

                    Transform transformation = geomInstance.Transform;
                    if (transform != null)
                        transformation = transformation.Multiply(transform.Inverse);

                    GeometryElement geomElement = geomInstance.GetInstanceGeometry(transformation);
                    if (geomElement == null)
                        continue;

                    List<Solid> solids = Solids(geomElement);
                    if (solids != null && solids.Count != 0)
                        result.AddRange(solids);
                }
                else if (geomObject is Solid)
                    result.Add((Solid)geomObject);
            }

            return result;
        }

        /***************************************************/

        public static List<Solid> Solids(this Element element, Options options, RevitSettings settings = null)
        {
            GeometryElement geomElement = element.get_Geometry(options);

            Transform transform = null;
            if (element is FamilyInstance)
                transform = ((FamilyInstance)element).GetTotalTransform();

            return geomElement.Solids(transform, settings);
        }

        /***************************************************/
    }
}

