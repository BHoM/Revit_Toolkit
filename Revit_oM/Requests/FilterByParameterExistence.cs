/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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

using System.ComponentModel;

namespace BH.oM.Adapters.Revit.Requests
{
    [Description("IRequest that filters elements the have (or do not have) a parameter with given name.")]
    public class FilterByParameterExistence : IParameterRequest
    {
        /***************************************************/
        /****                Properties                 ****/
        /***************************************************/

        [Description("Case sensitive name of the parameter to look for.")]
        public virtual string ParameterName { get; set; } = "";

        [Description("If true, elements with the given parameter will be filtered, if false - the opposite.")]
        public virtual bool ParameterExists { get; set; } = false;

        /***************************************************/
    }
}




