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

using BH.oM.Adapter;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.oM.Adapters.Revit
{
    [Description("Configuration used for adapter interaction with Revit on Push action.")]
    public class RevitPushConfig: ActionConfig
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        //[Description("Copy BHoM object's CustomData to resultant Revit Element's parameter values if true.")]
        //public bool CopyCustomData { get; set; } = true;

        //[Description("A dictionary of BHoM Guids and Revit ElementIds that represent them - if certain BHoM Guid is found in the keys, Revit Element that carries correspondent ElementId will be returned on push instead of standard convert.")]
        //public Dictionary<Guid, List<int>> RefObjects = null;

        [Description("If true, Revit warnings and failure messages will be suppressed (not shown to the user). Whilst this option may speed the pushing process up in case of multiple warnings, it may lead to important issues.")]
        public bool SuppressFailureMessages { get; set; } = false;


        /***************************************************/
        /****                  Default                  ****/
        /***************************************************/

        [Description("Default config, used if not set by the user.")]
        public static readonly RevitPushConfig Default = new RevitPushConfig();

        /***************************************************/
    }
}
