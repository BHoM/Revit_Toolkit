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
    [Description("An entity defining the relationship relationship between Revit families and BHoM type, to which these families are meant to be converted on Pull.")]
    public class FamilyMap : BHoMObject
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        [Description("BHoM type, to which the Revit families are meant to be converted on Pull.")]
        public virtual Type Type { get; set; } = null;

        [Description("Names of the families to be converted to a given BHoM type on Pull.")]
        public virtual List<string> FamilyNames { get; set; } = new List<string>();

        /***************************************************/
    }
}


