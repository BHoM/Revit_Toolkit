/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static CurveElement ToCurveElement(this ModelInstance modelInstance, Document document, PushSettings pushSettings = null)
        {
            CurveElement curveElement = pushSettings.FindRefObject<CurveElement>(document, modelInstance.BHoM_Guid);
            if (curveElement != null)
                return curveElement;

            pushSettings.DefaultIfNull();

            if (!(modelInstance.Location is ICurve))
                return null;

            ICurve curve = (ICurve)modelInstance.Location;

            Curve revitCurve = curve.ToRevit(pushSettings);
            if (revitCurve == null)
                return null;

            if (!BH.Engine.Geometry.Query.IsPlanar(curve as dynamic))
                return null;

            List<oM.Geometry.Point> points = BH.Engine.Geometry.Query.ControlPoints(curve as dynamic);
            if (points == null || points.Count <= 1)
                return null;

            XYZ point1 = null;
            XYZ point2 = null;
            XYZ point3 = null;

            if (points.IsCollinear(BH.oM.Geometry.Tolerance.Distance))
            {
                point1 = points[0].ToRevit(pushSettings);
                point2 = points[1].ToRevit(pushSettings);

                XYZ vector = point2 - point1;
                double length = vector.GetLength();
                vector = vector.Normalize();
                XYZ parallelVector = null;
                if (vector.X == 1)
                    parallelVector = new XYZ(0, 1, 0);
                else if (vector.Y == 1)
                    parallelVector = new XYZ(1, 0, 0);
                else if (vector.Z == 1)
                    parallelVector = new XYZ(0, 1, 0);
                else if (vector.X != 0)
                    parallelVector = new XYZ(-vector.X, vector.Y, vector.Z);
                else if (vector.Y != 0)
                    parallelVector = new XYZ(vector.X, -vector.Y, vector.Z);
                else if (vector.Z != 0)
                    parallelVector = new XYZ(vector.X, vector.Y, -vector.Z);
                else
                    parallelVector = XYZ.Zero;

                vector = parallelVector * length;

                point3 = point1 + vector;
            }
            else
            {
                point1 = points[0].ToRevit(pushSettings);
                point2 = points[1].ToRevit(pushSettings);
                point3 = points[2].ToRevit(pushSettings);
            }

            SketchPlane sketchPlane = SketchPlane.Create(document, Autodesk.Revit.DB.Plane.CreateByThreePoints(point1, point2, point3));

            curveElement = document.Create.NewModelCurve(revitCurve, sketchPlane);

            if (modelInstance.Properties != null)
            {
                string name = modelInstance.Properties.Name;
                if (!string.IsNullOrEmpty(name))
                {
                    Element element = new FilteredElementCollector(document).OfClass(typeof(GraphicsStyle)).ToList().Find(x => x.Name == name);
                    if (element != null)
                        curveElement.LineStyle = element;
                }
            }

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(modelInstance, curveElement);

            return curveElement;
        }

        /***************************************************/

        public static CurveElement ToCurveElement(this DraftingInstance draftingInstance, Document document, PushSettings pushSettings = null)
        {
            CurveElement curveElement = pushSettings.FindRefObject<CurveElement>(document, draftingInstance.BHoM_Guid);
            if (curveElement != null)
                return curveElement;

            pushSettings.DefaultIfNull();

            if (!(draftingInstance.Location is ICurve))
                return null;

            ICurve curve = (ICurve)draftingInstance.Location;

            Curve revitCurve = curve.ToRevit(pushSettings);
            if (revitCurve == null)
                return null;

            if (!BH.Engine.Geometry.Query.IsPlanar(curve as dynamic))
                return null;

            View view = Query.View(draftingInstance, document);
            if (view == null)
                return null;

            curveElement = document.Create.NewDetailCurve(view, revitCurve);
            if (curveElement == null)
                return null;

            if (draftingInstance.Properties != null)
            {
                string name = draftingInstance.Properties.Name;
                if(!string.IsNullOrEmpty(name))
                {
                    Element element = new FilteredElementCollector(document).OfClass(typeof(GraphicsStyle)).ToList().Find(x => x.Name == name);
                    if(element != null)
                        curveElement.LineStyle = element;
                } 
            }

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(draftingInstance, curveElement);

            return curveElement;
        }

        /***************************************************/
    }
}