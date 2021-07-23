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

using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.oM.Adapters.Revit.Mapping
{
    [Description("An entity defining the relationship between object's property names (or names of RevitParameters attached to it) and parameter names of correspondent Revit elements.")]
    public class ParameterMap : BHoMObject
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        [Description("Type, which property names (or names of RevitParameters) are being mapped with Revit element parameters.")]
        public virtual Type Type { get; set; } = null;

        [Description("A collection of type property names (or names of RevitParameters) and sets of their correspondent Revit parameter names.")]
        public virtual List<IParameterLink> ParameterLinks { get; set; } = new List<IParameterLink>();

        /***************************************************/
    }
}


