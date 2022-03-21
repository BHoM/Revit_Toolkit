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

        [Description("Attempts to modify a view's scale while checking if it's controlled by a view template.")]
        [Input("view", "The view to change the Detail Level.")]
        [Input("scale", "The scale to set the view to, in format 1:value (1:100 should take input equal to 100).")]
        [Output("success", "True if the scale has been successfully set to the view.")]
        public static bool SetViewScale(this View view, int scale)
        {
            if (view == null)
            {
                BH.Engine.Base.Compute.RecordError("Cannot set scale to a null view.");
                return false;
            }

            if (scale <= 0)
            {
                BH.Engine.Base.Compute.RecordWarning("Scale of a view can only be set to positive values.");
                return false;
            }

            Parameter scaleParameter = view.get_Parameter(BuiltInParameter.VIEW_SCALE_PULLDOWN_METRIC);
            if (scaleParameter == null)
            {
                BH.Engine.Base.Compute.RecordWarning("Scale of a view could not be set because the relevant parameter could not be found.");
                return false;
            }

            if (scaleParameter.IsReadOnly)
            {
                BH.Engine.Base.Compute.RecordWarning("Scale of a view could not be set because the relevant parameter is read only (possibly because it is controlled by the template).");
                return false;
            }

            view.Scale = scale;
            return true;
        }

        /***************************************************/
    }
}


