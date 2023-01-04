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
using BH.oM.Base.Attributes;
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

        [Description("Extracts the geometry primitives from a given Revit geometry element.")]
        [Input("geometryElement", "Revit geometry element to extract the geometry primitives from.")]
        [Input("transform", "Transform to apply to the output primitives.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("primitives", "Geometry primitives extracted from the input Revit geometry element.")]
        public static List<GeometryObject> GeometryPrimitives(this GeometryElement geometryElement, Transform transform = null, RevitSettings settings = null)
        {
            if (geometryElement == null)
                return null;

            List<GeometryObject> geometry = new List<GeometryObject>();
            List<GeometryObject> instanceGeometry = new List<GeometryObject>();
            foreach (GeometryObject geometryObject in geometryElement)
            {
                if (geometryObject is GeometryInstance)
                {
                    GeometryInstance geometryInstance = (GeometryInstance)geometryObject;

                    Transform geometryTransform = geometryInstance.Transform;
                    if (transform != null)
                        geometryTransform = geometryTransform.Multiply(transform.Inverse);

                    GeometryElement geomElement = geometryInstance.GetInstanceGeometry(geometryTransform);
                    if (geomElement == null)
                        continue;

                    List<GeometryObject> instanceGeometries = geomElement.GeometryPrimitives(null, settings);
                    if (instanceGeometries != null)
                        instanceGeometry.AddRange(instanceGeometries);
                }
                else
                    geometry.Add(geometryObject);
            }

            instanceGeometry = instanceGeometry.Where(x => !(x is Solid && ((Solid)x).Faces.IsEmpty)).ToList();
            geometry = geometry.Where(x => !(x is Solid && ((Solid)x).Faces.IsEmpty)).ToList();

            return geometry.Any() ? geometry : instanceGeometry;
        }

        /***************************************************/

        [Description("Extracts the geometry primitives from a given Revit element.")]
        [Input("element", "Revit element to extract the geometry primitives from.")]
        [Input("options", "Options for parsing the geometry of a Revit element.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("primitives", "Geometry primitives extracted from the input Revit element.")]
        public static List<GeometryObject> GeometryPrimitives(this Element element, Options options, RevitSettings settings = null)
        {
            if (element == null)
                return null;

            if (element.ViewSpecific)
            {
                bool includeNonVisible = options.IncludeNonVisibleObjects;
                options = new Options();
                options.IncludeNonVisibleObjects = includeNonVisible;
                options.View = element.Document.GetElement(element.OwnerViewId) as View;
            }

            GeometryElement geometryElement = element.get_Geometry(options);
            if (geometryElement == null)
                return new List<GeometryObject>();

            Transform transform = (element as FamilyInstance)?.GetTotalTransform();

            return geometryElement.GeometryPrimitives(transform, settings);
        }

        /***************************************************/
    }
}

