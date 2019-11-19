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

            PlanarSurface aPlanarSurface = roof.Location as PlanarSurface;
            if (aPlanarSurface == null)
                return null;

            RoofBase aRoofBase = pushSettings.FindRefObject<RoofBase>(document, roof.BHoM_Guid);
            if (aRoofBase != null)
                return aRoofBase;

            pushSettings.DefaultIfNull();

            RoofType aRoofType = null;

            if (roof.Construction != null)
                aRoofType = roof.Construction.ToRevitHostObjAttributes(document, pushSettings) as RoofType;

            if (aRoofType == null)
            {
                string aFamilyTypeName = BH.Engine.Adapters.Revit.Query.FamilyTypeName(roof);
                if (!string.IsNullOrEmpty(aFamilyTypeName))
                {
                    List<RoofType> aRoofTypeList = new FilteredElementCollector(document).OfClass(typeof(RoofType)).Cast<RoofType>().ToList().FindAll(x => x.Name == aFamilyTypeName);
                    if (aRoofTypeList != null || aRoofTypeList.Count() != 0)
                        aRoofType = aRoofTypeList.First();
                }
            }

            if (aRoofType == null)
            {
                string aFamilyTypeName = roof.Name;

                if (!string.IsNullOrEmpty(aFamilyTypeName))
                {
                    List<RoofType> aRoofTypeList = new FilteredElementCollector(document).OfClass(typeof(RoofType)).Cast<RoofType>().ToList().FindAll(x => x.Name == aFamilyTypeName);
                    if (aRoofTypeList != null || aRoofTypeList.Count() != 0)
                        aRoofType = aRoofTypeList.First();
                }
            }

            if (aRoofType == null)
                return null;

            
            double aLowElevation = roof.LowElevation();
            if (double.IsNaN(aLowElevation))
                return null;

            Level aLevel = document.HighLevel(aLowElevation, true);
            if (aLevel == null)
                return null;

            double aElevation = aLevel.Elevation;
            if (pushSettings.ConvertUnits)
                aElevation = UnitUtils.ConvertFromInternalUnits(aElevation, DisplayUnitType.DUT_METERS);

            //oM.Geometry.Plane aPlane = BH.Engine.Geometry.Create.Plane(BH.Engine.Geometry.Create.Point(0, 0, aLowElevation), BH.Engine.Geometry.Create.Vector(0, 0, 1));
            oM.Geometry.Plane aPlane = BH.Engine.Geometry.Create.Plane(BH.Engine.Geometry.Create.Point(0, 0, aElevation), BH.Engine.Geometry.Create.Vector(0, 0, 1));

            ICurve aCurve = BH.Engine.Geometry.Modify.Project(aPlanarSurface.ExternalBoundary as dynamic, aPlane) as ICurve;

            CurveArray aCurveArray = null;
            if (aCurve is PolyCurve)
                aCurveArray = ((PolyCurve)aCurve).ToRevitCurveArray(pushSettings);
            else if (aCurve is Polyline)
                aCurveArray = ((Polyline)aCurve).ToRevitCurveArray(pushSettings);

            if (aCurveArray == null)
                return null;

            ModelCurveArray aModelCurveArray = new ModelCurveArray();
            aRoofBase = document.Create.NewFootPrintRoof(aCurveArray, aLevel, aRoofType, out aModelCurveArray);
            if (aRoofBase != null)
            {
                Parameter aParameter = aRoofBase.get_Parameter(BuiltInParameter.ROOF_UPTO_LEVEL_PARAM);
                if (aParameter != null)
                    aParameter.Set(ElementId.InvalidElementId);

                List<ICurve> aCurveList = null;

                if (aPlanarSurface.ExternalBoundary is PolyCurve)
                    aCurveList = ((PolyCurve)aPlanarSurface.ExternalBoundary).Curves;
                else if (aPlanarSurface.ExternalBoundary is Polyline)
                    aCurveList = Query.Curves((Polyline)aPlanarSurface.ExternalBoundary);

                if (aCurveList != null && aCurveList.Count > 2)
                {
                    SlabShapeEditor aSlabShapeEditor = aRoofBase.SlabShapeEditor;
                    aSlabShapeEditor.ResetSlabShape();

                    foreach (ICurve aCurve_Temp in aCurveList)
                    {
                        oM.Geometry.Point aPoint = BH.Engine.Geometry.Query.IStartPoint(aCurve_Temp);
                        if (System.Math.Abs(aPoint.Z - aPlane.Origin.Z) > Tolerance.MicroDistance)
                        {
                            XYZ aXYZ = aPoint.ToRevit(pushSettings);
                            aSlabShapeEditor.DrawPoint(aXYZ);
                        }
                            
                    }
                }
            }

            aRoofBase.CheckIfNullPush(roof);
            if (aRoofBase == null)
                return null;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aRoofBase, roof, new BuiltInParameter[] { BuiltInParameter.ROOF_LEVEL_OFFSET_PARAM, BuiltInParameter.ROOF_BASE_LEVEL_PARAM, BuiltInParameter.ROOF_UPTO_LEVEL_PARAM }, pushSettings.ConvertUnits);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(roof, aRoofBase);

            return aRoofBase;
        }

        /***************************************************/
    }
}