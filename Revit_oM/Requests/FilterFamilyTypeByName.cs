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

using BH.oM.Data.Requests;
using System.ComponentModel;

namespace BH.oM.Adapters.Revit.Requests
{
    [Description("IRequest that filters Revit family types by names of theirs and their parent family, with option to loose the search by leaving one or both of the input names blank.")]
    public class FilterFamilyTypeByName : IRequest
    {
        /***************************************************/
        /****                Properties                 ****/
        /***************************************************/

        [Description("Name of Revit family as shown in Revit UI. Optional: if left blank, all families will be filtered in search for specified family type name.")]
        public virtual string FamilyName { get; set; } = "";

        [Description("Revit family type name matching one displayed in Revit UI. Optional: if left blank, all types within family will be returned. If family name is left blank too, all family types in model will be returned.")]
        public virtual string FamilyTypeName { get; set; } = "";

        [Description("If true: only perfect, case sensitive text match will be accepted. If false: capitals and small letters will be treated as equal.")]
        public virtual bool CaseSensitive { get; set; } = true;

        /***************************************************/
    }
}
