/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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
        /**** Internal Methods                          ****/
        /***************************************************/

        internal static CurveElement ToCurveElement(this ModelInstance modelInstance, Document document, PushSettings pushSettings = null)
        {
            CurveElement aCurveElement = pushSettings.FindRefObject<CurveElement>(document, modelInstance.BHoM_Guid);
            if (aCurveElement != null)
                return aCurveElement;

            pushSettings.DefaultIfNull();

            if (!(modelInstance.Location is ICurve))
                return null;

            ICurve aCurve_BHoM = (ICurve)modelInstance.Location;

            Curve aCurve_Revit = aCurve_BHoM.ToRevit(pushSettings);
            if (aCurve_Revit == null)
                return null;

            if (!BH.Engine.Geometry.Query.IsPlanar(aCurve_BHoM as dynamic))
                return null;

            List<oM.Geometry.Point> aPointList = BH.Engine.Geometry.Query.ControlPoints(aCurve_BHoM as dynamic);
            if (aPointList == null || aPointList.Count <= 1)
                return null;

            XYZ aPoint_1 = null;
            XYZ aPoint_2 = null;
            XYZ aPoint_3 = null;

            if(aPointList.Count == 2)
            {
                aPoint_1 = aPointList[0].ToRevit(pushSettings);
                aPoint_2 = aPointList[1].ToRevit(pushSettings);

                XYZ aVector = aPoint_2 - aPoint_1;
                double aLength = aVector.GetLength();
                aVector = aVector.Normalize();
                XYZ aVector_Parallel = null;
                if (aVector.X == 1)
                    aVector_Parallel = new XYZ(0, 1, 0);
                else if (aVector.Y == 1)
                    aVector_Parallel = new XYZ(1, 0, 0);
                else if (aVector.Z == 1)
                    aVector_Parallel = new XYZ(0, 1, 0);
                else if (aVector.X != 0)
                    aVector_Parallel = new XYZ(-aVector.X, aVector.Y, aVector.Z);
                else if (aVector.Y != 0)
                    aVector_Parallel = new XYZ(aVector.X, -aVector.Y, aVector.Z);
                else if (aVector.Z != 0)
                    aVector_Parallel = new XYZ(aVector.X, aVector.Y, -aVector.Z);
                else
                    aVector_Parallel = XYZ.Zero;

                aVector = aVector_Parallel * aLength;

                aPoint_3 = aPoint_1 + aVector;
            }
            else
            {
                aPoint_1 = aPointList[0].ToRevit(pushSettings);
                aPoint_2 = aPointList[1].ToRevit(pushSettings);
                aPoint_3 = aPointList[2].ToRevit(pushSettings);
            }

            SketchPlane aSketchPlane = SketchPlane.Create(document, Autodesk.Revit.DB.Plane.CreateByThreePoints(aPoint_1, aPoint_2, aPoint_3));

            aCurveElement = document.Create.NewModelCurve(aCurve_Revit, aSketchPlane);

            if (modelInstance.Properties != null)
            {
                string aName = modelInstance.Properties.Name;
                if (!string.IsNullOrEmpty(aName))
                {
                    Element aElement = new FilteredElementCollector(document).OfClass(typeof(GraphicsStyle)).ToList().Find(x => x.Name == aName);
                    if (aElement != null)
                        aCurveElement.LineStyle = aElement;
                }
            }

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(modelInstance, aCurveElement);

            return aCurveElement;
        }

        /***************************************************/

        internal static CurveElement ToCurveElement(this DraftingInstance draftingInstance, Document document, PushSettings pushSettings = null)
        {
            CurveElement aCurveElement = pushSettings.FindRefObject<CurveElement>(document, draftingInstance.BHoM_Guid);
            if (aCurveElement != null)
                return aCurveElement;

            pushSettings.DefaultIfNull();

            if (!(draftingInstance.Location is ICurve))
                return null;

            ICurve aCurve_BHoM = (ICurve)draftingInstance.Location;

            Curve aCurve_Revit = aCurve_BHoM.ToRevit(pushSettings);
            if (aCurve_Revit == null)
                return null;

            if (!BH.Engine.Geometry.Query.IsPlanar(aCurve_BHoM as dynamic))
                return null;

            View aView = Query.View(draftingInstance, document);
            if (aView == null)
                return null;

            aCurveElement = document.Create.NewDetailCurve(aView, aCurve_Revit);
            if (aCurveElement == null)
                return null;

            if (draftingInstance.Properties != null)
            {
                string aName = draftingInstance.Properties.Name;
                if(!string.IsNullOrEmpty(aName))
                {
                    Element aElement = new FilteredElementCollector(document).OfClass(typeof(GraphicsStyle)).ToList().Find(x => x.Name == aName);
                    if(aElement != null)
                        aCurveElement.LineStyle = aElement;
                } 
            }

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(draftingInstance, aCurveElement);

            return aCurveElement;
        }

        /***************************************************/
    }
}