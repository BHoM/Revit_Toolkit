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

using BH.oM.Adapters.Revit.Elements;
using BH.oM.Geometry;
using BH.oM.Reflection.Attributes;
using BH.oM.Adapters.Revit.Properties;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates ModelInstance by given Point, Family Name and Family Type Name. ModelInstance represents generic 3D elements which have not been defined in BHoM structure")]
        [Input("location", "Location Point of Object in 3D space")]
        [Input("familyName", "Revit Family Name")]
        [Input("familyTypeName", "Revit Family Type Name")]
        [Output("ModelInstance")]
        public static ModelInstance ModelInstance(string familyName, string familyTypeName, Point location)
        {
            if (location == null || string.IsNullOrWhiteSpace(familyTypeName) || string.IsNullOrWhiteSpace(familyName))
                return null;

            return ModelInstance(Create.InstanceProperties(familyName, familyTypeName), location);
        }

        /***************************************************/

        [Description("Creates ModelInstance by given Point, Family Name and Family Type Name. ModelInstance represents generic 3D elements which have not been defined in BHoM structure")]
        [Input("location", "Location Curve of Object in 3D space")]
        [Input("familyName", "Revit Family Name")]
        [Input("familyTypeName", "Revit Family Type Name")]
        [Output("ModelInstance")]
        public static ModelInstance ModelInstance(string familyName, string familyTypeName, ICurve location)
        {
            if (location == null || string.IsNullOrWhiteSpace(familyTypeName) || string.IsNullOrWhiteSpace(familyName))
                return null;

            return ModelInstance(Create.InstanceProperties(familyName, familyTypeName), location);
        }

        /***************************************************/

        [Description("Creates ModelInstance by given Point, Family Name and Family Type Name. ModelInstance represents generic 3D elements which have not been defined in BHoM structure")]
        [Input("location", "Location Point of Object in 3D space")]
        [Input("properties", "InstanceProperties of Instance")]
        [Output("ModelInstance")]
        public static ModelInstance ModelInstance(InstanceProperties properties, Point location)
        {
            if (properties == null || location == null)
                return null;

            ModelInstance modelInstance = new ModelInstance()
            {
                Properties = properties,
                Name = properties.Name,
                Location = location
            };

            return modelInstance;
        }

        /***************************************************/

        [Description("Creates ModelInstance by given Point, Family Name and Family Type Name. ModelInstance represents generic 3D elements which have not been defined in BHoM structure")]
        [Input("location", "Location Curve of Object in 3D space")]
        [Input("properties", "InstanceProperties of Instance")]
        [Output("ModelInstance")]
        public static ModelInstance ModelInstance(InstanceProperties properties, ICurve location)
        {
            if (properties == null || location == null)
                return null;

            ModelInstance modelInstance = new ModelInstance()
            {
                Properties = properties,
                Name = properties.Name,
                Location = location
            };

            return modelInstance;
        }

        /***************************************************/

        [Description("Creates ModelInstance by given Point, Family Name and Family Type Name. ModelInstance represents generic 3D elements which have not been defined in BHoM structure")]
        [Input("location", "Location Curve of Object in 3D space")]
        [Output("ModelInstance")]
        public static ModelInstance ModelInstance(ICurve location)
        {
            if (location == null)
                return null;

            InstanceProperties instanceProperties = new InstanceProperties();
            instanceProperties.CustomData.Add(Convert.CategoryName, "Lines");

            ModelInstance modelInstance = new ModelInstance()
            {
                Properties = instanceProperties,
                Name = "Detail Lines",
                Location = location
            };
            modelInstance.CustomData.Add(Convert.CategoryName, "Lines");

            return modelInstance;
        }

        /***************************************************/

        [Description("Creates ModelInstance by given Point, Family Name and Family Type Name. ModelInstance represents generic 3D elements which have not been defined in BHoM structure")]
        [Input("name", "InstanceProperties name")]
        [Input("location", "Location Curve of Object in 3D space")]
        [Output("ModelInstance")]
        public static ModelInstance ModelInstance(string name, ICurve location)
        {
            if (location == null)
                return null;

            InstanceProperties instanceProperties = new InstanceProperties();
            instanceProperties.Name = name;
            instanceProperties.CustomData.Add(Convert.CategoryName, "Lines");

            ModelInstance modelInstance = new ModelInstance()
            {
                Properties = instanceProperties,
                Name = "Detail Lines",
                Location = location
            };
            modelInstance.CustomData.Add(Convert.CategoryName, "Lines");

            return modelInstance;
        }

        /***************************************************/

        [Description("Creates ModelInstance from given IBHoMObject. ModelInstance represents generic 3D elements which have not been defined in BHoM structure")]
        [Input("bHoMObject", "IBHoMObject")]
        [Output("ModelInstance")]
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

        public static ModelInstance ModelInstance(string categoryName, ISurface location)
        {
            if (string.IsNullOrWhiteSpace(categoryName) || location == null)
                return null;

            InstanceProperties instanceProperties = new InstanceProperties();
            instanceProperties.CustomData.Add(Convert.CategoryName, categoryName);

            ModelInstance modelInstance = new ModelInstance()
            {
                Properties = instanceProperties,
                Name = "Surface",
                Location = location
            };
            modelInstance.CustomData.Add(Convert.CategoryName, categoryName);

            return modelInstance;
        }

        /***************************************************/

        public static ModelInstance ModelInstance(string categoryName, ISolid location)
        {
            if (string.IsNullOrWhiteSpace(categoryName) || location == null)
                return null;

            InstanceProperties instanceProperties = new InstanceProperties();
            instanceProperties.CustomData.Add(Convert.CategoryName, categoryName);

            ModelInstance modelInstance = new ModelInstance()
            {
                Properties = instanceProperties,
                Name = "Solid",
                Location = location
            };
            modelInstance.CustomData.Add(Convert.CategoryName, categoryName);

            return modelInstance;
        }

        /***************************************************/
    }
}


