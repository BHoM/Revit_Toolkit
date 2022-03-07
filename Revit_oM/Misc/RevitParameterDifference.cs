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

using BH.oM.Diffing;
using System.ComponentModel;

namespace BH.oM.Adapters.Revit
{
    [Description("Represents a difference in terms of RevitParameters found between a Previous and Following version of an object.")]
    public class RevitParameterDifference : IPropertyDifference
    {
        [Description("The name of the Parameter that is different.")]
        public virtual string Name { get; set; }

        [Description("A human-friendly description associated with this difference.")]
        public virtual string Description { get; set; }

        [Description("The older value of this RevitParameter (associated with the past version of the object).")]
        public virtual object PastValue { get; set; }

        [Description("The newer value of this RevitParameter (associated with the following version of the object).")]
        public virtual object FollowingValue { get; set; }

        [Description("Full Name of the object's property that owns the RevitParameter that was different.")]
        public virtual string FullName { get; set; }

        [Description("Whether the RevitParameter that is Different was Modified, Removed or Added with respect to the old version of the owner object. Useful for filtering.")]
        public virtual DifferenceType DifferenceType { get; set; }
    }
}



