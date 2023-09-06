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
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Creates and returns a new Section view in the current Revit file.")]
        [Input("document", "Revit current UI document to be processed.")]
        [Input("boundingBoxXyz", "The BoundingBoxXYZ to fit the section to.")]
        [Input("viewName", "Name of the new view.")]
        [Input("viewFamilyType", "View type to be applied on view creation. If left empty, a default section view type will be used.")]
        [Input("viewTemplateId", "View Template Id to be applied in the view. If left empty, default template defined in view type will be applied.")]
        [Input("viewScale", "Scale of the view in format 1:value (1:100 should take input equal to 100). Only applicable if the template does not own the parameter.")]
        [Input("viewDetailLevel", "Detail Level of the view. Only applicable if the template does not own the parameter.")]
        [Output("viewSection", "Section view created based on the inputs.")]
        public static ViewSection ViewSection(this Document document, BoundingBoxXYZ boundingBoxXyz, string viewName = null, ViewFamilyType viewFamilyType = null, ElementId viewTemplateId = null, int viewScale = 0, ViewDetailLevel viewDetailLevel = ViewDetailLevel.Undefined)
        {
            if (document == null)
            {
                BH.Engine.Base.Compute.RecordError("Could not create a section view in a null document.");
                return null;
            }

            if (boundingBoxXyz == null)
            {
                BH.Engine.Base.Compute.RecordError("Could not create a section view based on a null box.");
                return null;
            }

            if (viewFamilyType == null)
                viewFamilyType = Query.ViewFamilyType(document, ViewFamily.Section);

            if (viewFamilyType == null)
            {
                BH.Engine.Base.Compute.RecordError("Section view creation failed: no valid view type could not be found to create it.");
                return null;
            }

            ViewSection result = Autodesk.Revit.DB.ViewSection.CreateSection(document, viewFamilyType.Id, boundingBoxXyz);

            if (viewScale != 0)
                result.SetViewScale(viewScale);

            result.SetViewDetailLevel(viewDetailLevel);
            result.SetViewTemplate(viewTemplateId);

            if (!string.IsNullOrEmpty(viewName))
            {
                result.SetViewName(viewName);
            }

            // In case of non-vertical section, sometimes Revit rotates the views by 90 degrees compared to the provided input
            // The lines below bring the view orientation back to the desirable shape
            if (1 - Math.Abs(boundingBoxXyz.Transform.BasisY.DotProduct(XYZ.BasisZ)) > BH.oM.Geometry.Tolerance.Angle)
            {
                document.Regenerate();

                // Check if the view got rotated and action if necessary
                BoundingBoxXYZ cropBox = result.CropBox;
                if (Math.Abs(cropBox.Transform.BasisX.DotProduct(boundingBoxXyz.Transform.BasisX)) < BH.oM.Geometry.Tolerance.Angle)
                {
                    // Rotate the cropbox by 90 degrees
                    Element cropBoxElement = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_Viewers).FirstOrDefault(x => x != result && x.Name == result.Name);
                    BoundingBoxXYZ boxOfCropBoxElement = cropBoxElement.get_BoundingBox(result);
                    ElementTransformUtils.RotateElement(document, cropBoxElement.Id, Line.CreateUnbound((boxOfCropBoxElement.Min + boxOfCropBoxElement.Max) / 2 + boxOfCropBoxElement.Transform.Origin, boundingBoxXyz.Transform.BasisZ), Math.PI / 2);

                    // Realign the crop box to show the desired part of the view
                    cropBox = result.CropBox;
                    BoundingBoxXYZ bbox = new BoundingBoxXYZ();
                    bbox.Enabled = true;

                    double xDomain = cropBox.Max.Y - cropBox.Min.Y;
                    double yDomain = cropBox.Max.X - cropBox.Min.X;
                    XYZ mid = (cropBox.Min + cropBox.Max) / 2;
                    bbox.Transform = cropBox.Transform;
                    bbox.Min = new XYZ(mid.X - xDomain / 2, mid.Y - yDomain / 2, result.CropBox.Min.Z);
                    bbox.Max = new XYZ(mid.X + xDomain / 2, mid.Y + yDomain / 2, result.CropBox.Max.Z);

                    result.CropBox = bbox;
                }
            }

            return result;
        }

        /***************************************************/
    }
}



