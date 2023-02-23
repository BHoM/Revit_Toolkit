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
using Autodesk.Revit.UI;
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Compute
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Places view on sheet at specified position and rotation using the specified viewport type.")]
        [Input("document", "The current Revit document to be processed.")]
        [Input("sheet", "Sheet to place the view on.")]
        [Input("view", "View to be placed on sheet.")]
        [Input("viewportTypeId", "Id of the viewport to be used.")]
        [Input("viewportPlacementPoint", "Placement point of the viewport on the sheet.")]
        [Input("viewportRotation", "Rotation type of the viewport. The default is None.")]
        [Output("viewPort", "The new viewport.")]
        public static Viewport PlaceViewOnSheet (this Document document, ViewSheet sheet, View view, ElementId viewportTypeId, XYZ viewportPlacementPoint = null, ViewportRotation viewportRotation = ViewportRotation.None)
        {
            Viewport viewPort = Viewport.Create(document, sheet.Id, view.Id, viewportPlacementPoint);

            if (viewPort == null)
            {
                BH.Engine.Base.Compute.RecordWarning($"Could not create Viewport of the view '{view.Name}'. Please check if view isn't empty.");
                return viewPort;
            }

            viewPort.ChangeTypeId(viewportTypeId);
            viewPort.Rotation = viewportRotation;

            document.Regenerate();

            if (viewportPlacementPoint != null)
                viewPort.SetBoxCenter(viewportPlacementPoint);

            return viewPort;
        }

        /***************************************************/

    }
}

