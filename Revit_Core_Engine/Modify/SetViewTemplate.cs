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

        [Description("Set View Template to the given view.")]
        [Input("view", "The View for which the View Template will be set.")]
        [Input("viewTemplateId", "Id of the View Template to be set.")]
        public static void SetViewTemplate(this View view, ElementId viewTemplateId)
        {
            Document doc = view.Document;

            if (viewTemplateId != null && viewTemplateId.IntegerValue != -1)
            {
                if ((doc.GetElement(viewTemplateId) as View)?.IsTemplate != true)
                {
                    BH.Engine.Base.Compute.RecordWarning($"Could not apply the View Template of Id '{viewTemplateId}'. Please check if it's a valid View Template.");
                    return;
                }

                try
                {
                    view.ViewTemplateId = viewTemplateId;
                }
                catch (Exception)
                {
                    BH.Engine.Base.Compute.RecordWarning($"Could not apply the View Template of Id '{viewTemplateId}'. Please check if view template is applicable to this view.");
                }
            }
        }

        /***************************************************/
    }
}