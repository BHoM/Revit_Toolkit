/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Creates GenericObject by given Point, Family Name and Family Type Name. GenericObject represents generic 3D elements which have not been defined in BHoM structure")]
        [Input("point", "Location Point of Object in 3D space")]
        [Input("familyName", "Revit Family Name")]
        [Input("familyTypeName", "Revit Family Type Name")]
        [Output("GenericObject")]
        public static GenericObject GenericObject(Point point, string familyName, string familyTypeName)
        {
            GenericObject aGenericObject = new GenericObject()
            {
                Name = familyTypeName,
                Location = point
            };

            aGenericObject.CustomData.Add(Convert.FamilyName, familyName);
            aGenericObject.CustomData.Add(Convert.FamilyTypeName, familyTypeName);

            return aGenericObject;
        }

        /***************************************************/

        [Description("Creates GenericObject by given Point, Family Name and Family Type Name. GenericObject represents generic 3D elements which have not been defined in BHoM structure")]
        [Input("curve", "Location Curve of Object in 3D space")]
        [Input("familyName", "Revit Family Name")]
        [Input("familyTypeName", "Revit Family Type Name")]
        [Output("GenericObject")]
        public static GenericObject GenericObject(ICurve curve, string familyName, string familyTypeName)
        {
            GenericObject aGenericObject = new GenericObject()
            {
                Name = familyTypeName,
                Location = curve
            };

            aGenericObject.CustomData.Add(Convert.FamilyName, familyName);
            aGenericObject.CustomData.Add(Convert.FamilyTypeName, familyTypeName);

            return aGenericObject;
        }

        /***************************************************/
    }
}

