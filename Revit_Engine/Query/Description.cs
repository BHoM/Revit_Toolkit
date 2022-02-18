﻿/*
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

using BH.oM.Base.Attributes;
using BH.oM.Diffing;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns a human-readable description of a given property difference resulting from a Revit diffing workflow.")]
        [Input("difference", "Property difference to be processed.")]
        [Output("description", "Human-readable description of the input property difference resulting from a Revit diffing workflow.")]
        public static string Description(this PropertyDifference difference)
        {
            string propertyLabel;
            if (difference.DisplayName.EndsWith("(RevitParameter)"))
                propertyLabel = difference.DisplayName.Replace("(RevitParameter)", "(Revit Parameter)");
            else
                propertyLabel = difference.DisplayName + " (BHoM Property)";

            return $"{propertyLabel}: from " + (string.IsNullOrWhiteSpace(difference?.PastValue?.ToString()) ? "<empty>" : difference.PastValue) + " to " + (string.IsNullOrWhiteSpace(difference?.FollowingValue?.ToString()) ? "<empty>" : difference.FollowingValue);
        }

        /***************************************************/
    }
}