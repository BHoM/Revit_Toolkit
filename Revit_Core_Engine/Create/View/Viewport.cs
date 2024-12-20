/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Places view on sheet at specified position and rotation using the specified viewport type.")]
        [Input("sheet", "Sheet to place the view on.")]
        [Input("view", "View to be placed on sheet.")]
        [Input("viewportTypeId", "Id of the viewport type to be used.")]
        [Input("viewportPlacementPoint", "Placement point of the viewport on the sheet.")]
        [Input("viewportRotation", "Rotation type of the viewport. The default is None.")]
        [Output("viewPort", "The new viewport.")]
        public static Viewport Viewport(this ViewSheet sheet, View view, ElementId viewportTypeId, XYZ viewportPlacementPoint = null, ViewportRotation viewportRotation = ViewportRotation.None)
        {
            Document document = sheet.Document;

            Viewport viewPort = Autodesk.Revit.DB.Viewport.Create(document, sheet.Id, view.Id, new XYZ());

            if (viewPort == null)
            {
                BH.Engine.Base.Compute.RecordError($"Could not create Viewport of the view '{view.Name}'. Please check if view isn't empty.");
                return viewPort;
            }

            viewPort.ChangeTypeId(viewportTypeId);
            viewPort.Rotation = viewportRotation;

            if (viewportPlacementPoint != null)
            {
                document.Regenerate();
                viewPort.SetBoxCenter(viewportPlacementPoint);
            }
            else
            {
                BH.Engine.Base.Compute.RecordNote("Viewport placement point not defined - it has been placed in default position");
            }

            return viewPort;
        }

        /***************************************************/

    }
}



