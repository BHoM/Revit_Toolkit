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
using BH.Engine.Adapters.Revit;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static RoofBase ToRevitRoofBase(this oM.Physical.Elements.Roof roof, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (roof == null || roof.Location == null || document == null)
                return null;

            PlanarSurface planarSurface = roof.Location as PlanarSurface;
            if (planarSurface == null)
                return null;

            RoofBase roofBase = refObjects.GetValue<RoofBase>(document, roof.BHoM_Guid);
            if (roofBase != null)
                return roofBase;

            settings = settings.DefaultIfNull();

            RoofType roofType = roof.Construction?.ToRevitElementType(document, new List<BuiltInCategory> { BuiltInCategory.OST_Roofs }, settings, refObjects) as RoofType;
            if (roofType == null)
                roofType = roof.ElementType(document, settings);

            if (roofType == null)
            {
                Compute.ElementTypeNotFoundWarning(roof);
                return null;
            }

            Level level = document.LevelBelow(roof.Location, settings);
            if (level == null)
                return null;

            double elevation = level.Elevation.ToSI(SpecTypeId.Length);
            
            oM.Geometry.Plane plane = BH.Engine.Geometry.Create.Plane(BH.Engine.Geometry.Create.Point(0, 0, elevation), BH.Engine.Geometry.Create.Vector(0, 0, 1));

            ICurve curve = planarSurface.ExternalBoundary.IProject(plane);
            CurveArray curveArray = Create.CurveArray(curve.IToRevitCurves());

            ModelCurveArray modelCurveArray = new ModelCurveArray();
            roofBase = document.Create.NewFootPrintRoof(curveArray, level, roofType, out modelCurveArray);
            
            if (roofBase != null)
            {
                Parameter parameter = roofBase.get_Parameter(BuiltInParameter.ROOF_UPTO_LEVEL_PARAM);
                if (parameter != null)
                    parameter.Set(ElementId.InvalidElementId);

                List<oM.Geometry.Point> controlPoints = planarSurface.ExternalBoundary.IControlPoints();

                if (controlPoints != null && controlPoints.Count > 2)
                {
                    SlabShapeEditor slabShapeEditor = roofBase.SlabShapeEditor;
                    slabShapeEditor.ResetSlabShape();

                    foreach (oM.Geometry.Point point in controlPoints)
                    {
                        if (Math.Abs(point.Z - plane.Origin.Z) > settings.DistanceTolerance)
                        {
                            XYZ xyz = point.ToRevit();
                            slabShapeEditor.DrawPoint(xyz);
                        }
                            
                    }
                }
            }

            roofBase.CheckIfNullPush(roof);
            if (roofBase == null)
                return null;

            // Copy parameters from BHoM object to Revit element
            roofBase.CopyParameters(roof, settings);

            // Update the offset in case the level had been overwritten.
            double offset = 0;
            if (roofBase.LevelId.IntegerValue != level.Id.IntegerValue)
            {
                Level newLevel = document.GetElement(roofBase.LevelId) as Level;
                offset += (level.ProjectElevation - newLevel.ProjectElevation).ToSI(SpecTypeId.Length);
            }

            roofBase.SetParameter(BuiltInParameter.ROOF_LEVEL_OFFSET_PARAM, offset);

            refObjects.AddOrReplace(roof, roofBase);
            return roofBase;
        }

        /***************************************************/
    }
}

