/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Creates ModelInstance by given Point, Family Name and Family Type Name. ModelInstance represents generic 3D elements which have not been defined in BHoM structure")]
        [Input("point", "Location Point of Object in 3D space")]
        [Input("familyName", "Revit Family Name")]
        [Input("familyTypeName", "Revit Family Type Name")]
        [Output("ModelInstance")]
        public static ModelInstance ModelInstance(string familyName, string familyTypeName, Point point)
        {
            if (point == null || string.IsNullOrWhiteSpace(familyTypeName) || string.IsNullOrWhiteSpace(familyName))
                return null;

            return ModelInstance(Create.InstanceProperties(familyName, familyTypeName), point);
        }

        /***************************************************/

        [Description("Creates ModelInstance by given Point, Family Name and Family Type Name. ModelInstance represents generic 3D elements which have not been defined in BHoM structure")]
        [Input("curve", "Location Curve of Object in 3D space")]
        [Input("familyName", "Revit Family Name")]
        [Input("familyTypeName", "Revit Family Type Name")]
        [Output("ModelInstance")]
        public static ModelInstance ModelInstance(string familyName, string familyTypeName, ICurve curve)
        {
            if (curve == null || string.IsNullOrWhiteSpace(familyTypeName) || string.IsNullOrWhiteSpace(familyName))
                return null;

            return ModelInstance(Create.InstanceProperties(familyName, familyTypeName), curve);
        }

        /***************************************************/

        [Description("Creates ModelInstance by given Point, Family Name and Family Type Name. ModelInstance represents generic 3D elements which have not been defined in BHoM structure")]
        [Input("point", "Location Point of Object in 3D space")]
        [Input("properties", "InstanceProperties of Instance")]
        [Output("ModelInstance")]
        public static ModelInstance ModelInstance(InstanceProperties properties, Point point)
        {
            if (properties == null || point == null)
                return null;

            ModelInstance aModelInstance = new ModelInstance()
            {
                Properties = properties,
                Name = properties.Name,
                Location = point
            };

            return aModelInstance;
        }

        /***************************************************/

        [Description("Creates ModelInstance by given Point, Family Name and Family Type Name. ModelInstance represents generic 3D elements which have not been defined in BHoM structure")]
        [Input("curve", "Location Curve of Object in 3D space")]
        [Input("properties", "InstanceProperties of Instance")]
        [Output("ModelInstance")]
        public static ModelInstance ModelInstance(InstanceProperties properties, ICurve curve)
        {
            if (properties == null || curve == null)
                return null;

            ModelInstance aModelInstance = new ModelInstance()
            {
                Properties = properties,
                Name = properties.Name,
                Location = curve
            };

            return aModelInstance;
        }

        /***************************************************/

        [Description("Creates ModelInstance by given Point, Family Name and Family Type Name. ModelInstance represents generic 3D elements which have not been defined in BHoM structure")]
        [Input("curve", "Location Curve of Object in 3D space")]
        [Output("ModelInstance")]
        public static ModelInstance ModelInstance(ICurve location)
        {
            if (location == null)
                return null;

            InstanceProperties aInstanceProperties = new InstanceProperties();
            aInstanceProperties.CustomData.Add(Convert.CategoryName, "Lines");

            ModelInstance aModelInstance = new ModelInstance()
            {
                Properties = aInstanceProperties,
                Name = "Detail Lines",
                Location = location
            };
            aModelInstance.CustomData.Add(Convert.CategoryName, "Lines");

            return aModelInstance;
        }

        /***************************************************/

        [Description("Creates ModelInstance by given Point, Family Name and Family Type Name. ModelInstance represents generic 3D elements which have not been defined in BHoM structure")]
        [Input("name", "InstanceProperties name")]
        [Input("curve", "Location Curve of Object in 3D space")]
        [Output("ModelInstance")]
        public static ModelInstance ModelInstance(string name, ICurve location)
        {
            if (location == null)
                return null;

            InstanceProperties aInstanceProperties = new InstanceProperties();
            aInstanceProperties.Name = name;
            aInstanceProperties.CustomData.Add(Convert.CategoryName, "Lines");

            ModelInstance aModelInstance = new ModelInstance()
            {
                Properties = aInstanceProperties,
                Name = "Detail Lines",
                Location = location
            };
            aModelInstance.CustomData.Add(Convert.CategoryName, "Lines");

            return aModelInstance;
        }

        /***************************************************/

        [Description("Creates ModelInstance from given IBHoMObject. ModelInstance represents generic 3D elements which have not been defined in BHoM structure")]
        [Input("bHoMObject", "IBHoMObject")]
        [Output("ModelInstance")]
        public static ModelInstance ModelInstance(oM.Base.IBHoMObject bHoMObject)
        {
            if (bHoMObject == null)
                return null;

            InstanceProperties aInstanceProperties = new InstanceProperties()
            {
                Name = bHoMObject.Name,
                CustomData = new System.Collections.Generic.Dictionary<string, object>(bHoMObject.CustomData)
            };

            ModelInstance aModelInstance = new ModelInstance()
            {
                Name = bHoMObject.Name,
                Properties = aInstanceProperties,
                CustomData = new System.Collections.Generic.Dictionary<string, object>(bHoMObject.CustomData)
            };

            return aModelInstance;
        }

        /***************************************************/

        public static ModelInstance ModelInstance(string categoryName, ISurface location)
        {
            if (string.IsNullOrWhiteSpace(categoryName) || location == null)
                return null;

            InstanceProperties aInstanceProperties = new InstanceProperties();
            aInstanceProperties.CustomData.Add(Convert.CategoryName, categoryName);

            ModelInstance aModelInstance = new ModelInstance()
            {
                Properties = aInstanceProperties,
                Name = "Surface",
                Location = location
            };
            aModelInstance.CustomData.Add(Convert.CategoryName, categoryName);

            return aModelInstance;
        }

        /***************************************************/

        public static ModelInstance ModelInstance(string categoryName, ISolid location)
        {
            if (string.IsNullOrWhiteSpace(categoryName) || location == null)
                return null;

            InstanceProperties aInstanceProperties = new InstanceProperties();
            aInstanceProperties.CustomData.Add(Convert.CategoryName, categoryName);

            ModelInstance aModelInstance = new ModelInstance()
            {
                Properties = aInstanceProperties,
                Name = "Solid",
                Location = location
            };
            aModelInstance.CustomData.Add(Convert.CategoryName, categoryName);

            return aModelInstance;
        }

        /***************************************************/
    }
}

