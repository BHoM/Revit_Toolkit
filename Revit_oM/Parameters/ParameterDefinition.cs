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
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.oM.Adapters.Revit.Parameters
{
    [Description("An object representing the definition of a Revit parameter (project parameter or shared parameter as such, not its value related to certain element or type).")]
    public class ParameterDefinition : BHoMObject
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        [Description("Parameter group, to which the Revit parameter represented by this object belongs. One of the dropdown values for project parameters, any value for shared parameter.")]
        public virtual string ParameterGroup { get; set; } = "";

        [Description("Parameter type of the Revit parameter represented by this object.")]
        public virtual string ParameterType { get; set; } = "";

        [Description("Discipline, to which the Revit parameter represented by this object belongs. Only relevant in case if there is more than one types with the same name (e.g. piping, HVAC and structural Velocity).\n" +
                     "Six sensible values can be applied: Common, Electrical, Energy, HVAC, Piping and Structural.")]
        public virtual string Discipline { get; set; } = "";

        [Description("If true, the Revit parameter represented by this object is an instance parameter, otherwise it is a type parameter.")]
        public virtual bool Instance { get; set; } = true;

        [Description("Categories, to which the Revit parameter represented by this object is bound. On Push, it will get bound to all categories if this value is null.")]
        public virtual List<string> Categories { get; set; } = null;

        [Description("If true, the Revit parameter represented by this object is a shared parameter, otherwise it is a project parameter.")]
        public virtual bool Shared { get; set; } = false;

        /***************************************************/
    }
}
