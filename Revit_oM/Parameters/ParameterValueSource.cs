/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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

using BH.oM.Verification.Conditions;
using System.ComponentModel;

namespace BH.oM.Revit.Parameters
{
    //TODO: add ConvertUnits prop to reflect the behaviour of FilterByParameterNumber? could actually be welcome, potentially helpful in other contexts as well
    [Description("Object pointing at a Revit parameter as the source of a value to extract.")]
    public class ParameterValueSource : IValueSource
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        [Description("Name of the parameter to extract the value from.")]
        public virtual string ParameterName { get; set; } = "";

        [Description("If true, the value to be extracted from the underlying type, not the instance.")]
        public virtual bool FromType { get; set; } = false;

        /***************************************************/
    }
}
