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
        [Output("viewPlan", "The new view.")]        
        public static View ViewPlan(this Document document, Autodesk.Revit.DB.Level level, string viewName = null, CurveLoop cropBox = null, ElementId viewTemplateId = null, ViewDetailLevel viewDetailLevel = ViewDetailLevel.Coarse)
        {
            View result = null;

            ViewFamilyType vft = Query.ViewFamilyType(document, ViewFamily.FloorPlan);

            result = Autodesk.Revit.DB.ViewPlan.Create(document, vft.Id, level.Id);
            
            Modify.SetViewDetailLevel(result, viewDetailLevel);
            
            if (cropBox != null)
            {
                try
                {
                    ViewCropRegionShapeManager vcrShapeMgr = result.GetCropRegionShapeManager();
                    result.CropBoxVisible = true;
                    vcrShapeMgr.SetCropShape(cropBox);
                }
                catch (Exception)
                {
                    BH.Engine.Base.Compute.RecordWarning("Could not create the Floor Plan with the provided crop box. Check if the crop box is a valid geometry and if the view's designated template accepts it to change.");
                }
            }

            if (viewTemplateId != null)
            {
                try
                {
                    result.ViewTemplateId = viewTemplateId;
                }
                catch (Exception)
                {
                    BH.Engine.Base.Compute.RecordWarning("Could not apply the View Template of Id " + viewTemplateId + "'." + ". Please check if it's a valid ElementId.");
                }
            }
            
            if (!string.IsNullOrEmpty(viewName))
            {
                try
                {
#if (REVIT2018 || REVIT2019)
                    result.ViewName = viewName;
#else
                    result.Name = viewName;
#endif
                }
                catch
                {
                    BH.Engine.Base.Compute.RecordWarning("There is already a view named '" + viewName + "'." + " It has been named '" + result.Name + "' instead.");
                }
            }
            
            return result;
        }

        /***************************************************/

        [Description("Creates and returns a new Floor Plan view in the current Revit file.")]
        [Input("document", "Revit current document to be processed.")]
        [Input("level", "The level that the created Floor Plan refers to.")]
        [Input("viewName", "Optional, name of the new view.")]
        [Input("scopeBox", "Optional, the Scope Box to attempt to apply to the newly created view.")]
        [Input("viewTemplateId", "Optional, the View Template Id to be applied in the view.")]
        [Input("cropRegionVisible", "True if enable crop region visibility.")]
        [Input("annotationCrop", "True if enable annotation crop visibility.")]
        [Output("viewPlan", "The new view.")]
        public static View ViewPlan(this Document document, string viewName, Level level, ElementId scopeBoxId = null, ElementId viewTemplateId = null, bool cropRegionVisible = false, bool annotationCrop = false)
        {
            View result = null;

            ViewFamilyType vft = Query.ViewFamilyType(document, ViewFamily.FloorPlan);

            result = Autodesk.Revit.DB.ViewPlan.Create(document, vft.Id, level.Id);

            if (viewTemplateId != null)
            {
                try
                {
                    result.ViewTemplateId = viewTemplateId;
                }
                catch (Exception)
                {
                    BH.Engine.Base.Compute.RecordWarning("Could not apply the View Template of Id " + viewTemplateId + "'." + ". Please check if it's a valid ElementId.");
                }
            }

            if (!string.IsNullOrEmpty(viewName))
            {
                try
                {
#if (REVIT2018 || REVIT2019)
                    result.ViewName = viewName;
#else
                    result.Name = viewName;
#endif
                }
                catch
                {
                    BH.Engine.Base.Compute.RecordWarning("There is already a view named '" + viewName + "'." + " It has been named '" + result.Name + "' instead.");
                }
            }

            if (scopeBoxId != null)
            {
                try
                {
                    result.get_Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP).Set(scopeBoxId);
                }
                catch (Exception)
                {
                    BH.Engine.Base.Compute.RecordWarning("Could not apply the Scope Box of Id " + scopeBoxId + "'." + ". Please check if it's a valid ElementId.");
                }
                
            }

            if (cropRegionVisible)
                result.get_Parameter(BuiltInParameter.VIEWER_CROP_REGION_VISIBLE).Set(1);
            else
                result.get_Parameter(BuiltInParameter.VIEWER_CROP_REGION_VISIBLE).Set(0);

            if (annotationCrop)
                result.get_Parameter(BuiltInParameter.VIEWER_ANNOTATION_CROP_ACTIVE).Set(1);
            else
                result.get_Parameter(BuiltInParameter.VIEWER_ANNOTATION_CROP_ACTIVE).Set(0);

            return result;
        }
    }
}



