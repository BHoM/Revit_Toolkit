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

using BH.oM.Data.Requests;
using System.ComponentModel;

namespace BH.oM.Adapters.Revit.Requests
{
    [Description("IRequest that filters all link instances with given file name/path, Name parameter value or ElementId from active Revit document. If link name field is left blank, all link instances in the document will be filtered.")]
    public class FilterLinkInstance : IRequest
    {
        /***************************************************/
        /****                Properties                 ****/
        /***************************************************/
        
        [Description("Name of the link file (alternatively whole file path, Name parameter value or ElementId) used to filter the Revit link instances. Optional: if left blank, all link instances in the document will be filtered.")]
        public virtual string LinkName { get; set; } = "";

        [Description("If true: only perfect, case sensitive text match will be accepted. If false: capitals and small letters will be treated as equal.")]
        public virtual bool CaseSensitive { get; set; } = true;

        /***************************************************/
    }
}

