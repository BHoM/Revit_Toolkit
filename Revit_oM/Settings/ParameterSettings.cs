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

using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Base;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.oM.Adapters.Revit.Settings
{
    [Description("An entity holding information about conversion-specific Revit parameter names as well as relationships between type property names (or CustomData keys) and Revit parameter names.")]
    public class ParameterSettings : BHoMObject
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        [Description("A collection of entities defining relationships between property names of BHoM types (or CustomData keys) and parameter names of correspondent Revit elements.")]
        public virtual List<ParameterMap> ParameterMaps { get; set; } = new List<ParameterMap>();

        [Description("Name of the Revit parameter to be used as a source (on Pull) and target (on Push) of information for BHoM tags.")]
        public virtual string TagsParameter { get; set; } = "BHE_Tags";

        [Description("Name of the Revit parameter to be used as a source of information about material grade of a Revit element.")]
        public virtual string MaterialGradeParameter { get; set; } = "BHE_Material Grade";

        /***************************************************/
    }
}

