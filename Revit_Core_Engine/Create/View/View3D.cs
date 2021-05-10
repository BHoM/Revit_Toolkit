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
using System;
using System.ComponentModel;
using BH.Engine.Geometry;
using BH.oM.Reflection.Attributes;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Creates and returns a new ISOMETRIC 3D view in the current Revit file.")]
        [Input("document", "Revit current document to be processed.")]
        [Input("viewName", "Optional, name of the new view.")]
        [Input("viewTemplateId", "Optional, the View Template Id to be applied in the view.")]
        [Input("viewDetailLevel", "Optional, the Detail Level of the view.")]        
        [Output("view3D", "The new view.")]        
        public static View View3D(this Document document, string viewName = null, ElementId viewTemplateId = null, ViewDetailLevel viewDetailLevel = ViewDetailLevel.Coarse)
        {
            View result = null;

            ViewFamilyType vft = Query.ViewFamilyType(document, ViewFamily.ThreeDimensional);

            result = Autodesk.Revit.DB.View3D.CreateIsometric(document, vft.Id);
            result.DetailLevel = viewDetailLevel;

            if (viewTemplateId != null)
            {
                try
                {
                    result.ViewTemplateId = viewTemplateId;
                }
                catch (Exception)
                {
                    BH.Engine.Reflection.Compute.RecordWarning("Could not apply the View Template of Id " + viewTemplateId + "'." + ". Please check if it's a valid ElementId.");
                }
            }
            
            if (!string.IsNullOrEmpty(viewName))
            {
                try
                {
#if (REVIT2020 || REVIT2021)
                    result.Name = viewName;
#else
                    result.ViewName = viewName;
#endif
                }
                catch
                {
                    BH.Engine.Reflection.Compute.RecordWarning("There is already a view named '" + viewName + "'." + " It has been named '" + result.Name + "' instead.");
                }
            }
            
            return result;
        }
        
        /***************************************************/
        
        [Description("Creates and returns a new PERSPECTIVE 3D view in the current Revit file based on the view's constructed bounding box and orientation.")]
        [Input("document", "Revit current document to be processed.")]
        [Input("viewName", "Optional, name of the new view.")]
        [Input("boundingBoxXyz", "Optional, the BoundingBoxXYZ to fit the perspective view")]
        [Input("viewOrientation3D", "Optional, the orientation to which the perspective view will be set.")]
        [Input("viewTemplateId", "Optional, the View Template Id to be applied in the view.")]
        [Input("viewDetailLevel", "Optional, the Detail Level of the view.")]        
        [Output("view3D", "The new 3D view.")]       
        public static View View3D(this Document document, string viewName = null, BoundingBoxXYZ boundingBoxXyz = null, ViewOrientation3D viewOrientation3D = null, ElementId viewTemplateId = null, ViewDetailLevel viewDetailLevel = ViewDetailLevel.Coarse) 
        {
            //for information about a perspective boundingbox and its orientation see here:
            //https://knowledge.autodesk.com/support/revit-products/learn-explore/caas/CloudHelp/cloudhelp/2014/ENU/Revit/files/GUID-A7FA8DBC-830E-482D-9B66-147399524442-htm.html

            View3D result = null;

            ViewFamilyType vft = Query.ViewFamilyType(document, ViewFamily.ThreeDimensional);

            result = Autodesk.Revit.DB.View3D.CreatePerspective(document, vft.Id);
            result.DetailLevel = viewDetailLevel;
            
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
                    BH.Engine.Reflection.Compute.RecordWarning("There is already a view named '" + viewName + "'." + " It has been named '" + result.Name + "' instead.");
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
                    BH.Engine.Reflection.Compute.RecordWarning("Could not apply the View Template of Id " + viewTemplateId + "'." + ". Please check if it's a valid ElementId.");
                }
            }

            if (viewOrientation3D != null)
            {
                try
                {
                    result.SetOrientation(viewOrientation3D);
                }
                catch (Exception)
                {
                    BH.Engine.Reflection.Compute.RecordWarning("Could not set the view's orientation in 3D due to unexpected values, please report this error.");
                }
            }

            if (boundingBoxXyz != null)
            {
                try
                {
                    //transform to view coordinates previously picked points                    
                    BoundingBoxXYZ bb = result.get_BoundingBox(result);
                    Transform transform = bb.Transform;
                    
                    bb.Max = transform.Inverse.OfPoint(boundingBoxXyz.Max);
                    bb.Min = transform.Inverse.OfPoint(boundingBoxXyz.Min);

                    result.CropBox = bb;
                    
                    //changes the view's far clip to the the eye and target line's length (x1.5 offset)
                    result.SetParameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR,boundingBoxXyz.Min.DistanceTo(boundingBoxXyz.Max) * 1.5);

                }
                catch (Exception)
                {
                    BH.Engine.Reflection.Compute.RecordWarning("Could not set the view's bounding box due to unexpected values, please report this error.");
                }
            }

            return result;
        }
        
        /***************************************************/

        [Description("Creates and returns a new PERSPECTIVE 3D view in the current Revit file.")]
        [Input("document", "Revit current document to be processed.")]
        [Input("eye", "The 3D view eye point in XYZ, where the perspective starts.")]
        [Input("target", "The 3D view target point in XYZ, where the perspective is looking at.")]
        [Input("horizontalFieldOfView", "The view's horizontal dimension at target in feet, e.g. if target is a door then perhaps 3 feet will allow the view to see it entirely.")]
        [Input("viewRatio", "Optional, the view's lens/screen ratio in height/width, commonly 9/16.")]
        [Input("viewName", "Optional, name of the new view.")]
        [Input("viewTemplateId", "Optional, the View Template Id to be applied in the view.")]
        [Input("viewDetailLevel", "Optional, the Detail Level of the view.")]
        [Output("view3D", "The new 3D view.")]
        public static View View3D(this Document document, XYZ eye, XYZ target, double horizontalFieldOfView, double viewRatio = 0.5625, string viewName = null, ElementId viewTemplateId = null, ViewDetailLevel viewDetailLevel = ViewDetailLevel.Coarse)
        {
            XYZ normal = (target - eye).Normalize();
            XYZ planarEye = new XYZ(eye.X,eye.Y,0);
            XYZ planarTarget = new XYZ(target.X,target.Y,0);
            XYZ planarNormal = (planarTarget - planarEye).Normalize();

            //get vertical and horizontal angle
            double verticalAngle = (XYZ.BasisZ.AngleTo(normal) - Math.PI / 2) * (-1);
            double horizontalAngle = XYZ.BasisX.AngleOnPlaneTo(planarNormal, XYZ.BasisZ);

            //create view orientation
            ViewOrientation3D viewOrientation3D = new ViewOrientation3D(eye, CombineHorizontalWithVerticalAngles(horizontalAngle, verticalAngle + Math.PI/2), CombineHorizontalWithVerticalAngles(horizontalAngle, verticalAngle));

            //information can be found here
            //https://knowledge.autodesk.com/support/revit-products/learn-explore/caas/CloudHelp/cloudhelp/2014/ENU/Revit/files/GUID-A7FA8DBC-830E-482D-9B66-147399524442-htm.html
                
            //rotate center point to the right side, representing HFOV start point
            double angleToRotate = Math.PI / 2;
            Transform t1 = Transform.CreateRotationAtPoint(XYZ.BasisZ, angleToRotate * -1, target);
            XYZ rotate = target.Add((horizontalFieldOfView / 2) * (planarNormal * -1));
            XYZ hfovLeft = t1.OfPoint(rotate);
            XYZ bottomLeft = hfovLeft.Add(((viewRatio * horizontalFieldOfView) / 2) * (viewOrientation3D.UpDirection * -1));

            //for the right
            Transform t2 = Transform.CreateRotationAtPoint(XYZ.BasisZ, angleToRotate, target);
            XYZ hfovRight = t2.OfPoint(rotate);
            XYZ topRight = hfovRight.Add(((viewRatio * horizontalFieldOfView) / 2) * viewOrientation3D.UpDirection);

            //lines for top and bottom
            Line topLine = Line.CreateBound(topRight, eye);
            Line bottomLine = Line.CreateBound(bottomLeft, eye);

            // to calculate bb max/min offset we need to perform an inverse regression estimate using y=A+B/x
            double a = 0.9995538525; //constant for THIS inverse regression
            double b = -0.08573511; //constant for THIS inverse regression
            //get line-based element's length
            double cameraLength = planarEye.DistanceTo(planarTarget);
            double evaluateLines = a + (b / cameraLength);

            //creates plane to project point at and retrieve cropbox.min
            BH.oM.Geometry.Plane backClippingPlane = BH.Engine.Geometry.Create.Plane(BH.Revit.Engine.Core.Convert.PointFromRevit(target), BH.Revit.Engine.Core.Convert.VectorFromRevit(viewOrientation3D.ForwardDirection));
            BH.oM.Geometry.Point pointToProject = BH.Revit.Engine.Core.Convert.PointFromRevit(bottomLine.Evaluate(evaluateLines, true));
            BH.oM.Geometry.Point bbBhomMin = backClippingPlane.ClosestPoint(pointToProject);
            XYZ bbMin = BH.Revit.Engine.Core.Convert.ToRevit(bbBhomMin);
            XYZ bbMax = topLine.Evaluate(evaluateLines, true);
            
            BoundingBoxXYZ boundingBox3DCone = new BoundingBoxXYZ();
            boundingBox3DCone.Max = bbMax;
            boundingBox3DCone.Min = bbMin;

            return View3D(document, viewName, boundingBox3DCone, viewOrientation3D, viewTemplateId, viewDetailLevel);
        }
        
        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/
        
        [Description("Creates a unit vector by rotating the global X axis around global Z axis by radiansHorizontalAxis, and then rotating the resultant vector in the global Y axis around the global Z axis by radiansVerticalAxis.")]
        [Input("radiansHorizontalAxis", "The angle in radians from horizontal axis.")]
        [Input("radiansVerticalAxis", "The angle in radians from vertical axis.")]
        [Output("vector","The vector representing the direction of combining the two given angles and their axis.")]
        private static XYZ CombineHorizontalWithVerticalAngles(double radiansHorizontalAxis, double radiansVerticalAxis)
        {
            double a = Math.Cos(radiansHorizontalAxis);
            double b = Math.Sin(radiansHorizontalAxis);
            double c = Math.Tan(radiansVerticalAxis);

            return new XYZ(a, b, c);
        }
        
        /***************************************************/
    }
}
