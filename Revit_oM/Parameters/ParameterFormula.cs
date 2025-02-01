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

using BH.oM.Base;
using System.ComponentModel;
using System.Collections.Generic;

namespace BH.oM.Adapters.Revit.Parameters
{
    [Description("A BHoMObject contains a formula to establish relationship with other parameters of a Revit Instance object")]
    public class ParameterFormula : BHoMObject
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/
        [Description("List of RevitParameters involved in the formula")]
        public virtual List<RevitParameter> InputParameters{ get; set; } = null;

        [Description("Formula string. RevitParameters' names are used as variables. using C# syntax, can use all methods from System.Math, System.Enumerable type, Ex: Pow(p1,2); Join(p1,p2,p3), ")]
        public virtual string Formula { get; set; } = string.Empty;

        [Description("Return type of the formula")]
        public virtual string ReturnType { get; set; }

        [Description("Custom additional data, Ex: List<List<object>> object represent a lookup table")]
        public override Dictionary<string,object> CustomData { get; set; } = null;

        /***************************************************/
    }
}







