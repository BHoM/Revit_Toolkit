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

using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static RoofBase ToRevitRoofBase(this oM.Physical.Elements.Roof roof, Document document, PushSettings pushSettings = null)
        {
            if (roof == null || roof.Location == null || document == null)
                return null;

            PlanarSurface planarSurface = roof.Location as PlanarSurface;
            if (planarSurface == null)
                return null;

            RoofBase roofBase = pushSettings.FindRefObject<RoofBase>(document, roof.BHoM_Guid);
            if (roofBase != null)
                return roofBase;

            pushSettings.DefaultIfNull();

            RoofType roofType = null;

            if (roof.Construction != null)
                roofType = roof.Construction.ToRevitHostObjAttributes(document, pushSettings) as RoofType;

            if (roofType == null)
            {
                string familyTypeName = BH.Engine.Adapters.Revit.Query.FamilyTypeName(roof);
                if (!string.IsNullOrEmpty(familyTypeName))
                {
                    List<RoofType> roofTypeList = new FilteredElementCollector(document).OfClass(typeof(RoofType)).Cast<RoofType>().ToList().FindAll(x => x.Name == familyTypeName);
                    if (roofTypeList != null || roofTypeList.Count() != 0)
                        roofType = roofTypeList.First();
                }
            }

            if (roofType == null)
            {
                string familyTypeName = roof.Name;

                if (!string.IsNullOrEmpty(familyTypeName))
                {
                    List<RoofType> roofTypeList = new FilteredElementCollector(document).OfClass(typeof(RoofType)).Cast<RoofType>().ToList().FindAll(x => x.Name == familyTypeName);
                    if (roofTypeList != null || roofTypeList.Count() != 0)
                        roofType = roofTypeList.First();
                }
            }

            if (roofType == null)
                return null;
            
            double lowElevation = roof.LowElevation();
            if (double.IsNaN(lowElevation))
                return null;

            Level level = document.HighLevel(lowElevation);
            if (level == null)
                return null;

            double elevation = level.Elevation.ToSI(UnitType.UT_Length);
            
            oM.Geometry.Plane plane = BH.Engine.Geometry.Create.Plane(BH.Engine.Geometry.Create.Point(0, 0, elevation), BH.Engine.Geometry.Create.Vector(0, 0, 1));

            ICurve curve = BH.Engine.Geometry.Modify.Project(planarSurface.ExternalBoundary as dynamic, plane) as ICurve;

            CurveArray curveArray = null;
            if (curve is PolyCurve)
                curveArray = ((PolyCurve)curve).ToRevitCurveArray();
            else if (curve is Polyline)
                curveArray = ((Polyline)curve).ToRevitCurveArray();

            if (curveArray == null)
                return null;

            ModelCurveArray modelCurveArray = new ModelCurveArray();
            roofBase = document.Create.NewFootPrintRoof(curveArray, level, roofType, out modelCurveArray);
            if (roofBase != null)
            {
                Parameter parameter = roofBase.get_Parameter(BuiltInParameter.ROOF_UPTO_LEVEL_PARAM);
                if (parameter != null)
                    parameter.Set(ElementId.InvalidElementId);

                List<ICurve> curveList = null;

                if (planarSurface.ExternalBoundary is PolyCurve)
                    curveList = ((PolyCurve)planarSurface.ExternalBoundary).Curves;
                else if (planarSurface.ExternalBoundary is Polyline)
                    curveList = Query.Curves((Polyline)planarSurface.ExternalBoundary);

                if (curveList != null && curveList.Count > 2)
                {
                    SlabShapeEditor slabShapeEditor = roofBase.SlabShapeEditor;
                    slabShapeEditor.ResetSlabShape();

                    foreach (ICurve tempCurve in curveList)
                    {
                        oM.Geometry.Point point = BH.Engine.Geometry.Query.IStartPoint(tempCurve);
                        if (System.Math.Abs(point.Z - plane.Origin.Z) > Tolerance.MicroDistance)
                        {
                            XYZ xyz = point.ToRevit(pushSettings);
                            slabShapeEditor.DrawPoint(xyz);
                        }
                            
                    }
                }
            }

            roofBase.CheckIfNullPush(roof);
            if (roofBase == null)
                return null;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(roofBase, roof, new BuiltInParameter[] { BuiltInParameter.ROOF_LEVEL_OFFSET_PARAM, BuiltInParameter.ROOF_BASE_LEVEL_PARAM, BuiltInParameter.ROOF_UPTO_LEVEL_PARAM });

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(roof, roofBase);

            return roofBase;
        }

        /***************************************************/
    }
}