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

using Autodesk.Revit.DB;
using BH.oM.Base;
using System;
using System.ComponentModel;
using BH.oM.Reflection.Attributes;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Attempts to modify a view's Detail Level while checking if it's controlled by a view template.")]
        [Input("view", "The view to change the Detail Level.")]
        [Input("viewDetailLevel", "The Detail Level to change in the view.")]
        [Output("setViewDetailLevel", "The modified view.")] 
        public static void SetViewDetailLevel(this View view, ViewDetailLevel viewDetailLevel)
        {
            if (view.ViewTemplateId != ElementId.InvalidElementId)
            {
                View viewTemplate = view.Document.GetElement(view.ViewTemplateId) as View;
                Parameter detailLevelParameter = viewTemplate.get_Parameter(BuiltInParameter.VIEW_DETAIL_LEVEL);
                if(detailLevelParameter.UserModifiable)
                    view.DetailLevel = viewDetailLevel;
                else
                    BH.Engine.Reflection.Compute.RecordWarning("Could not change the view detail level because it's being controlled by the view template.");
            }
            else
            {
                view.DetailLevel = viewDetailLevel;
            }
        }

        /***************************************************/
    }
}

