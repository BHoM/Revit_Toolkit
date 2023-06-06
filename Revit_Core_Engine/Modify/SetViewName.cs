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
using BH.oM.Base;
using System;
using System.ComponentModel;
using BH.oM.Base.Attributes;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [PreviousVersion("6.2", "BH.Revit.Engine.Core.Modify.SetViewName(Autodesk.Revit.DB.View, System.String, Autodesk.Revit.DB.Document)")]
        [Description("Set View Name to the given value. If the view name already exists in the model, a number suffix is added.")]
        [Input("view", "The View for which the name will be changed.")]
        [Input("viewName", "New name of the view.")]
        public static void SetViewName(this View view, string viewName)
        {
            var document = view.Document;
            int number = 0;
            string uniqueName = viewName;

            while (uniqueName.IsExistingViewName(document))
            {
                number++;
                uniqueName = $"{viewName} ({number})";
            }

#if (REVIT2018 || REVIT2019)
                    view.ViewName = uniqueName;
#else
            view.Name = uniqueName;
#endif
            if (uniqueName != viewName)
            {
                BH.Engine.Base.Compute.RecordWarning($"There is already a view named '{viewName}'. It has been named '{uniqueName}' instead.");
            }
        }

        /***************************************************/
    }
}