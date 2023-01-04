/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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

using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Diffing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Gets a general representation of a revitParameterDifferenceType: only in terms of `added`, `modified` or `removed`.")]
        [Input("revitParameterDifferenceType", "A type of revitParameterDifference.")]
        [Output("differenceType", "General difference type: added, removed or modified.")]
        public static DifferenceType DifferenceType(this RevitParameterDifferenceType revitParameterDifferenceType)
        {
            if (revitParameterDifferenceType == RevitParameterDifferenceType.AddedAssigned || revitParameterDifferenceType == RevitParameterDifferenceType.AddedUnassigned)
                return oM.Diffing.DifferenceType.Added;

            if (revitParameterDifferenceType == RevitParameterDifferenceType.RemovedAssigned || revitParameterDifferenceType == RevitParameterDifferenceType.RemovedUnassigned)
                return oM.Diffing.DifferenceType.Removed;

            return oM.Diffing.DifferenceType.Modified; 
        }

        /***************************************************/
    }
}




