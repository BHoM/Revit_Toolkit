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

using BH.oM.Reflection.Attributes;
using BH.oM.Adapters.Revit.Properties;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates InstanceProperties")]
        [Input("familyName", "Revit Family Name")]
        [Input("familyTypeName", "Revit Family Type Name")]
        [Output("InstanceProperties")]
        public static InstanceProperties InstanceProperties(string familyName, string familyTypeName)
        {
            InstanceProperties instanceProperties = new InstanceProperties()
            {
                Name = Query.FamilyTypeFullName(familyName, familyTypeName),
            };

            instanceProperties.CustomData.Add(Convert.FamilyName, familyName);
            instanceProperties.CustomData.Add(Convert.FamilyTypeName, familyTypeName);

            return instanceProperties;
        }

        /***************************************************/

        [Description("Creates InstanceProperties")]
        [Input("name", "name")]
        [Output("InstanceProperties")]
        public static InstanceProperties InstanceProperties(string name)
        {
            InstanceProperties instanceProperties = new InstanceProperties()
            {
                Name = name,
            };

            return instanceProperties;
        }

        /***************************************************/
    }
}

