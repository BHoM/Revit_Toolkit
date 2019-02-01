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
using BH.oM.Structure.Properties.Section;
using BH.oM.Structure.Properties.Framing;
using BHG = BH.Engine.Geometry;
using BHS = BH.Engine.Structure;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static FramingElement ToBHoMFramingElement(this FamilyInstance familyInstance, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            FramingElement aFramingElement = pullSettings.FindRefObject<FramingElement>(familyInstance.Id.IntegerValue);
            if (aFramingElement != null)
                return aFramingElement;

            oM.Geometry.Line barCurve = null;
            bool nonlinear = false;
            ConstantFramingElementProperty property = null;
            StructuralUsage1D usage = StructuralUsage1D.Undefined;
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
                    barCurve = null;
                }
                else
                {
                    double baseLevel = (familyInstance.Document.GetElement(baseLevelParam.AsElementId()) as Level).ProjectElevation;
                    double topLevel = (familyInstance.Document.GetElement(topLevelParam.AsElementId()) as Level).ProjectElevation;
                    double baseOffset = baseOffsetParam.AsDouble();
                    double topOffset = topOffsetParam.AsDouble();
                    XYZ baseNode = new XYZ(loc.X, loc.Y, baseLevel + baseOffset);
                    XYZ topNode = new XYZ(loc.X, loc.Y, topLevel + topOffset);
                    barCurve = new oM.Geometry.Line { Start = baseNode.ToBHoM(pullSettings), End = topNode.ToBHoM(pullSettings) };
                }

                int multiplier = familyInstance.FacingOrientation.DotProduct(new XYZ(0, 1, 0)) > 0 ? 1 : -1;
                rotation = familyInstance.FacingOrientation.AngleTo(new XYZ(1, 0, 0)) * multiplier;
            }
            else if (location is LocationCurve)
            {
                barCurve = (location as LocationCurve).Curve.ToBHoM(pullSettings) as oM.Geometry.Line;
                if (barCurve == null)
                {
                    nonlinear = true;
                    familyInstance.NonlinearBarWarning();
                }
                else if (structuralType != StructuralType.Column)
                {
                    double ZOffset = familyInstance.LookupParameterDouble("z Offset Value", pullSettings.ConvertUnits);
                    barCurve = BHG.Modify.Translate(barCurve, new oM.Geometry.Vector { X = 0, Y = 0, Z = ZOffset });
                }
                rotation = -familyInstance.LookupParameterDouble(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE, false);
            }

            if (!nonlinear && barCurve == null) familyInstance.BarCurveNotFoundWarning();
            ISectionProperty aSectionProperty = familyInstance.ToBHoMSectionProperty(pullSettings) as ISectionProperty;

            switch (structuralType)
            {
                case StructuralType.Beam:
                    usage = StructuralUsage1D.Beam;
                    break;
                case StructuralType.Brace:
                    usage = StructuralUsage1D.Brace;
                    break;
                case StructuralType.Column:
                    usage = StructuralUsage1D.Column;
                    break;
            }

            //TODO: Allow varying orientation angle and varying cross sections (tapers etc) - TBC
            property = BHS.Create.ConstantFramingElementProperty(aSectionProperty, rotation, aSectionProperty.Name);
            
            if (familyInstance.Name != null)
                name = familyInstance.Name;

            aFramingElement = BHS.Create.FramingElement(barCurve, property, usage, name);
            aFramingElement = Modify.SetIdentifiers(aFramingElement, familyInstance) as FramingElement;
            if (pullSettings.CopyCustomData)
                aFramingElement = Modify.SetCustomData(aFramingElement, familyInstance, pullSettings.ConvertUnits) as FramingElement;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aFramingElement);

            return aFramingElement;
        }

        /***************************************************/
    }
}