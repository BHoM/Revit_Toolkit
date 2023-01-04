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

using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Properties;
using BH.oM.Geometry;
using BH.oM.Base.Attributes;
using BH.oM.Revit;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates ModelInstance object based on point location, Revit family name and family type name. Such ModelInstance can be pushed to Revit as a point-driven element, e.g. chair.")]
        [Input("familyName", "Name of Revit family to be used when creating the element.")]
        [Input("familyTypeName", "Name of Revit family type to be used when creating the element.")]
        [InputFromProperty("location")]
        [InputFromProperty("orientation")]
        [Input("hostId", "ElementId of the Revit element hosting the element represented by this ModelInstance.")]
        [Input("hostDocument", "If the host element belongs to a Revit link document, the name, path or ElementId of the Revit link instance needs to be specified here.")]
        [Output("modelInstance")]
        public static ModelInstance ModelInstance(string familyName, string familyTypeName, Point location, Basis orientation = null, int hostId = -1, string hostDocument = "")
        {
            if (location == null || string.IsNullOrWhiteSpace(familyTypeName) || string.IsNullOrWhiteSpace(familyName))
                return null;

            return ModelInstance(Create.InstanceProperties(familyName, familyTypeName), location, orientation, hostId, hostDocument);
        }

        /***************************************************/

        [Description("Creates ModelInstance object based on curve location, Revit family name and family type name. Such ModelInstance can be pushed to Revit as a curve-driven element, e.g. duct.")]
        [Input("familyName", "Name of Revit family to be used when creating the element.")]
        [Input("familyTypeName", "Name of Revit family type to be used when creating the element.")]
        [InputFromProperty("location")]
        [Input("hostId", "ElementId of the Revit element hosting the element represented by this ModelInstance.")]
        [Input("hostDocument", "If the host element belongs to a Revit link document, the name, path or ElementId of the Revit link instance needs to be specified here.")]
        [Output("modelInstance")]
        public static ModelInstance ModelInstance(string familyName, string familyTypeName, ICurve location, int hostId = -1, string hostDocument = "")
        {
            if (location == null || string.IsNullOrWhiteSpace(familyTypeName) || string.IsNullOrWhiteSpace(familyName))
                return null;

            return ModelInstance(Create.InstanceProperties(familyName, familyTypeName), location, hostId, hostDocument);
        }

        /***************************************************/

        [Description("Creates ModelInstance object based on collection of points, Revit family name and family type name. Such ModelInstance can be pushed to Revit as an Adaptive Component.\n"
                   + "The collection of points may contain either only the location points or both location points and shape handles, in order correspondent to the order of adaptive points in the family definition.")]
        [Input("familyName", "Name of Revit family to be used when creating the element.")]
        [Input("familyTypeName", "Name of Revit family type to be used when creating the element.")]
        [InputFromProperty("location")]
        [Output("modelInstance")]
        public static ModelInstance ModelInstance(string familyName, string familyTypeName, List<Point> location)
        {
            if (location == null || string.IsNullOrWhiteSpace(familyTypeName) || string.IsNullOrWhiteSpace(familyName))
                return null;

            return ModelInstance(Create.InstanceProperties(familyName, familyTypeName), location);
        }

        /***************************************************/

        [Description("Creates ModelInstance object based on point location and BHoM InstanceProperties. Such ModelInstance can be pushed to Revit as a point-driven element, e.g. chair.")]
        [InputFromProperty("properties")]
        [InputFromProperty("location")]
        [InputFromProperty("orientation")]
        [Input("hostId", "ElementId of the Revit element hosting the element represented by this ModelInstance.")]
        [Input("hostDocument", "If the host element belongs to a Revit link document, the name, path or ElementId of the Revit link instance needs to be specified here.")]
        [Output("modelInstance")]
        public static ModelInstance ModelInstance(InstanceProperties properties, Point location, Basis orientation = null, int hostId = -1, string hostDocument = "")
        {
            if (properties == null || location == null)
                return null;

            ModelInstance modelInstance = new ModelInstance()
            {
                Properties = properties,
                Name = properties.Name,
                Location = location,
                Orientation = orientation
            };

            if (hostId != -1)
                modelInstance.Fragments.AddOrReplace(new RevitHostFragment(hostId, hostDocument));

            return modelInstance;
        }

        /***************************************************/

        [Description("Creates ModelInstance object based on curve location and BHoM InstanceProperties. Such ModelInstance can be pushed to Revit as a curve-driven element, e.g. duct.")]
        [InputFromProperty("properties")]
        [InputFromProperty("location")]
        [Input("hostId", "ElementId of the Revit element hosting the element represented by this ModelInstance.")]
        [Input("hostDocument", "If the host element belongs to a Revit link document, the name, path or ElementId of the Revit link instance needs to be specified here.")]
        [Output("modelInstance")]
        public static ModelInstance ModelInstance(InstanceProperties properties, ICurve location, int hostId = -1, string hostDocument = "")
        {
            if (properties == null || location == null)
                return null;

            ModelInstance modelInstance = new ModelInstance()
            {
                Properties = properties,
                Name = properties.Name,
                Location = location
            };

            if (hostId != -1)
                modelInstance.Fragments.AddOrReplace(new RevitHostFragment(hostId, hostDocument));

            return modelInstance;
        }

        /***************************************************/

        [Description("Creates ModelInstance object based on collection of points and BHoM InstanceProperties. Such ModelInstance can be pushed to Revit as an Adaptive Component.\n"
                   + "The collection of points may contain either only the location points or both location points and shape handles, in order correspondent to the order of adaptive points in the family definition.")]
        [InputFromProperty("properties")]
        [InputFromProperty("location")]
        [Output("modelInstance")]
        public static ModelInstance ModelInstance(InstanceProperties properties, List<Point> location)
        {
            if (properties == null || location == null)
                return null;

            ModelInstance modelInstance = new ModelInstance()
            {
                Properties = properties,
                Name = properties.Name,
                Location = new CompositeGeometry { Elements = new List<IGeometry>(location) },
            };

            return modelInstance;
        }

        /***************************************************/

        [Description("Creates ModelInstance object based on curve location. Such ModelInstance can be pushed to Revit as model line.")]
        [InputFromProperty("location")]
        [Output("modelInstance")]
        public static ModelInstance ModelInstance(ICurve location)
        {
            if (location == null)
                return null;

            InstanceProperties instanceProperties = new InstanceProperties();
            instanceProperties.CategoryName = "Lines";

            ModelInstance modelInstance = new ModelInstance()
            {
                Properties = instanceProperties,
                Name = "Lines",
                Location = location
            };

            return modelInstance;
        }

        /***************************************************/

        [Description("Creates ModelInstance object based on curve location. Such ModelInstance can be pushed to Revit as model line.")]
        [Input("name", "Name of Revit line type to be applied to the model line.")]
        [InputFromProperty("location")]
        [Output("modelInstance")]
        public static ModelInstance ModelInstance(string name, ICurve location)
        {
            if (location == null)
                return null;

            InstanceProperties instanceProperties = new InstanceProperties();
            instanceProperties.Name = name;
            instanceProperties.CategoryName = "Lines";

            ModelInstance modelInstance = new ModelInstance()
            {
                Properties = instanceProperties,
                Name = "Lines",
                Location = location
            };

            return modelInstance;
        }

        /***************************************************/

        [Description("Creates ModelInstance object based on surface location. Such ModelInstance can be pushed to Revit as DirectShape.")]
        [Input("categoryName", "Name of Revit category, to which the pushed DirectShape element will be assigned.")]
        [InputFromProperty("location")]
        [Output("modelInstance")]
        public static ModelInstance ModelInstance(string categoryName, ISurface location)
        {
            if (string.IsNullOrWhiteSpace(categoryName) || location == null)
                return null;

            InstanceProperties instanceProperties = new InstanceProperties();
            instanceProperties.CategoryName = categoryName;

            ModelInstance modelInstance = new ModelInstance()
            {
                Properties = instanceProperties,
                Name = "Surface",
                Location = location
            };

            return modelInstance;
        }

        /***************************************************/

        [Description("Creates ModelInstance object based on solid location. Such ModelInstance can be pushed to Revit as DirectShape.")]
        [Input("categoryName", "Name of Revit category, to which the pushed DirectShape element will be assigned.")]
        [InputFromProperty("location")]
        [Output("modelInstance")]
        public static ModelInstance ModelInstance(string categoryName, ISolid location)
        {
            if (string.IsNullOrWhiteSpace(categoryName) || location == null)
                return null;

            InstanceProperties instanceProperties = new InstanceProperties();
            instanceProperties.CategoryName = categoryName;

            ModelInstance modelInstance = new ModelInstance()
            {
                Properties = instanceProperties,
                Name = "Solid",
                Location = location
            };

            return modelInstance;
        }

        /***************************************************/

        [Description("Creates ModelInstance object based on other BHoMObject's properties. After assigning location to it, such ModelInstance can be pushed to Revit as any model element.")]
        [Input("bHoMObject", "BHoM object to inherit properties from.")]
        [Output("modelInstance")]
        public static ModelInstance ModelInstance(oM.Base.IBHoMObject bHoMObject)
        {
            if (bHoMObject == null)
                return null;

            InstanceProperties instanceProperties = new InstanceProperties()
            {
                Name = bHoMObject.Name,
                CustomData = new System.Collections.Generic.Dictionary<string, object>(bHoMObject.CustomData)
            };

            ModelInstance modelInstance = new ModelInstance()
            {
                Name = bHoMObject.Name,
                Properties = instanceProperties,
                CustomData = new System.Collections.Generic.Dictionary<string, object>(bHoMObject.CustomData)
            };

            return modelInstance;
        }
        
        /***************************************************/
    }
}





