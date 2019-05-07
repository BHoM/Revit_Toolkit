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

using System;
using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure.StructuralSections;

using BH.oM.Structure.SectionProperties;
using BH.oM.Geometry.ShapeProfiles;
using BHS = BH.Engine.Structure;
using BHG = BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static IProfile ToBHoMProfile(this FamilySymbol familySymbol, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            IProfile aProfile = pullSettings.FindRefObject<IProfile>(familySymbol.Id.IntegerValue);
            if (aProfile != null)
                return aProfile;

            string familyName = familySymbol.Family.Name;
            Parameter sectionShapeParam = familySymbol.LookupParameter("Section Shape");
            StructuralSectionShape sectionShape = sectionShapeParam == null ? sectionShape = StructuralSectionShape.NotDefined : (StructuralSectionShape)sectionShapeParam.AsInteger();
            
            List<Type> aTypes = Query.BHoMTypes(sectionShape).ToList();
            if (aTypes.Count == 0) aTypes.AddRange(Query.BHoMTypes(familyName));

            if (aTypes.Contains(typeof(CircleProfile)))
            {
                double diameter;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionConcreteRound) diameter = (section as StructuralSectionConcreteRound).Diameter;
                else if (section is StructuralSectionConcreteRound) diameter = (section as StructuralSectionConcreteRound).Diameter;
                else diameter = familySymbol.LookupDouble(diameterNames, false);

                if (!double.IsNaN(diameter))
                {
                    if (pullSettings.ConvertUnits) diameter = diameter.ToSI(UnitType.UT_Section_Dimension);
                    aProfile = BHG.Create.CircleProfile(diameter);
                }
                else
                {
                    double radius = familySymbol.LookupDouble(radiusNames, pullSettings.ConvertUnits);
                    if (!double.IsNaN(radius))
                    {
                        aProfile = BHG.Create.CircleProfile(radius * 2);
                    }
                }
            }

            else if (aTypes.Contains(typeof(FabricatedISectionProfile)))
            {
                double height, topFlangeWidth, botFlangeWidth, webThickness, topFlangeThickness, botFlangeThickness, weldSize;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionIWelded)
                {
                    StructuralSectionIWelded sectionType = section as StructuralSectionIWelded;
                    height = sectionType.Height;
                    topFlangeWidth = sectionType.TopFlangeWidth;
                    botFlangeWidth = sectionType.BottomFlangeWidth;
                    webThickness = sectionType.WebThickness;
                    topFlangeThickness = sectionType.TopFlangeThickness;
                    botFlangeThickness = sectionType.BottomFlangeThickness;
                    weldSize = 0;
                }
                else
                {
                    height = familySymbol.LookupDouble(heightNames, false);
                    topFlangeWidth = familySymbol.LookupDouble(topFlangeWidthNames, false);
                    botFlangeWidth = familySymbol.LookupDouble(botFlangeWidthNames, false);
                    webThickness = familySymbol.LookupDouble(webThicknessNames, false);
                    topFlangeThickness = familySymbol.LookupDouble(topFlangeThicknessNames, false);
                    botFlangeThickness = familySymbol.LookupDouble(botFlangeThicknessNames, false);
                    weldSize = familySymbol.LookupDouble(weldSizeNames1, false);
                }

                if (double.IsNaN(weldSize))
                {
                    weldSize = familySymbol.LookupDouble(weldSizeNames2, false);
                    if (!double.IsNaN(weldSize) && !double.IsNaN(webThickness))
                    {
                        weldSize = (weldSize - webThickness) / (Math.Sqrt(2));
                    }
                    else
                    {
                        weldSize = 0;
                    }
                }

                if (!double.IsNaN(height) && !double.IsNaN(topFlangeWidth) && !double.IsNaN(botFlangeWidth) && !double.IsNaN(webThickness) && !double.IsNaN(topFlangeThickness) && !double.IsNaN(botFlangeThickness))
                {
                    if (pullSettings.ConvertUnits)
                    {
                        height = height.ToSI(UnitType.UT_Section_Dimension);
                        topFlangeWidth = topFlangeWidth.ToSI(UnitType.UT_Section_Dimension);
                        botFlangeWidth = botFlangeWidth.ToSI(UnitType.UT_Section_Dimension);
                        webThickness = webThickness.ToSI(UnitType.UT_Section_Dimension);
                        topFlangeThickness = topFlangeThickness.ToSI(UnitType.UT_Section_Dimension);
                        botFlangeThickness = botFlangeThickness.ToSI(UnitType.UT_Section_Dimension);
                        weldSize = weldSize.ToSI(UnitType.UT_Section_Dimension);
                    }

                    aProfile = BHG.Create.FabricatedISectionProfile(height, topFlangeWidth, botFlangeWidth, webThickness, topFlangeThickness, botFlangeThickness, weldSize);
                }
            }

            else if (aTypes.Contains(typeof(RectangleProfile)))
            {
                double height, width, cornerRadius;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionConcreteRectangle)
                {
                    StructuralSectionConcreteRectangle sectionType = section as StructuralSectionConcreteRectangle;
                    height = sectionType.Height;
                    width = sectionType.Width;
                    cornerRadius = 0;
                }
                else if (section is StructuralSectionRectangularBar)
                {
                    StructuralSectionRectangularBar sectionType = section as StructuralSectionRectangularBar;
                    height = sectionType.Height;
                    width = sectionType.Width;
                    cornerRadius = 0;
                }
                else if (section is StructuralSectionRectangleParameterized)
                {
                    StructuralSectionRectangleParameterized sectionType = section as StructuralSectionRectangleParameterized;
                    height = sectionType.Height;
                    width = sectionType.Width;
                    cornerRadius = 0;
                }
                else
                {
                    height = familySymbol.LookupDouble(heightNames, false);
                    width = familySymbol.LookupDouble(widthNames, false);
                    cornerRadius = familySymbol.LookupDouble(cornerRadiusNames, false);
                }

                if (double.IsNaN(cornerRadius)) cornerRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width))
                {
                    if (pullSettings.ConvertUnits)
                    {
                        height = height.ToSI(UnitType.UT_Section_Dimension);
                        width = width.ToSI(UnitType.UT_Section_Dimension);
                        cornerRadius = cornerRadius.ToSI(UnitType.UT_Section_Dimension);
                    }

                    aProfile = BHG.Create.RectangleProfile(height, width, cornerRadius);
                }
            }

            else if (aTypes.Contains(typeof(AngleProfile)))
            {
                double height, width, webThickness, flangeThickness, rootRadius, toeRadius;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionLAngle)
                {
                    StructuralSectionLAngle sectionType = section as StructuralSectionLAngle;
                    height = sectionType.Height;
                    width = sectionType.Width;
                    webThickness = sectionType.WebThickness;
                    flangeThickness = sectionType.FlangeThickness;
                    rootRadius = sectionType.WebFillet;
                    toeRadius = sectionType.FlangeFillet;
                }
                else if (section is StructuralSectionLProfile)
                {
                    //TODO: Implement cold-formed profiles?
                    StructuralSectionLProfile sectionType = section as StructuralSectionLProfile;
                    height = sectionType.Height;
                    width = sectionType.Width;
                    webThickness = sectionType.WallNominalThickness;
                    flangeThickness = sectionType.WallNominalThickness;
                    rootRadius = sectionType.InnerFillet;
                    toeRadius = 0;
                }
                else
                {
                    height = familySymbol.LookupDouble(heightNames, false);
                    width = familySymbol.LookupDouble(widthNames, false);
                    webThickness = familySymbol.LookupDouble(webThicknessNames, false);
                    flangeThickness = familySymbol.LookupDouble(flangeThicknessNames, false);
                    rootRadius = familySymbol.LookupDouble(rootRadiusNames, false);
                    toeRadius = familySymbol.LookupDouble(toeRadiusNames, false);
                }

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness))
                {
                    if (pullSettings.ConvertUnits)
                    {
                        height = height.ToSI(UnitType.UT_Section_Dimension);
                        width = width.ToSI(UnitType.UT_Section_Dimension);
                        webThickness = webThickness.ToSI(UnitType.UT_Section_Dimension);
                        flangeThickness = flangeThickness.ToSI(UnitType.UT_Section_Dimension);
                        rootRadius = rootRadius.ToSI(UnitType.UT_Section_Dimension);
                        toeRadius = toeRadius.ToSI(UnitType.UT_Section_Dimension);
                    }

                    aProfile = BHG.Create.AngleProfile(height, width, webThickness, flangeThickness, rootRadius, toeRadius);
                }
            }

            else if (aTypes.Contains(typeof(BoxProfile)))
            {
                double height, width, thickness, outerRadius, innerRadius;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionRectangleHSS)
                {
                    StructuralSectionRectangleHSS sectionType = section as StructuralSectionRectangleHSS;
                    height = sectionType.Height;
                    width = sectionType.Width;
                    thickness = sectionType.WallNominalThickness;
                    outerRadius = sectionType.OuterFillet;
                    innerRadius = sectionType.InnerFillet;
                }
                else
                {
                    height = familySymbol.LookupDouble(heightNames, false);
                    width = familySymbol.LookupDouble(widthNames, false);
                    thickness = familySymbol.LookupDouble(wallThicknessNames, false);
                    outerRadius = familySymbol.LookupDouble(outerRadiusNames, false);
                    innerRadius = familySymbol.LookupDouble(innerRadiusNames, false);
                }

                if (double.IsNaN(outerRadius)) outerRadius = 0;
                if (double.IsNaN(innerRadius)) innerRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(thickness))
                {
                    if (pullSettings.ConvertUnits)
                    {
                        height = height.ToSI(UnitType.UT_Section_Dimension);
                        width = width.ToSI(UnitType.UT_Section_Dimension);
                        thickness = thickness.ToSI(UnitType.UT_Section_Dimension);
                        outerRadius = outerRadius.ToSI(UnitType.UT_Section_Dimension);
                        innerRadius = innerRadius.ToSI(UnitType.UT_Section_Dimension);
                    }

                    aProfile = BHG.Create.BoxProfile(height, width, thickness, outerRadius, innerRadius);
                }
            }

            else if (aTypes.Contains(typeof(ChannelProfile)))
            {
                double height, flangeWidth, webThickness, flangeThickness, rootRadius, toeRadius;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionCParallelFlange)
                {
                    StructuralSectionCParallelFlange sectionType = section as StructuralSectionCParallelFlange;
                    height = sectionType.Height;
                    flangeWidth = sectionType.Width;
                    webThickness = sectionType.WebThickness;
                    flangeThickness = sectionType.FlangeThickness;
                    rootRadius = sectionType.WebFillet;
                    toeRadius = sectionType.FlangeFillet;
                }
                else if (section is StructuralSectionCProfile)
                {
                    //TODO: Implement cold-formed profiles?
                    StructuralSectionCProfile sectionType = section as StructuralSectionCProfile;
                    height = sectionType.Height;
                    flangeWidth = sectionType.Width;
                    webThickness = sectionType.WallNominalThickness;
                    flangeThickness = sectionType.WallNominalThickness;
                    rootRadius = sectionType.InnerFillet;
                    toeRadius = 0;
                }
                else
                {
                    height = familySymbol.LookupDouble(heightNames, false);
                    flangeWidth = familySymbol.LookupDouble(widthNames, false);
                    webThickness = familySymbol.LookupDouble(webThicknessNames, false);
                    flangeThickness = familySymbol.LookupDouble(flangeThicknessNames, false);
                    rootRadius = familySymbol.LookupDouble(rootRadiusNames, false);
                    toeRadius = familySymbol.LookupDouble(toeRadiusNames, false);
                }

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(flangeWidth) && double.IsNaN(webThickness) && !double.IsNaN(flangeThickness))
                {
                    if (pullSettings.ConvertUnits)
                    {
                        height = height.ToSI(UnitType.UT_Section_Dimension);
                        flangeWidth = flangeWidth.ToSI(UnitType.UT_Section_Dimension);
                        webThickness = webThickness.ToSI(UnitType.UT_Section_Dimension);
                        flangeThickness = flangeThickness.ToSI(UnitType.UT_Section_Dimension);
                        rootRadius = rootRadius.ToSI(UnitType.UT_Section_Dimension);
                        toeRadius = toeRadius.ToSI(UnitType.UT_Section_Dimension);
                    }

                    aProfile = BHG.Create.ChannelProfile(height, flangeWidth, webThickness, flangeThickness, rootRadius, toeRadius);
                }
            }

            else if (aTypes.Contains(typeof(ISectionProfile)))
            {
                double height, width, webThickness, flangeThickness, rootRadius, toeRadius;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionIParallelFlange)
                {
                    StructuralSectionIParallelFlange sectionType = section as StructuralSectionIParallelFlange;
                    height = sectionType.Height;
                    width = sectionType.Width;
                    webThickness = sectionType.WebThickness;
                    flangeThickness = sectionType.FlangeThickness;
                    rootRadius = sectionType.WebFillet;
                    toeRadius = sectionType.FlangeFillet;
                }
                else if (section is StructuralSectionIWideFlange)
                {
                    StructuralSectionIWideFlange sectionType = section as StructuralSectionIWideFlange;
                    height = sectionType.Height;
                    width = sectionType.Width;
                    webThickness = sectionType.WebThickness;
                    flangeThickness = sectionType.FlangeThickness;
                    rootRadius = sectionType.WebFillet;
                    toeRadius = 0;
                }
                else
                {
                    height = familySymbol.LookupDouble(heightNames, false);
                    width = familySymbol.LookupDouble(widthNames, false);
                    webThickness = familySymbol.LookupDouble(webThicknessNames, false);
                    flangeThickness = familySymbol.LookupDouble(flangeThicknessNames, false);
                    rootRadius = familySymbol.LookupDouble(rootRadiusNames, false);
                    toeRadius = familySymbol.LookupDouble(toeRadiusNames, false);
                }

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness))
                {
                    if (pullSettings.ConvertUnits)
                    {
                        height = height.ToSI(UnitType.UT_Section_Dimension);
                        width = width.ToSI(UnitType.UT_Section_Dimension);
                        webThickness = webThickness.ToSI(UnitType.UT_Section_Dimension);
                        flangeThickness = flangeThickness.ToSI(UnitType.UT_Section_Dimension);
                        rootRadius = rootRadius.ToSI(UnitType.UT_Section_Dimension);
                        toeRadius = toeRadius.ToSI(UnitType.UT_Section_Dimension);
                    }

                    aProfile = BHG.Create.ISectionProfile(height, width, webThickness, flangeThickness, rootRadius, toeRadius);
                }
            }

            else if (aTypes.Contains(typeof(TSectionProfile)))
            {
                double height, width, webThickness, flangeThickness, rootRadius, toeRadius;

                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionISplitParallelFlange)
                {
                    StructuralSectionISplitParallelFlange sectionType = section as StructuralSectionISplitParallelFlange;
                    height = sectionType.Height;
                    width = sectionType.Width;
                    webThickness = sectionType.WebThickness;
                    flangeThickness = sectionType.FlangeThickness;
                    rootRadius = sectionType.WebFillet;
                    toeRadius = sectionType.FlangeFillet;
                }
                else if (section is StructuralSectionStructuralTees)
                {
                    StructuralSectionStructuralTees sectionType = section as StructuralSectionStructuralTees;
                    height = sectionType.Height;
                    width = sectionType.Width;
                    webThickness = sectionType.WebThickness;
                    flangeThickness = sectionType.FlangeThickness;
                    rootRadius = sectionType.WebFillet;
                    toeRadius = sectionType.FlangeFillet;
                }
                else if (section is StructuralSectionConcreteT)
                {
                    StructuralSectionConcreteT sectionType = section as StructuralSectionConcreteT;
                    height = sectionType.Height;
                    width = (sectionType.Width + 2 * sectionType.CantileverLength);
                    webThickness = sectionType.Width;
                    flangeThickness = sectionType.CantileverHeight;
                    rootRadius = 0;
                    toeRadius = 0;
                }
                else
                {
                    height = familySymbol.LookupDouble(heightNames, false);
                    width = familySymbol.LookupDouble(widthNames, false);
                    webThickness = familySymbol.LookupDouble(webThicknessNames, false);
                    flangeThickness = familySymbol.LookupDouble(flangeThicknessNames, false);
                    rootRadius = familySymbol.LookupDouble(rootRadiusNames, false);
                    toeRadius = familySymbol.LookupDouble(toeRadiusNames, false);
                }

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness))
                {
                    if (pullSettings.ConvertUnits)
                    {
                        height = height.ToSI(UnitType.UT_Section_Dimension);
                        width = width.ToSI(UnitType.UT_Section_Dimension);
                        webThickness = webThickness.ToSI(UnitType.UT_Section_Dimension);
                        flangeThickness = flangeThickness.ToSI(UnitType.UT_Section_Dimension);
                        rootRadius = rootRadius.ToSI(UnitType.UT_Section_Dimension);
                        toeRadius = toeRadius.ToSI(UnitType.UT_Section_Dimension);
                    }

                    aProfile = BHG.Create.TSectionProfile(height, width, webThickness, flangeThickness, rootRadius, toeRadius);
                }
            }

            else if (aTypes.Contains(typeof(ZSectionProfile)))
            {
                double height, flangeWidth, webThickness, flangeThickness, rootRadius, toeRadius;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionZProfile)
                {
                    StructuralSectionZProfile sectionType = section as StructuralSectionZProfile;
                    height = sectionType.Height;
                    flangeWidth = sectionType.BottomFlangeLength;
                    webThickness = sectionType.WallNominalThickness;
                    flangeThickness = sectionType.WallNominalThickness;
                    rootRadius = sectionType.InnerFillet;
                    toeRadius = 0;
                }
                else
                {
                    height = familySymbol.LookupDouble(heightNames, false);
                    flangeWidth = familySymbol.LookupDouble(widthNames, false);
                    webThickness = familySymbol.LookupDouble(webThicknessNames, false);
                    flangeThickness = familySymbol.LookupDouble(flangeThicknessNames, false);
                    rootRadius = familySymbol.LookupDouble(rootRadiusNames, false);
                    toeRadius = familySymbol.LookupDouble(toeRadiusNames, false);
                }

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(flangeWidth) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness))
                {
                    if (pullSettings.ConvertUnits)
                    {
                        height = height.ToSI(UnitType.UT_Section_Dimension);
                        flangeWidth = flangeWidth.ToSI(UnitType.UT_Section_Dimension);
                        webThickness = webThickness.ToSI(UnitType.UT_Section_Dimension);
                        flangeThickness = flangeThickness.ToSI(UnitType.UT_Section_Dimension);
                        rootRadius = rootRadius.ToSI(UnitType.UT_Section_Dimension);
                        toeRadius = toeRadius.ToSI(UnitType.UT_Section_Dimension);
                    }

                    aProfile = BHG.Create.ZSectionProfile(height, flangeWidth, webThickness, flangeThickness, rootRadius, toeRadius);
                }
            }

            else if (aTypes.Contains(typeof(TubeProfile)))
            {
                double thickness, diameter;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionPipeStandard)
                {
                    StructuralSectionPipeStandard sectionType = section as StructuralSectionPipeStandard;
                    thickness = sectionType.WallNominalThickness;
                    diameter = sectionType.Diameter;
                }
                else if (section is StructuralSectionRoundHSS)
                {
                    StructuralSectionRoundHSS sectionType = section as StructuralSectionRoundHSS;
                    thickness = sectionType.WallNominalThickness;
                    diameter = sectionType.Diameter;
                }
                else
                {
                    thickness = familySymbol.LookupDouble(wallThicknessNames, false);
                    diameter = familySymbol.LookupDouble(diameterNames, false);
                }

                if (!double.IsNaN(diameter) && !double.IsNaN(thickness))
                {
                    if (pullSettings.ConvertUnits)
                    {
                        diameter = diameter.ToSI(UnitType.UT_Section_Dimension);
                        thickness = thickness.ToSI(UnitType.UT_Section_Dimension);
                    }

                    aProfile = BHG.Create.TubeProfile(diameter, thickness);
                }

                double radius = familySymbol.LookupDouble(radiusNames, false);
                if (!double.IsNaN(radius) && !double.IsNaN(thickness))
                {
                    if (pullSettings.ConvertUnits)
                    {
                        radius = radius.ToSI(UnitType.UT_Section_Dimension);
                        thickness = thickness.ToSI(UnitType.UT_Section_Dimension);
                    }

                    aProfile = BHG.Create.TubeProfile(radius * 2, thickness);
                }
            }
            
            if (aProfile == null) aProfile = new FreeFormProfile(new List<oM.Geometry.ICurve>());

            aProfile = Modify.SetIdentifiers(aProfile, familySymbol) as IProfile;
            if (pullSettings.CopyCustomData)
                aProfile = Modify.SetCustomData(aProfile, familySymbol, pullSettings.ConvertUnits) as IProfile;
            aProfile.Name = familySymbol.Name;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aProfile);

            return aProfile;
        }


        /***************************************************/
        /****              Private helpers              ****/
        /***************************************************/

        private static string[] diameterNames = { "BHE_Diameter", "Diameter", "d", "D", "OD" };
        private static string[] radiusNames = { "BHE_Radius", "Radius", "r", "R" };
        private static string[] heightNames = { "BHE_Height", "BHE_Depth", "Height", "Depth", "d", "h", "D", "H", "Ht", "b" };
        private static string[] widthNames = { "b", "BHE_Width", "Width", "w", "B", "W", "bf", "D" };
        private static string[] cornerRadiusNames = { "Corner Radius", "r", "r1" };
        private static string[] topFlangeWidthNames = { "Top Flange Width", "bt", "bf_t", "bft", "b1", "b", "B", "Bt" };
        private static string[] botFlangeWidthNames = { "Bottom Flange Width", "bb", "bf_b", "bfb", "b2", "b", "B", "Bb" };
        private static string[] webThicknessNames = { "Web Thickness", "Stem Width", "tw", "t", "T" };
        private static string[] topFlangeThicknessNames = { "Top Flange Thickness", "tft", "tf_t", "tf", "T", "t" };
        private static string[] botFlangeThicknessNames = { "Bottom Flange Thickness", "tfb", "tf_b", "tf", "T", "t" };
        private static string[] flangeThicknessNames = { "Flange Thickness", "Slab Depth", "tf", "T", "t" };
        private static string[] weldSizeNames1 = { "Weld Size" };                                            // weld size, diagonal
        private static string[] weldSizeNames2 = { "k" };                                                    // weld size counted from bar's vertical axis
        private static string[] rootRadiusNames = { "Web Fillet", "Root Radius", "r", "r1", "tr", "kr", "R1", "R", "t" };
        private static string[] toeRadiusNames = { "Flange Fillet", "Toe Radius", "r2", "R2", "t" };
        private static string[] innerRadiusNames = { "Inner Fillet", "Inner Radius", "r1", "R1", "ri", "t" };
        private static string[] outerRadiusNames = { "Outer Fillet", "Outer Radius", "r2", "R2", "ro", "tr" };
        private static string[] wallThicknessNames = { "Wall Nominal Thickness", "Wall Thickness", "t", "T" };

        /***************************************************/
    }
}