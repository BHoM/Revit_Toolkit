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

using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.oM.Adapters.Revit.Parameters
{
    [Description("An entity defining the relationship between property names of a type (or CustomData keys) and sets of their correspondent Revit element parameter names.")]
    public class ElementParameterLink : IParameterLink
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        [Description("Name of the property (or CustomData key) to be linked with Revit parameters.")]
        public virtual string PropertyName { get; set; } = "";

        [Description("A collecation of Revit element parameter names to be linked with the type property.")]
        public virtual HashSet<string> ParameterNames { get; set; } = new HashSet<string>();

        /***************************************************/
    }
}

