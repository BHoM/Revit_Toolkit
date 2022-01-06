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

using BH.oM.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace BH.oM.Adapters.Revit
{
    [Description("Wrapper for Revit family file (.rfa) that stores basic information about it such as family category, familiy type names etc.")]
    public class RevitFilePreview : BHoMObject, IImmutable
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        [Description("Path to the Revit family file wrapped by this object.")]
        public virtual string Path { get; } = string.Empty;

        [Description("Name of the Revit category, to which belongs the family wrapped by this object.")]
        public virtual string CategoryName { get; } = string.Empty;

        [Description("Name of the Revit family wrapped by this object.")]
        public virtual string FamilyName { get; } = string.Empty;

        [Description("Names of the family types contained within the Revit family wrapped by this object.")]
        public virtual ReadOnlyCollection<string> FamilyTypeNames { get; } = null;
        

        /***************************************************/
        /****            Public Constructors            ****/
        /***************************************************/

        public RevitFilePreview(string path, string categoryName, string familyName, IEnumerable<string> familyTypeNames)
        {
            Path = path;
            CategoryName = categoryName;
            FamilyName = familyName;
            FamilyTypeNames = new ReadOnlyCollection<string>(familyTypeNames.ToList());
        }

        /***************************************************/
    }
}




