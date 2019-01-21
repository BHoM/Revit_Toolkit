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
using BH.oM.Adapters.Revit.Settings;
using System.Collections.Generic;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        static public List<oM.Geometry.ICurve> Curves(this GeometryElement geometryElement, Transform transform = null, PullSettings pullSettings = null)
        {
            if (geometryElement == null)
                return null;

            List<oM.Geometry.ICurve> aResult = new List<oM.Geometry.ICurve>();
            foreach (GeometryObject aGeometryObject in geometryElement)
            {
                if (aGeometryObject is Solid)
                {
                    Solid aSolid = (Solid)aGeometryObject;
                    EdgeArray aEdgeArray = aSolid.Edges;
                    if (aEdgeArray == null)
                        continue;

                    List<oM.Geometry.ICurve> aCurveList = aEdgeArray.ToBHoM(pullSettings);
                    if (aCurveList != null && aCurveList.Count != 0)
                        aResult.AddRange(aCurveList);                        
                }
                else if (aGeometryObject is GeometryInstance)
                {
                    GeometryInstance aGeometryInstance = (GeometryInstance)aGeometryObject;

                    Transform aTransform = aGeometryInstance.Transform;
                    if (transform != null)
                        aTransform = aTransform.Multiply(transform.Inverse);


                    GeometryElement aGeometryElement = aGeometryInstance.GetInstanceGeometry(aTransform);
                    if (aGeometryElement == null)
                        continue;

                    List<oM.Geometry.ICurve> aCurveList = Curves(aGeometryElement);
                    if (aCurveList != null && aCurveList.Count != 0)
                        aResult.AddRange(aCurveList);
                }
            }
            return aResult;
        }

        /***************************************************/

        static public List<oM.Geometry.ICurve> Curves(this Element element, Options options, PullSettings pullSettings = null)
        {
            GeometryElement aGeometryElement = element.get_Geometry(options);

            Transform aTransform = null;
            if (element is FamilyInstance)
                aTransform = ((FamilyInstance)element).GetTotalTransform();

            return Curves(aGeometryElement, aTransform, pullSettings);
        }

        /***************************************************/

        static public List<oM.Geometry.ICurve> Curves(this oM.Geometry.Polyline polyline)
        {
            if (polyline == null)
                return null;

            if (polyline.ControlPoints == null || polyline.ControlPoints.Count < 2)
                return null;

            List<oM.Geometry.ICurve> aResult = new List<oM.Geometry.ICurve>();

            for (int i = 1; i < polyline.ControlPoints.Count; i++)
                aResult.Add(BH.Engine.Geometry.Create.Line(polyline.ControlPoints[i - 1], polyline.ControlPoints[i]));

            if (BH.Engine.Geometry.Query.Distance(polyline.ControlPoints[polyline.ControlPoints.Count - 1], polyline.ControlPoints[0]) > oM.Geometry.Tolerance.MicroDistance)
                aResult.Add(BH.Engine.Geometry.Create.Line(polyline.ControlPoints[polyline.ControlPoints.Count - 1], polyline.ControlPoints[0]));

            return aResult;
        }
    }
}
