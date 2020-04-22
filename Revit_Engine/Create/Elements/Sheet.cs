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

using BH.Adapter.Revit;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates BHoM Sheet object.")]
        [Input("name", "Name of the created Sheet correspondent with Revit sheet name.")]
        [Input("number", "Number of the created Sheet correspondent with Revit sheet number.")]
        [Output("sheet")]
        public static Sheet Sheet(string name, string number)
        {
            Sheet sheet = new Sheet()
            {
                Name = name
            };

            sheet.CustomData.Add("Sheet Name", name);
            sheet.CustomData.Add("Sheet Number", number);

            sheet.CustomData.Add(RevitAdapter.FamilyName, "Sheet");
            sheet.CustomData.Add(RevitAdapter.FamilyTypeName, "Sheet");
            sheet.CustomData.Add(RevitAdapter.CategoryName, "Sheets");

            return sheet;
        }

        /***************************************************/
    }
}


