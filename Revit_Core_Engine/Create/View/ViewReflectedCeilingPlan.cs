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
using System;
using System.ComponentModel;
using BH.oM.Base.Attributes;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [ToBeRemoved("6.1", "Calls to this method are to be replaced with Create.ViewPlan with 'viewFamily' parameter equal to ViewFamily.CeilingPlan.")]
        [Description("Creates and returns a new Reflected Ceiling Plan view in the current Revit file.")]
        [Input("document", "Revit current document to be processed.")]
        [Input("level", "The level that the created Floor Plan refers to.")]
        [Input("viewName", "Optional, name of the new view.")]
        [Input("cropBox", "Optional, the crop region to attempt to apply to the newly created view.")]
        [Input("viewTemplateId", "Optional, the View Template Id to be applied in the view.")]
        [Input("viewDetailLevel", "Optional, the Detail Level of the view.")]
        [Output("viewReflectedCeilingPlan", "The new view.")]
        public static View ViewReflectedCeilingPlan(this Document document, Level level, string viewName = null, CurveLoop cropBox = null, ElementId viewTemplateId = null, ViewDetailLevel viewDetailLevel = ViewDetailLevel.Coarse)
        {
            View newView = null;

            ViewFamilyType viewFamilyType = Query.ViewFamilyType(document, ViewFamily.CeilingPlan);

            newView = Autodesk.Revit.DB.ViewPlan.Create(document, viewFamilyType.Id, level.Id);

            Modify.SetViewDetailLevel(newView, viewDetailLevel);

            if (cropBox != null)
            {
                try
                {
                    ViewCropRegionShapeManager vcrShapeMgr = newView.GetCropRegionShapeManager();
                    newView.CropBoxVisible = true;
                    vcrShapeMgr.SetCropShape(cropBox);
                }
                catch (Exception)
                {
                    BH.Engine.Base.Compute.RecordWarning("Could not create the Reflected Ceiling Plan with the provided crop box. Check if the crop box is a valid geometry.");
                }
            }

            newView.SetViewTemplate(viewTemplateId);

            if (!string.IsNullOrEmpty(viewName))
            {
                newView.SetViewName(viewName);
            }

            return newView;
        }

        /***************************************************/
    }
}




