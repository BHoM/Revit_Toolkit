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
using Autodesk.Revit.DB.Structure;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Structure.Elements;
using BH.oM.Physical.Elements;
using BH.oM.Physical.FramingProperties;
using BH.oM.Geometry.ShapeProfiles;
using BHG = BH.Engine.Geometry;
using BHS = BH.Engine.Structure;

using System;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static IFramingElement ToBHoMFramingElement(this FamilyInstance familyInstance, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            IFramingElement aFramingElement = pullSettings.FindRefObject<IFramingElement>(familyInstance.Id.IntegerValue);
            if (aFramingElement != null)
                return aFramingElement;

            oM.Geometry.ICurve locationCurve = null;
            bool nonlinear = false;
            ConstantFramingProperty property = null;
            string name = null;

            StructuralType structuralType = familyInstance.StructuralType;

            //TODO: switch from explicit Line to ICurve
            Location location = familyInstance.Location;
            double rotation = double.NaN;

            if (location is LocationPoint && structuralType == StructuralType.Column)
            {
                XYZ loc = (location as LocationPoint).Point;
                Parameter baseLevelParam = familyInstance.LookupParameter("Base Level");
                Parameter topLevelParam = familyInstance.LookupParameter("Top Level");
                Parameter baseOffsetParam = familyInstance.LookupParameter("Base Offset");
                Parameter topOffsetParam = familyInstance.LookupParameter("Top Offset");

                if (baseLevelParam == null || !baseLevelParam.HasValue || topLevelParam == null || !topLevelParam.HasValue || baseOffsetParam == null || !baseOffsetParam.HasValue || topOffsetParam == null || !topOffsetParam.HasValue)
                {
                    locationCurve = null;
                }
                else
                {
                    double baseLevel = (familyInstance.Document.GetElement(baseLevelParam.AsElementId()) as Level).ProjectElevation;
                    double topLevel = (familyInstance.Document.GetElement(topLevelParam.AsElementId()) as Level).ProjectElevation;
                    double baseOffset = baseOffsetParam.AsDouble();
                    double topOffset = topOffsetParam.AsDouble();
                    XYZ baseNode = new XYZ(loc.X, loc.Y, baseLevel + baseOffset);
                    XYZ topNode = new XYZ(loc.X, loc.Y, topLevel + topOffset);
                    locationCurve = new oM.Geometry.Line { Start = baseNode.ToBHoM(pullSettings), End = topNode.ToBHoM(pullSettings) };
                }

                rotation = Math.PI * 0.5 + (location as LocationPoint).Rotation;

            }
            else if (location is LocationCurve)
            {
                locationCurve = (location as LocationCurve).Curve.ToBHoM(pullSettings);
                if (locationCurve == null)
                {
                    nonlinear = true;
                    familyInstance.NonlinearBarWarning();
                }
                else if (structuralType != StructuralType.Column)
                {
                    double ZOffset = familyInstance.LookupDouble("z Offset Value", pullSettings.ConvertUnits);
                    if (ZOffset != 0 && !double.IsNaN(ZOffset))
                        locationCurve = BHG.Modify.Translate(locationCurve as dynamic, new oM.Geometry.Vector { X = 0, Y = 0, Z = ZOffset });
                }

                if (structuralType == StructuralType.Column && locationCurve is BH.oM.Geometry.Line && BHS.Query.IsVertical(locationCurve as BH.oM.Geometry.Line))
                {
                    rotation = Math.PI * 0.5 - familyInstance.LookupDouble(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE, false);
                }
                else
                {
                    if (IsVerticalNonLinearCurve((location as LocationCurve).Curve))
                    {
                        rotation = Math.PI * 0.5 - familyInstance.LookupDouble(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE, false);
                    }
                    else
                        rotation = -familyInstance.LookupDouble(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE, false);
                        
                }

            }

            if (!nonlinear && locationCurve == null) familyInstance.BarCurveNotFoundWarning();

            IProfile profile = familyInstance.Symbol.ToBHoMProfile(pullSettings);

            BH.oM.Physical.Materials.Material material = pullSettings.FindRefObject<oM.Physical.Materials.Material>(familyInstance.StructuralMaterialId.IntegerValue);

            if (material == null)
            {
                ElementId materialId = familyInstance.StructuralMaterialId;

                if (materialId.IntegerValue != -1)
                {
                    Autodesk.Revit.DB.Material aMaterial_Revit = familyInstance.Document.GetElement(materialId) as Autodesk.Revit.DB.Material;
                    if (aMaterial_Revit != null)
                        material = aMaterial_Revit.ToBHoMMaterial(pullSettings);
                }

                if (material == null)
                {
                    Compute.InvalidDataMaterialWarning(familyInstance);
                    Compute.MaterialTypeNotFoundWarning(familyInstance);
                    material = new oM.Physical.Materials.Material();
                }
            }


            //TODO: Allow varying orientation angle and varying cross sections (tapers etc) - TBC
            property = BH.Engine.Physical.Create.ConstantFramingProperty(profile, material, rotation, profile.Name);

            if (familyInstance.Name != null)
                name = familyInstance.Name;

            switch (structuralType)
            {

                case StructuralType.Beam:
                    aFramingElement = BH.Engine.Physical.Create.Beam(locationCurve, property, name);
                    break;
                case StructuralType.Brace:
                    aFramingElement = BH.Engine.Physical.Create.Bracing(locationCurve, property, name);
                    break;
                case StructuralType.Column:
                    aFramingElement = BH.Engine.Physical.Create.Column(locationCurve, property, name);
                    break;
                case StructuralType.NonStructural:
                case StructuralType.Footing:
                case StructuralType.UnknownFraming:
                default:
                    aFramingElement = BH.Engine.Physical.Create.Beam(locationCurve, property, name);
                    break;
            }

            aFramingElement = Modify.SetIdentifiers(aFramingElement, familyInstance) as IFramingElement;
            if (pullSettings.CopyCustomData)
                aFramingElement = Modify.SetCustomData(aFramingElement, familyInstance, pullSettings.ConvertUnits) as IFramingElement;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aFramingElement);

            return aFramingElement;
        }

        /***************************************************/

        private static bool IsVerticalNonLinearCurve(Curve revitCurve)
        {
            if (!(revitCurve is Line))
            {
                CurveLoop curveLoop = CurveLoop.Create(new Curve[] { revitCurve });
                if (curveLoop.HasPlane())
                {
                    Plane curvePlane = curveLoop.GetPlane();
                    //Orientation angles are handled slightly differently for framing elements that have a curve fits in a plane that contains the z-vector
                    if (Math.Abs(curvePlane.Normal.DotProduct(XYZ.BasisZ)) < BH.oM.Geometry.Tolerance.Angle)
                        return true;
                }
            }
            return false;
        }

        /***************************************************/
    }
}