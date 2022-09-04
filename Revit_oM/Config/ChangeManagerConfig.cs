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

using Autodesk.Revit.DB;
using BH.oM.Adapter;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Base;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.oM.Adapters.Revit
{
    [Description("Configuration used to specify how a change manager should inspect that objects passed into it.")]
    public class ChangeManagerConfig : IObject
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        [Description("Categories of which the change manager should use in the change comparison.")]
        public virtual List<BuiltInCategory> Categories { get; set; } = null;

        [Description("Properties of which the change manager should use in the change comparison.")]
        public virtual List<string> Properties { get; set; } = null;

        [Description("If true, change manager will compare only existing element ids against new element ids. ")]
        public virtual bool IsAdditions { get; set; }

        [Description("If true, change manager will compare only existing element ids against new element ids. ")]
        public virtual bool IsDeletions { get; set; }

        [Description("If true, change manager will compare element properties against elements.")]
        public virtual bool IsModifications { get; set; }

        /***************************************************/
    }
}


