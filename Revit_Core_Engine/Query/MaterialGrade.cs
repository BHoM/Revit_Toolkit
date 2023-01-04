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

using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Extracts the material grade information from a Revit element. The grade is extracted from a Revit parameter under the name specified in the MappingSettings.")]
        [Input("element", "Revit Element to be queried.")]
        [Input("settings", "Revit adapter settings containing the information about the name of the parameter that stores the material grade information.")]
        [Output("grade", "Material grade extracted from the Revit element.")]
        public static string MaterialGrade(this Element element, RevitSettings settings)
        {
            if (element == null)
                return null;

            string materialGrade = element.LookupParameterString(settings.MappingSettings.MaterialGradeParameter);
            if (materialGrade != null)
                materialGrade = materialGrade.Replace(" ", "");

            return materialGrade;
        }

        /***************************************************/
    }
}



