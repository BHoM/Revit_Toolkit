/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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

using BH.oM.Adapters.Revit.Properties;
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates an object that carries information about correspondent Revit family type.")]
        [Input("familyName", "Name of the correspondent Revit family.")]
        [Input("familyTypeName", "Name of the correspondent Revit family type.")]
        [Output("instanceProperties")]
        public static InstanceProperties InstanceProperties(string familyName, string familyTypeName)
        {
            return new InstanceProperties { Name = Query.FamilyTypeFullName(familyName, familyTypeName) };
        }

        /***************************************************/

        [Description("Creates an object that carries information about correspondent Revit family type.")]
        [Input("name", "Name of the correspondent Revit family type in format FamilyName: FamilyTypeName.")]
        [Output("instanceProperties")]
        public static InstanceProperties InstanceProperties(string name)
        {
            return new InstanceProperties { Name = name };
        }

        /***************************************************/
    }
}




