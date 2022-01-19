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

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Revit.Adapter.Core
{
    [Description("Class indicating that a button can be clicked in any UI mode of Revit.")]
    public class AlwaysAvailable : IExternalCommandAvailability
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Check if the button is can be clicked in the given state of the app.")]
        [Input("applicationData", "Revit application to be queried.")]
        [Input("selectedCategories", "Categories of elements currently selected in the application.")]
        [Output("available", "True if the button can be clicked in the given state of the app. Always true in this case.")]
        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            return true;
        }

        /***************************************************/
    }
}
