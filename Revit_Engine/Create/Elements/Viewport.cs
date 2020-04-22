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
using BH.oM.Geometry;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates BHoM Viewport object in specific point location, linked to the view by given name and to the sheet by given sheet Number.")]
        [Input("sheetNumber", "Revit sheet number linked with created Viewport")]
        [Input("viewName", "Revit view name linked with created Viewport")]
        [InputFromProperty("location")]
        [Output("viewport")]
        public static Viewport Viewport(string sheetNumber, string viewName, Point location)
        {
            Viewport viewport = new Viewport()
            {
                Location = location
            };

            viewport.CustomData.Add("Sheet Number", sheetNumber);
            viewport.CustomData.Add("View Name", viewName);

            viewport.CustomData.Add(RevitAdapter.FamilyName, "Viewport");
            viewport.CustomData.Add(RevitAdapter.CategoryName, "Viewports");

            return viewport;
        }

        /***************************************************/
    }
}


