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
using System;
using System.ComponentModel;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.UI;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Creates and returns a new Plan view in the current Revit file.")]
        [Input("document", "Revit current document to be processed.")]
        [Input("level", "The level that the created Floor Plan refers to.")]
        [Input("viewName", "Name of the new view.")]
        [Input("viewFamily", "View family type of the new view.")]
        [Input("cropBox", "Optional, the crop region to attempt to apply to the newly created view.")]
        [Input("viewTemplateId", "Optional, the View Template Id to be applied in the view.")]
        [Input("viewDetailLevel", "Optional, the Detail Level of the view.")]
        [Input("cropRegionVisible", "True if the crop region should be visible.")]
        [Input("annotationCrop", "True if the annotation crop should be visible.")]
        [Output("newView", "The new view.")]
        public static View ViewPlan(this Document document, Level level, string viewName, ViewFamily viewFamily = ViewFamily.FloorPlan, CurveLoop cropBox = null, ElementId viewTemplateId = null, ViewDetailLevel viewDetailLevel = ViewDetailLevel.Coarse, bool cropRegionVisible = false, bool annotationCrop = false)
        {
            View newView = null;

            if (!(viewFamily == ViewFamily.FloorPlan || viewFamily == ViewFamily.CeilingPlan || viewFamily == ViewFamily.AreaPlan || viewFamily == ViewFamily.StructuralPlan))
            {
                BH.Engine.Base.Compute.RecordError($"Could not create View of type '{viewFamily}'. It has to be a FloorPlan, CeilingPlan, AreaPlan, or StructuralPlan ViewType.");
                return newView;
            }

            ViewFamilyType viewFamilyType = Query.ViewFamilyType(document, viewFamily);

            newView = Autodesk.Revit.DB.ViewPlan.Create(document, viewFamilyType.Id, level.Id);
            
            if (viewDetailLevel != ViewDetailLevel.Undefined)
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
                    BH.Engine.Base.Compute.RecordWarning("Could not apply the provided crop box to newly created view. Check if the crop box is a valid geometry.");
                }
            }

            if (viewTemplateId != null)
            {
                if (!(document.GetElement(viewTemplateId) as View).IsTemplate)
                {
                    BH.Engine.Base.Compute.RecordWarning($"Could not apply the View Template of Id '{viewTemplateId}'. Please check if it's a valid View Template.");
                    return newView;
                }

                try
                {
                    newView.ViewTemplateId = viewTemplateId;
                }
                catch (Exception)
                {
                    BH.Engine.Base.Compute.RecordWarning($"Could not apply the View Template of Id '{viewTemplateId}'. Please check if it's a valid ElementId.");
                }
            }
            
            if (!string.IsNullOrEmpty(viewName))
            {
                newView.SetViewName(viewName);
            }

            if (!newView.get_Parameter(BuiltInParameter.VIEWER_CROP_REGION_VISIBLE).Set(cropRegionVisible ? 1 : 0))
                BH.Engine.Base.Compute.RecordWarning($"Could not set the crop region visibility in the view. Parameter is ready-only.");

            if (!newView.get_Parameter(BuiltInParameter.VIEWER_ANNOTATION_CROP_ACTIVE).Set(annotationCrop ? 1 : 0))
                BH.Engine.Base.Compute.RecordWarning($"Could not set the annotation crop in the view. Parameter is ready-only.");

            return newView;
        }

        /***************************************************/

        [Description("Creates and returns a new Plan view in the current Revit file.")]
        [Input("document", "The current Revit document to be processed.")]
        [Input("level", "The level that the created view refers to.")]
        [Input("viewName", "Name of the new view.")]
        [Input("viewFamily", "View Family of the View Type. The default is FloorPlan.")]
        [Input("scopeBoxId", "(Optional) A Scope Box Id to attempt to apply to the newly created view.")]
        [Input("viewTemplateId", "(Optional) A View Template Id to be applied in the view.")]
        [Input("viewDetailLevel", "Optional, the Detail Level of the view.")]
        [Input("cropRegionVisible", "True if the crop region should be visible.")]
        [Input("annotationCrop", "True if the annotation crop should be visible.")]
        [Output("newView", "The new view.")]
        public static View ViewPlan(this Document document, Level level, string viewName, ViewFamily viewFamily = ViewFamily.FloorPlan,  ElementId scopeBoxId = null, ElementId viewTemplateId = null, ViewDetailLevel viewDetailLevel = ViewDetailLevel.Coarse, bool cropRegionVisible = false, bool annotationCrop = false)
        {
            View newView = ViewPlan(document, level, viewName, viewFamily, null as CurveLoop, viewTemplateId, viewDetailLevel, cropRegionVisible, annotationCrop);

            if (scopeBoxId != null)
            {
                Element scopeBox = document.GetElement(scopeBoxId);

                if (!((BuiltInCategory)scopeBox.Category.Id.IntegerValue == BuiltInCategory.OST_VolumeOfInterest))
                {
                    BH.Engine.Base.Compute.RecordWarning($"Could not apply the Scope Box of Id '{scopeBoxId}'. Please check if it's a valid Scope Box element.");
                    return newView;
                }

                try
                {
                    newView.get_Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP).Set(scopeBoxId);
                }
                catch (Exception)
                {
                    BH.Engine.Base.Compute.RecordWarning($"Could not apply the Scope Box of Id '{scopeBoxId}'. Please check if it's a valid ElementId.");
                }
            }

            return newView;
        }

        /***************************************************/
    }
}



