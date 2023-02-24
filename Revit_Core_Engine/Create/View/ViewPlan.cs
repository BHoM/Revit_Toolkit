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

        [Description("Creates and returns a new Floor Plan view in the current Revit file.")]
        [Input("document", "Revit current document to be processed.")]
        [Input("level", "The level that the created Floor Plan refers to.")]
        [Input("viewName", "Optional, name of the new view.")]
        [Input("cropBox", "Optional, the crop region to attempt to apply to the newly created view.")]
        [Input("viewTemplateId", "Optional, the View Template Id to be applied in the view.")]
        [Input("viewDetailLevel", "Optional, the Detail Level of the view.")]        
        [Output("newView", "The new view.")]        
        public static View ViewPlan(this Document document, Level level, string viewName = null, CurveLoop cropBox = null, ElementId viewTemplateId = null, ViewDetailLevel viewDetailLevel = ViewDetailLevel.Coarse)
        {
            View newView = null;

            ViewFamilyType viewFamilyType = Query.ViewFamilyType(document, ViewFamily.FloorPlan);

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
                    BH.Engine.Base.Compute.RecordWarning("Could not create the Floor Plan with the provided crop box. Check if the crop box is a valid geometry.");
                }
            }

            if (viewTemplateId != null)
            {
                if (!(document.GetElement(viewTemplateId) as View).IsTemplate)
                {
                    BH.Engine.Base.Compute.RecordWarning("Could not apply the View Template of Id " + viewTemplateId + "'." + ". Please check if it's a valid View Template.");
                    return newView;
                }

                try
                {
                    newView.ViewTemplateId = viewTemplateId;
                }
                catch (Exception)
                {
                    BH.Engine.Base.Compute.RecordWarning("Could not apply the View Template of Id " + viewTemplateId + "'." + ". Please check if it's a valid ElementId.");
                }
            }
            
            if (!string.IsNullOrEmpty(viewName))
            {
                int number = 0;
                string uniqueName = viewName;

                while (uniqueName.IsExistingViewName(document))
                {
                    number++;
                    uniqueName = $"{viewName} ({number})";
                }

#if (REVIT2018 || REVIT2019)
                    newView.ViewName = uniqueName;
#else
                newView.Name = uniqueName;
#endif
                if (uniqueName != viewName)
                {
                    BH.Engine.Base.Compute.RecordWarning($"There is already a view named '{viewName}'. It has been named '{uniqueName}' instead.");
                }
            }
            
            return newView;
        }

        /***************************************************/

        [Description("Creates and returns a new Floor Plan view in the current Revit file.")]
        [Input("document", "The current Revit document to be processed.")]
        [Input("level", "The level that the created Floor Plan refers to.")]
        [Input("viewName", "Name of the new view.")]
        [Input("viewFamily", "View Family of the View Type. The default is FloorPlan.")]
        [Input("scopeBox", "(Optional) A Scope Box to attempt to apply to the newly created view.")]
        [Input("viewTemplateId", "(Optional) A View Template Id to be applied in the view.")]
        [Input("cropRegionVisible", "True if the crop region should be visible.")]
        [Input("annotationCrop", "True if the annotation crop should be visible.")]
        [Output("newView", "The new view.")]
        public static View ViewPlan(this Document document, Level level, string viewName, ViewFamily viewFamily = ViewFamily.FloorPlan,  ElementId scopeBoxId = null, ElementId viewTemplateId = null, bool cropRegionVisible = false, bool annotationCrop = false)
        {
            View newView = null;

            if (!(viewFamily == ViewFamily.FloorPlan || viewFamily == ViewFamily.CeilingPlan || viewFamily == ViewFamily.AreaPlan || viewFamily == ViewFamily.StructuralPlan))
            {
                BH.Engine.Base.Compute.RecordWarning("Could not create View of type " + viewFamily + "'." + ". It has to be a FloorPlan, CeilingPlan, AreaPlan, or StructuralPlan ViewType.");
                return newView;
            }

            ViewFamilyType viewFamilyType = Query.ViewFamilyType(document, viewFamily);

            newView = Autodesk.Revit.DB.ViewPlan.Create(document, viewFamilyType.Id, level.Id);

            if (viewTemplateId != null)
            {
                if (!(document.GetElement(viewTemplateId) as View).IsTemplate)
                {
                    BH.Engine.Base.Compute.RecordWarning("Could not apply the View Template of Id " + viewTemplateId + "'." + ". Please check if it's a valid View Template.");
                    return newView;
                }

                try
                {
                    newView.ViewTemplateId = viewTemplateId;
                }
                catch (Exception)
                {
                    BH.Engine.Base.Compute.RecordWarning("Could not apply the View Template of Id " + viewTemplateId + "'." + ". Please check if it's a valid ElementId.");
                }
            }

            if (!string.IsNullOrEmpty(viewName))
            {
                int number = 0;
                string uniqueName = viewName;

                while (uniqueName.IsExistingViewName(document))
                {
                    number++;
                    uniqueName = $"{viewName} ({number})";
                }

#if (REVIT2018 || REVIT2019)
                    newView.ViewName = uniqueName;
#else
                newView.Name = uniqueName;
#endif
                if (uniqueName != viewName)
                {
                    BH.Engine.Base.Compute.RecordWarning($"There is already a view named '{viewName}'. It has been named '{uniqueName}' instead.");
                }
            }

            if (scopeBoxId != null)
            {
                Element scopeBox = document.GetElement(scopeBoxId);

                if (!((BuiltInCategory)scopeBox.Category.Id.IntegerValue == BuiltInCategory.OST_VolumeOfInterest))
                {
                    BH.Engine.Base.Compute.RecordWarning("Could not apply the Scope Box of Id " + scopeBoxId + "'." + ". Please check if it's a valid Scope Box element.");
                    return newView;
                }

                try
                {
                    newView.get_Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP).Set(scopeBoxId);
                }
                catch (Exception)
                {
                    BH.Engine.Base.Compute.RecordWarning("Could not apply the Scope Box of Id " + scopeBoxId + "'." + ". Please check if it's a valid ElementId.");
                }
            }

            newView.get_Parameter(BuiltInParameter.VIEWER_CROP_REGION_VISIBLE).Set(cropRegionVisible? 1: 0);
            newView.get_Parameter(BuiltInParameter.VIEWER_ANNOTATION_CROP_ACTIVE).Set(annotationCrop? 1: 0);

            return newView;
        }

        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        [Description("Check if given view name exists in the Revit model.")]
        private static bool IsExistingViewName(this string viewName, Document document)
        {
            List<string> viewNamesInModel = new FilteredElementCollector(document).OfClass(typeof(ViewPlan)).WhereElementIsNotElementType().Select(x => x.Name).ToList();
            
            return viewNamesInModel.Contains(viewName);
        }

        /***************************************************/

    }
}



