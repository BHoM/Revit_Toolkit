using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure.StructuralSections;
using BH.oM.Adapters.Revit;
using BH.oM.Structural.Properties;
using System;
using System.Collections.Generic;
using BHS = BH.Engine.Structure;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static IProfile ToBHoMProfile(this FamilySymbol familySymbol, PullSettings pullSettings = null)
        {
            if (pullSettings == null)
                pullSettings = PullSettings.Default;

            IProfile aProfile = null;

            string familyName = familySymbol.Family.Name;
            Parameter sectionShapeParam = familySymbol.LookupParameter("Section Shape");
            StructuralSectionShape sectionShape = sectionShapeParam == null ? sectionShape = StructuralSectionShape.NotDefined : (StructuralSectionShape)sectionShapeParam.AsInteger();
            
            List<Type> aTypes = Query.BHoMTypes(sectionShape);
            if (aTypes.Count == 0) aTypes.AddRange(Query.BHoMTypes(familyName));

            if (aTypes.Contains(typeof(CircleProfile)))
            {
                double diameter;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionConcreteRound) diameter = (section as StructuralSectionConcreteRound).Diameter;
                else if (section is StructuralSectionConcreteRound) diameter = (section as StructuralSectionConcreteRound).Diameter;
                else diameter = familySymbol.LookupParameterDouble(diameterNames, false);

                if (!double.IsNaN(diameter))
                {
                    if (pullSettings.ConvertUnits) diameter = diameter.ToSI(UnitType.UT_Section_Dimension);
                    aProfile = BHS.Create.CircleProfile(diameter);
                }
                else
                {
                    double radius = familySymbol.LookupParameterDouble(radiusNames, pullSettings.ConvertUnits);
                    if (!double.IsNaN(radius))
                    {
                        aProfile = BHS.Create.CircleProfile(radius * 2);
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
                    height = familySymbol.LookupParameterDouble(heightNames, false);
                    topFlangeWidth = familySymbol.LookupParameterDouble(topFlangeWidthNames, false);
                    botFlangeWidth = familySymbol.LookupParameterDouble(botFlangeWidthNames, false);
                    webThickness = familySymbol.LookupParameterDouble(webThicknessNames, false);
                    topFlangeThickness = familySymbol.LookupParameterDouble(topFlangeThicknessNames, false);
                    botFlangeThickness = familySymbol.LookupParameterDouble(botFlangeThicknessNames, false);
                    weldSize = familySymbol.LookupParameterDouble(weldSizeNames1, false);
                }

                if (double.IsNaN(weldSize))
                {
                    weldSize = familySymbol.LookupParameterDouble(weldSizeNames2, false);
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

                    aProfile = BHS.Create.FabricatedISectionProfile(height, topFlangeWidth, botFlangeWidth, webThickness, topFlangeThickness, botFlangeThickness, weldSize);
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
                    height = familySymbol.LookupParameterDouble(heightNames, false);
                    width = familySymbol.LookupParameterDouble(widthNames, false);
                    cornerRadius = familySymbol.LookupParameterDouble(cornerRadiusNames, false);
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

                    aProfile = BHS.Create.RectangleProfile(height, width, cornerRadius);
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
                    height = familySymbol.LookupParameterDouble(heightNames, false);
                    width = familySymbol.LookupParameterDouble(widthNames, false);
                    webThickness = familySymbol.LookupParameterDouble(webThicknessNames, false);
                    flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, false);
                    rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, false);
                    toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, false);
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

                    aProfile = BHS.Create.AngleProfile(height, width, webThickness, flangeThickness, rootRadius, toeRadius);
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
                    height = familySymbol.LookupParameterDouble(heightNames, false);
                    width = familySymbol.LookupParameterDouble(widthNames, false);
                    thickness = familySymbol.LookupParameterDouble(wallThicknessNames, false);
                    outerRadius = familySymbol.LookupParameterDouble(outerRadiusNames, false);
                    innerRadius = familySymbol.LookupParameterDouble(innerRadiusNames, false);
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

                    aProfile = BHS.Create.BoxProfile(height, width, thickness, outerRadius, innerRadius);
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
                    height = familySymbol.LookupParameterDouble(heightNames, false);
                    flangeWidth = familySymbol.LookupParameterDouble(widthNames, false);
                    webThickness = familySymbol.LookupParameterDouble(webThicknessNames, false);
                    flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, false);
                    rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, false);
                    toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, false);
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

                    aProfile = BHS.Create.ChannelProfile(height, flangeWidth, webThickness, flangeThickness, rootRadius, toeRadius);
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
                    height = familySymbol.LookupParameterDouble(heightNames, false);
                    width = familySymbol.LookupParameterDouble(widthNames, false);
                    webThickness = familySymbol.LookupParameterDouble(webThicknessNames, false);
                    flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, false);
                    rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, false);
                    toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, false);
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

                    aProfile = BHS.Create.ISectionProfile(height, width, webThickness, flangeThickness, rootRadius, toeRadius);
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
                    height = familySymbol.LookupParameterDouble(heightNames, false);
                    width = familySymbol.LookupParameterDouble(widthNames, false);
                    webThickness = familySymbol.LookupParameterDouble(webThicknessNames, false);
                    flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, false);
                    rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, false);
                    toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, false);
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

                    aProfile = BHS.Create.TSectionProfile(height, width, webThickness, flangeThickness, rootRadius, toeRadius);
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
                    height = familySymbol.LookupParameterDouble(heightNames, false);
                    flangeWidth = familySymbol.LookupParameterDouble(widthNames, false);
                    webThickness = familySymbol.LookupParameterDouble(webThicknessNames, false);
                    flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, false);
                    rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, false);
                    toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, false);
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

                    aProfile = BHS.Create.ZSectionProfile(height, flangeWidth, webThickness, flangeThickness, rootRadius, toeRadius);
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
                    thickness = familySymbol.LookupParameterDouble(wallThicknessNames, false);
                    diameter = familySymbol.LookupParameterDouble(diameterNames, false);
                }

                if (!double.IsNaN(diameter) && !double.IsNaN(thickness))
                {
                    if (pullSettings.ConvertUnits)
                    {
                        diameter = diameter.ToSI(UnitType.UT_Section_Dimension);
                        thickness = thickness.ToSI(UnitType.UT_Section_Dimension);
                    }

                    aProfile = BHS.Create.TubeProfile(diameter, thickness);
                }

                double radius = familySymbol.LookupParameterDouble(radiusNames, false);
                if (!double.IsNaN(radius) && !double.IsNaN(thickness))
                {
                    if (pullSettings.ConvertUnits)
                    {
                        radius = radius.ToSI(UnitType.UT_Section_Dimension);
                        thickness = thickness.ToSI(UnitType.UT_Section_Dimension);
                    }

                    aProfile = BHS.Create.TubeProfile(radius * 2, thickness);
                }
            }
            
            if (aProfile == null) aProfile = new FreeFormProfile(new List<oM.Geometry.ICurve>());

            aProfile = Modify.SetIdentifiers(aProfile, familySymbol) as IProfile;
            if (pullSettings.CopyCustomData)
                aProfile = Modify.SetCustomData(aProfile, familySymbol, pullSettings.ConvertUnits) as IProfile;

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
