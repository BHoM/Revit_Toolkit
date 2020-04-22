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

using BH.Adapter.Revit;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Properties;
using BH.oM.Geometry;
using BH.oM.Reflection.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates DraftingInstance object based on point location, Revit family name and family type name. Such DraftingInstance can be pushed to Revit as a point-driven drafting element.")]
        [Input("familyName", "Name of Revit family to be used when creating the element.")]
        [Input("familyTypeName", "Name of Revit family type to be used when creating the element.")]
        [InputFromProperty("viewName")]
        [InputFromProperty("location")]
        [Output("draftingInstance")]
        public static DraftingInstance DraftingInstance(string familyName, string familyTypeName, string viewName, Point location)
        {
            if (string.IsNullOrWhiteSpace(familyName) || string.IsNullOrWhiteSpace(familyTypeName) || string.IsNullOrWhiteSpace(viewName) || location == null)
                return null;

            return DraftingInstance(Create.InstanceProperties(familyName, familyTypeName), viewName, location);
        }

        /***************************************************/

        [Description("Creates DraftingInstance object based on point location and BHoM InstanceProperties. Such DraftingInstance can be pushed to Revit as a point-driven drafting element.")]
        [InputFromProperty("properties")]
        [InputFromProperty("viewName")]
        [InputFromProperty("location")]
        [Output("draftingInstance")]
        public static DraftingInstance DraftingInstance(InstanceProperties properties, string viewName, Point location)
        {
            if (properties == null || string.IsNullOrWhiteSpace(viewName) || location == null)
                return null;

            DraftingInstance draftingInstance = new DraftingInstance()
            {
                Properties = properties,
                Name = properties.Name,
                ViewName = viewName,
                Location = location
            };

            return draftingInstance;
        }

        /***************************************************/

        [Description("Creates DraftingInstance object based on curve location and BHoM InstanceProperties. Such DraftingInstance can be pushed to Revit as a detail line.")]
        [InputFromProperty("properties")]
        [InputFromProperty("viewName")]
        [InputFromProperty("location")]
        [Output("draftingInstance")]
        public static DraftingInstance DraftingInstance(InstanceProperties properties, string viewName, ICurve location)
        {
            if (properties == null || string.IsNullOrWhiteSpace(viewName) || location == null)
                return null;

            DraftingInstance draftingInstance = new DraftingInstance()
            {
                Properties = properties,
                Name = properties.Name,
                ViewName = viewName,
                Location = location
            };

            return draftingInstance;
        }

        /***************************************************/

        [Description("Creates DraftingInstance object based on curve location. Such DraftingInstance can be pushed to Revit as a detail line with default graphic style.")]
        [InputFromProperty("viewName")]
        [InputFromProperty("location")]
        [Output("draftingInstance")]
        public static DraftingInstance DraftingInstance(string viewName, ICurve location)
        {
            if (string.IsNullOrWhiteSpace(viewName) || location == null)
                return null;

            InstanceProperties instanceProperties = new InstanceProperties();
            instanceProperties.CustomData.Add(RevitAdapter.CategoryName, "Lines");

            DraftingInstance draftingInstance = new DraftingInstance()
            {
                Properties = instanceProperties,
                Name = "Detail Lines",
                ViewName = viewName,
                Location = location
            };
            draftingInstance.CustomData.Add(RevitAdapter.CategoryName, "Lines");

            return draftingInstance;
        }

        /***************************************************/

        [Description("Creates DraftingInstance object based on curve location and graphic style name. Such DraftingInstance can be pushed to Revit as a detail line.")]
        [Input("name", "Name of Revit graphic style to be used to create the element on Push.")]
        [InputFromProperty("viewName")]
        [InputFromProperty("location")]
        [Output("draftingInstance")]
        public static DraftingInstance DraftingInstance(string name, string viewName, ICurve location)
        {
            if (string.IsNullOrWhiteSpace(viewName) || location == null)
                return null;

            InstanceProperties instanceProperties = new InstanceProperties();
            instanceProperties.Name = name;
            instanceProperties.CustomData.Add(RevitAdapter.CategoryName, "Lines");

            DraftingInstance draftingInstance = new DraftingInstance()
            {
                Properties = instanceProperties,
                Name = "Detail Lines",
                ViewName = viewName,
                Location = location
            };
            draftingInstance.CustomData.Add(RevitAdapter.CategoryName, "Lines");

            return draftingInstance;
        }

        /***************************************************/

        [Description("Creates DraftingInstance object based on curve location, Revit family name and family type name. Such DraftingInstance can be pushed to Revit as a view specific, curve-driven element, e.g. load line.")]
        [Input("familyName", "Name of Revit family to be used when creating the element.")]
        [Input("familyTypeName", "Name of Revit family type to be used when creating the element.")]
        [InputFromProperty("viewName")]
        [InputFromProperty("location")]
        [Output("draftingInstance")]
        public static DraftingInstance DraftingInstance(string familyName, string familyTypeName, string viewName, ICurve location)
        {
            if (string.IsNullOrWhiteSpace(familyName) || string.IsNullOrWhiteSpace(familyTypeName) || string.IsNullOrWhiteSpace(viewName) || location == null)
                return null;

            return DraftingInstance(Create.InstanceProperties(familyName, familyTypeName), viewName, location);
        }

        /***************************************************/

        [Description("Creates DraftingInstance object based on surface location and Revit filled region type name. Such DraftingInstance can be pushed to Revit as a filled region.")]
        [Input("name", "Name of Revit filled region type to be used when creating the element.")]
        [InputFromProperty("viewName")]
        [InputFromProperty("location")]
        [Output("draftingInstance")]
        public static DraftingInstance DraftingInstance(string name, string viewName, ISurface location)
        {
            InstanceProperties instanceProperties = new InstanceProperties();
            instanceProperties.Name = name;

            return Create.DraftingInstance(instanceProperties, viewName, location);
        }

        /***************************************************/

        [Description("Creates DraftingInstance object based on surface location and BHoM InstanceProperties. Such DraftingInstance can be pushed to Revit as a filled region.")]
        [InputFromProperty("properties")]
        [InputFromProperty("viewName")]
        [InputFromProperty("location")]
        [Output("draftingInstance")]
        public static DraftingInstance DraftingInstance(InstanceProperties properties, string viewName, ISurface location)
        {
            if (properties == null || string.IsNullOrWhiteSpace(viewName))
                return null;

            List<PlanarSurface> surfaces = new List<PlanarSurface>();
            if (location is PlanarSurface)
                surfaces.Add((PlanarSurface)location);
            else if (location is PolySurface)
            {
                PolySurface polySurface = (PolySurface)location;
                if (polySurface.Surfaces.Any(x => !(x is PlanarSurface)))
                {
                    BH.Engine.Reflection.Compute.RecordError("Only PlanarSurfaces and PolySurfaces consisting of PlanarSurfaces can be used as location for ISurface-based DraftingInstances.");
                    return null;
                }

                surfaces = polySurface.Surfaces.Cast<PlanarSurface>().ToList();
            }
            else
            {
                BH.Engine.Reflection.Compute.RecordError("Only PlanarSurfaces and PolySurfaces consisting of PlanarSurfaces can be used as location for ISurface-based DraftingInstances.");
                return null;
            }

            foreach (PlanarSurface surface in surfaces)
            {
                Vector normal = (surface).Normal();
                if (normal == null || 1 - Math.Abs(normal.DotProduct(Vector.ZAxis)) > Tolerance.Angle)
                {
                    BH.Engine.Reflection.Compute.RecordError("Normal of the surface or its components is not parallel to the global Z axis.");
                    return null;
                }
            }
            
            DraftingInstance draftingInstance = new DraftingInstance()
            {
                Properties = properties,
                Name = properties.Name,
                ViewName = viewName,
                Location = location
            };

            return draftingInstance;
        }


        /***************************************************/
        /****            Deprecated methods             ****/
        /***************************************************/

        [Deprecated("3.1", "Location type has been generalized from PlanarSurface to ISurface.")]
        public static DraftingInstance DraftingInstance(string name, string viewName, PlanarSurface location)
        {
            return Create.DraftingInstance(name, viewName, location as ISurface);
        }

        /***************************************************/

        [Deprecated("3.1", "Location type has been generalized from PlanarSurface to ISurface.")]
        public static DraftingInstance DraftingInstance(InstanceProperties properties, string viewName, PlanarSurface location)
        {
            return Create.DraftingInstance(properties, viewName, location as ISurface);
        }

        /***************************************************/
    }
}

