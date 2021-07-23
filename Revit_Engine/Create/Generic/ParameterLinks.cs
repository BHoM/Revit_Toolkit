/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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

using BH.oM.Adapters.Revit.Mapping;
using BH.oM.Reflection.Attributes;
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

        [Description("Creates an entity defining the relationship between property name of an object (or name of a RevitParameter attached to it) and sets of their correspondent Revit element parameter names.")]
        [InputFromProperty("propertyName")]
        [InputFromProperty("parameterNames")]
        [Output("parameterLink")]
        public static ElementParameterLink ElementParameterLink(string propertyName, IEnumerable<string> parameterNames)
        {
            return new ElementParameterLink { PropertyName = propertyName, ParameterNames = new HashSet<string>(parameterNames) };
        }

        /***************************************************/

        [Description("Creates an entity defining the relationship between property name of an object (or name of a RevitParameter attached to it) and sets of their correspondent Revit element type parameter names.")]
        [InputFromProperty("propertyName")]
        [InputFromProperty("parameterNames")]
        [Output("parameterLink")]
        public static ElementTypeParameterLink ElementTypeParameterLink(string propertyName, IEnumerable<string> parameterNames)
        {
            return new ElementTypeParameterLink { PropertyName = propertyName, ParameterNames = new HashSet<string>(parameterNames) };
        }

        /***************************************************/

        [Description("Creates a collection of entities defining the relationship between property name of an object (or name of a RevitParameter attached to it) and sets of their correspondent Revit parameter names. If parameter name starts with prefix 'Type:', parameter will be sought for in element's type.")]
        [InputFromProperty("Name of the property (or RevitParameter) to be linked with Revit parameters.")]
        [InputFromProperty("A collecation of Revit parameter names to be linked with a given type property. If parameter name starts with prefix 'Type:', parameter will be sought for in element's type (ElementTypeParameterLink will be created).")]
        [Output("parameterLinks")]
        public static IEnumerable<IParameterLink> ParameterLinks(string propertyName, IEnumerable<string> parameterNames)
        {
            List<IParameterLink> result = new List<IParameterLink>();

            List<string> elementTypeParameterNames = parameterNames.Where(x => x.Replace(" ", "").ToLower().StartsWith("type:")).ToList();
            List<string> elementParameterNames = parameterNames.Except(elementTypeParameterNames).ToList();

            if (elementParameterNames.Count != 0)
                result.Add(new ElementParameterLink { PropertyName = propertyName, ParameterNames = new HashSet<string>(elementParameterNames) });

            if (elementTypeParameterNames.Count != 0)
                result.Add(new ElementTypeParameterLink { PropertyName = propertyName, ParameterNames = new HashSet<string>(elementTypeParameterNames.Select(x => x.Split(new[] { ':' }, 2)[1].TrimStart())) });

            return result;
        }

        /***************************************************/
    }
}


