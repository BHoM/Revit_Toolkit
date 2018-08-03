using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure.StructuralSections;
using BH.oM.Structural.Properties;
using System;
using System.Collections.Generic;
using BHS = BH.Engine.Structure;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static IProfile ToBHoMProfile(this FamilySymbol familySymbol)
        {
            string familyName = familySymbol.Family.Name;
            Parameter sectionShapeParam = familySymbol.LookupParameter("Section Shape");
            StructuralSectionShape sectionShape = sectionShapeParam == null ? sectionShape = StructuralSectionShape.NotDefined : (StructuralSectionShape)sectionShapeParam.AsInteger();
            
            List<Type> aTypes = Engine.Revit.Query.BHoMTypes(sectionShape);

            if (aTypes.Count == 0) aTypes.AddRange(Engine.Revit.Query.BHoMTypes(familyName));
            if (aTypes.Count == 0) return null;

            if (aTypes.Contains(typeof(CircleProfile)))
            {
                double diameter;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionConcreteRound) diameter = (section as StructuralSectionConcreteRound).Diameter.ToSI(UnitType.UT_Section_Dimension);
                else if (section is StructuralSectionConcreteRound) diameter = (section as StructuralSectionConcreteRound).Diameter.ToSI(UnitType.UT_Section_Dimension);
                else diameter = familySymbol.LookupParameterDouble(diameterNames, true);

                if (!double.IsNaN(diameter))
                {
                    return BHS.Create.CircleProfile(diameter);
                }

                double radius = familySymbol.LookupParameterDouble(radiusNames, true);
                if (!double.IsNaN(radius))
                {
                    return BHS.Create.CircleProfile(radius * 2);
                }
            }

            else if (aTypes.Contains(typeof(FabricatedISectionProfile)))
            {
                double height, topFlangeWidth, botFlangeWidth, webThickness, topFlangeThickness, botFlangeThickness, weldSize;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionIWelded)
                {
                    StructuralSectionIWelded sectionType = section as StructuralSectionIWelded;
                    height = sectionType.Height.ToSI(UnitType.UT_Section_Dimension);
                    topFlangeWidth = sectionType.TopFlangeWidth.ToSI(UnitType.UT_Section_Dimension);
                    botFlangeWidth = sectionType.BottomFlangeWidth.ToSI(UnitType.UT_Section_Dimension);
                    webThickness = sectionType.WebThickness.ToSI(UnitType.UT_Section_Dimension);
                    topFlangeThickness = sectionType.TopFlangeThickness.ToSI(UnitType.UT_Section_Dimension);
                    botFlangeThickness = sectionType.BottomFlangeThickness.ToSI(UnitType.UT_Section_Dimension);
                    weldSize = 0;
                }
                else
                {
                    height = familySymbol.LookupParameterDouble(heightNames, true);
                    topFlangeWidth = familySymbol.LookupParameterDouble(topFlangeWidthNames, true);
                    botFlangeWidth = familySymbol.LookupParameterDouble(botFlangeWidthNames, true);
                    webThickness = familySymbol.LookupParameterDouble(webThicknessNames, true);
                    topFlangeThickness = familySymbol.LookupParameterDouble(topFlangeThicknessNames, true);
                    botFlangeThickness = familySymbol.LookupParameterDouble(botFlangeThicknessNames, true);
                    weldSize = familySymbol.LookupParameterDouble(weldSizeNames1, true);
                }

                if (double.IsNaN(weldSize))
                {
                    weldSize = familySymbol.LookupParameterDouble(weldSizeNames2, true);
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
                    return BHS.Create.FabricatedISectionProfile(height, topFlangeWidth, botFlangeWidth, webThickness, topFlangeThickness, botFlangeThickness, weldSize);
                }
            }

            else if (aTypes.Contains(typeof(RectangleProfile)))
            {
                double height, width, cornerRadius;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionConcreteRectangle)
                {
                    StructuralSectionConcreteRectangle sectionType = section as StructuralSectionConcreteRectangle;
                    height = sectionType.Height.ToSI(UnitType.UT_Section_Dimension);
                    width = sectionType.Width.ToSI(UnitType.UT_Section_Dimension);
                    cornerRadius = 0;
                }
                else if (section is StructuralSectionRectangularBar)
                {
                    StructuralSectionRectangularBar sectionType = section as StructuralSectionRectangularBar;
                    height = sectionType.Height.ToSI(UnitType.UT_Section_Dimension);
                    width = sectionType.Width.ToSI(UnitType.UT_Section_Dimension);
                    cornerRadius = 0;
                }
                else if (section is StructuralSectionRectangleParameterized)
                {
                    StructuralSectionRectangleParameterized sectionType = section as StructuralSectionRectangleParameterized;
                    height = sectionType.Height.ToSI(UnitType.UT_Section_Dimension);
                    width = sectionType.Width.ToSI(UnitType.UT_Section_Dimension);
                    cornerRadius = 0;
                }
                else
                {
                    height = familySymbol.LookupParameterDouble(heightNames, true);
                    width = familySymbol.LookupParameterDouble(widthNames, true);
                    cornerRadius = familySymbol.LookupParameterDouble(cornerRadiusNames, true);
                }

                if (double.IsNaN(cornerRadius)) cornerRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(width))
                {
                    return BHS.Create.RectangleProfile(height, width, cornerRadius);
                }
            }

            else if (aTypes.Contains(typeof(AngleProfile)))
            {
                double height, width, webThickness, flangeThickness, rootRadius, toeRadius;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionLAngle)
                {
                    StructuralSectionLAngle sectionType = section as StructuralSectionLAngle;
                    height = sectionType.Height.ToSI(UnitType.UT_Section_Dimension);
                    width = sectionType.Width.ToSI(UnitType.UT_Section_Dimension);
                    webThickness = sectionType.WebThickness.ToSI(UnitType.UT_Section_Dimension);
                    flangeThickness = sectionType.FlangeThickness.ToSI(UnitType.UT_Section_Dimension);
                    rootRadius = sectionType.WebFillet.ToSI(UnitType.UT_Section_Dimension);
                    toeRadius = sectionType.FlangeFillet.ToSI(UnitType.UT_Section_Dimension);
                }
                else if (section is StructuralSectionLProfile)
                {
                    //TODO: Implement cold-formed profiles?
                    StructuralSectionLProfile sectionType = section as StructuralSectionLProfile;
                    height = sectionType.Height.ToSI(UnitType.UT_Section_Dimension);
                    width = sectionType.Width.ToSI(UnitType.UT_Section_Dimension);
                    webThickness = sectionType.WallNominalThickness.ToSI(UnitType.UT_Section_Dimension);
                    flangeThickness = sectionType.WallNominalThickness.ToSI(UnitType.UT_Section_Dimension);
                    rootRadius = sectionType.InnerFillet.ToSI(UnitType.UT_Section_Dimension);
                    toeRadius = 0;
                }
                else
                {
                    height = familySymbol.LookupParameterDouble(heightNames, true);
                    width = familySymbol.LookupParameterDouble(widthNames, true);
                    webThickness = familySymbol.LookupParameterDouble(webThicknessNames, true);
                    flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, true);
                    rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, true);
                    toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, true);
                }

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness) && !double.IsNaN(rootRadius) && !double.IsNaN(toeRadius))
                {
                    return BHS.Create.AngleProfile(height, width, webThickness, flangeThickness, rootRadius, toeRadius);
                }
            }

            else if (aTypes.Contains(typeof(BoxProfile)))
            {
                double height, width, thickness, outerRadius, innerRadius;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionRectangleHSS)
                {
                    StructuralSectionRectangleHSS sectionType = section as StructuralSectionRectangleHSS;
                    height = sectionType.Height.ToSI(UnitType.UT_Section_Dimension);
                    width = sectionType.Width.ToSI(UnitType.UT_Section_Dimension);
                    thickness = sectionType.WallNominalThickness.ToSI(UnitType.UT_Section_Dimension);
                    outerRadius = sectionType.OuterFillet.ToSI(UnitType.UT_Section_Dimension);
                    innerRadius = sectionType.InnerFillet.ToSI(UnitType.UT_Section_Dimension);
                }
                else
                {
                    height = familySymbol.LookupParameterDouble(heightNames, true);
                    width = familySymbol.LookupParameterDouble(widthNames, true);
                    thickness = familySymbol.LookupParameterDouble(wallThicknessNames, true);
                    outerRadius = familySymbol.LookupParameterDouble(outerRadiusNames, true);
                    innerRadius = familySymbol.LookupParameterDouble(innerRadiusNames, true);
                }

                if (double.IsNaN(outerRadius)) outerRadius = 0;
                if (double.IsNaN(innerRadius)) innerRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(thickness) && !double.IsNaN(outerRadius) && !double.IsNaN(innerRadius))
                {
                    return BHS.Create.BoxProfile(height, width, thickness, outerRadius, innerRadius);
                }
            }

            else if (aTypes.Contains(typeof(ChannelProfile)))
            {
                double height, flangeWidth, webThickness, flangeThickness, rootRadius, toeRadius;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionCParallelFlange)
                {
                    StructuralSectionCParallelFlange sectionType = section as StructuralSectionCParallelFlange;
                    height = sectionType.Height.ToSI(UnitType.UT_Section_Dimension);
                    flangeWidth = sectionType.Width.ToSI(UnitType.UT_Section_Dimension);
                    webThickness = sectionType.WebThickness.ToSI(UnitType.UT_Section_Dimension);
                    flangeThickness = sectionType.FlangeThickness.ToSI(UnitType.UT_Section_Dimension);
                    rootRadius = sectionType.WebFillet.ToSI(UnitType.UT_Section_Dimension);
                    toeRadius = sectionType.FlangeFillet.ToSI(UnitType.UT_Section_Dimension);
                }
                else if (section is StructuralSectionCProfile)
                {
                    //TODO: Implement cold-formed profiles?
                    StructuralSectionCProfile sectionType = section as StructuralSectionCProfile;
                    height = sectionType.Height.ToSI(UnitType.UT_Section_Dimension);
                    flangeWidth = sectionType.Width.ToSI(UnitType.UT_Section_Dimension);
                    webThickness = sectionType.WallNominalThickness.ToSI(UnitType.UT_Section_Dimension);
                    flangeThickness = sectionType.WallNominalThickness.ToSI(UnitType.UT_Section_Dimension);
                    rootRadius = sectionType.InnerFillet.ToSI(UnitType.UT_Section_Dimension);
                    toeRadius = 0;
                }
                else
                {
                    height = familySymbol.LookupParameterDouble(heightNames, true);
                    flangeWidth = familySymbol.LookupParameterDouble(widthNames, true);
                    webThickness = familySymbol.LookupParameterDouble(webThicknessNames, true);
                    flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, true);
                    rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, true);
                    toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, true);
                }

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(flangeWidth) && double.IsNaN(webThickness) && !double.IsNaN(flangeThickness) && !double.IsNaN(rootRadius) && double.IsNaN(toeRadius))
                {
                    return BHS.Create.ChannelProfile(height, flangeWidth, webThickness, flangeThickness, rootRadius, toeRadius);
                }
            }

            else if (aTypes.Contains(typeof(ISectionProfile)))
            {
                double height, width, webThickness, flangeThickness, rootRadius, toeRadius;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionIParallelFlange)
                {
                    StructuralSectionIParallelFlange sectionType = section as StructuralSectionIParallelFlange;
                    height = sectionType.Height.ToSI(UnitType.UT_Section_Dimension);
                    width = sectionType.Width.ToSI(UnitType.UT_Section_Dimension);
                    webThickness = sectionType.WebThickness.ToSI(UnitType.UT_Section_Dimension);
                    flangeThickness = sectionType.FlangeThickness.ToSI(UnitType.UT_Section_Dimension);
                    rootRadius = sectionType.WebFillet.ToSI(UnitType.UT_Section_Dimension);
                    toeRadius = sectionType.FlangeFillet.ToSI(UnitType.UT_Section_Dimension);
                }
                else if (section is StructuralSectionIWideFlange)
                {
                    StructuralSectionIWideFlange sectionType = section as StructuralSectionIWideFlange;
                    height = sectionType.Height.ToSI(UnitType.UT_Section_Dimension);
                    width = sectionType.Width.ToSI(UnitType.UT_Section_Dimension);
                    webThickness = sectionType.WebThickness.ToSI(UnitType.UT_Section_Dimension);
                    flangeThickness = sectionType.FlangeThickness.ToSI(UnitType.UT_Section_Dimension);
                    rootRadius = sectionType.WebFillet.ToSI(UnitType.UT_Section_Dimension);
                    toeRadius = 0;
                }
                else
                {
                    height = familySymbol.LookupParameterDouble(heightNames, true);
                    width = familySymbol.LookupParameterDouble(widthNames, true);
                    webThickness = familySymbol.LookupParameterDouble(webThicknessNames, true);
                    flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, true);
                    rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, true);
                    toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, true);
                }

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness) && !double.IsNaN(rootRadius) && !double.IsNaN(toeRadius))
                {
                    return BHS.Create.ISectionProfile(height, width, webThickness, flangeThickness, rootRadius, toeRadius);
                }
            }

            else if (aTypes.Contains(typeof(TSectionProfile)))
            {
                double height, width, webThickness, flangeThickness, rootRadius, toeRadius;

                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionISplitParallelFlange)
                {
                    StructuralSectionISplitParallelFlange sectionType = section as StructuralSectionISplitParallelFlange;
                    height = sectionType.Height.ToSI(UnitType.UT_Section_Dimension);
                    width = sectionType.Width.ToSI(UnitType.UT_Section_Dimension);
                    webThickness = sectionType.WebThickness.ToSI(UnitType.UT_Section_Dimension);
                    flangeThickness = sectionType.FlangeThickness.ToSI(UnitType.UT_Section_Dimension);
                    rootRadius = sectionType.WebFillet.ToSI(UnitType.UT_Section_Dimension);
                    toeRadius = sectionType.FlangeFillet.ToSI(UnitType.UT_Section_Dimension);
                }
                else if (section is StructuralSectionStructuralTees)
                {
                    StructuralSectionStructuralTees sectionType = section as StructuralSectionStructuralTees;
                    height = sectionType.Height.ToSI(UnitType.UT_Section_Dimension);
                    width = sectionType.Width.ToSI(UnitType.UT_Section_Dimension);
                    webThickness = sectionType.WebThickness.ToSI(UnitType.UT_Section_Dimension);
                    flangeThickness = sectionType.FlangeThickness.ToSI(UnitType.UT_Section_Dimension);
                    rootRadius = sectionType.WebFillet.ToSI(UnitType.UT_Section_Dimension);
                    toeRadius = sectionType.FlangeFillet.ToSI(UnitType.UT_Section_Dimension);
                }
                else if (section is StructuralSectionConcreteT)
                {
                    StructuralSectionConcreteT sectionType = section as StructuralSectionConcreteT;
                    height = sectionType.Height.ToSI(UnitType.UT_Section_Dimension);
                    width = (sectionType.Width + 2 * sectionType.CantileverLength).ToSI(UnitType.UT_Section_Dimension);
                    webThickness = sectionType.Width.ToSI(UnitType.UT_Section_Dimension);
                    flangeThickness = sectionType.CantileverHeight.ToSI(UnitType.UT_Section_Dimension);
                    rootRadius = 0;
                    toeRadius = 0;
                }
                else
                {
                    height = familySymbol.LookupParameterDouble(heightNames, true);
                    width = familySymbol.LookupParameterDouble(widthNames, true);
                    webThickness = familySymbol.LookupParameterDouble(webThicknessNames, true);
                    flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, true);
                    rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, true);
                    toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, true);
                }

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness) && !double.IsNaN(rootRadius) && !double.IsNaN(toeRadius))
                {
                    return BHS.Create.TSectionProfile(height, width, webThickness, flangeThickness, rootRadius, toeRadius);
                }
            }

            else if (aTypes.Contains(typeof(ZSectionProfile)))
            {
                double height, flangeWidth, webThickness, flangeThickness, rootRadius, toeRadius;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionZProfile)
                {
                    StructuralSectionZProfile sectionType = section as StructuralSectionZProfile;
                    height = sectionType.Height.ToSI(UnitType.UT_Section_Dimension);
                    flangeWidth = sectionType.BottomFlangeLength.ToSI(UnitType.UT_Section_Dimension);
                    webThickness = sectionType.WallNominalThickness.ToSI(UnitType.UT_Section_Dimension);
                    flangeThickness = sectionType.WallNominalThickness.ToSI(UnitType.UT_Section_Dimension);
                    rootRadius = sectionType.InnerFillet.ToSI(UnitType.UT_Section_Dimension);
                    toeRadius = 0;
                }
                else
                {
                    height = familySymbol.LookupParameterDouble(heightNames, true);
                    flangeWidth = familySymbol.LookupParameterDouble(widthNames, true);
                    webThickness = familySymbol.LookupParameterDouble(webThicknessNames, true);
                    flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, true);
                    rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, true);
                    toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, true);
                }

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(flangeWidth) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness) && !double.IsNaN(rootRadius) && !double.IsNaN(toeRadius))
                {
                    return BHS.Create.ZSectionProfile(height, flangeWidth, webThickness, flangeThickness, rootRadius, toeRadius);
                }
            }

            else if (aTypes.Contains(typeof(TubeProfile)))
            {
                double thickness, diameter;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionPipeStandard)
                {
                    StructuralSectionPipeStandard sectionType = section as StructuralSectionPipeStandard;
                    thickness = sectionType.WallNominalThickness.ToSI(UnitType.UT_Section_Dimension);
                    diameter = sectionType.Diameter.ToSI(UnitType.UT_Section_Dimension);
                }
                else if (section is StructuralSectionRoundHSS)
                {
                    StructuralSectionRoundHSS sectionType = section as StructuralSectionRoundHSS;
                    thickness = sectionType.WallNominalThickness.ToSI(UnitType.UT_Section_Dimension);
                    diameter = sectionType.Diameter.ToSI(UnitType.UT_Section_Dimension);
                }
                else
                {
                    thickness = familySymbol.LookupParameterDouble(wallThicknessNames, true);
                    diameter = familySymbol.LookupParameterDouble(diameterNames, true);
                }
                if (!double.IsNaN(diameter) && !double.IsNaN(thickness))
                {
                    return BHS.Create.TubeProfile(diameter, thickness);
                }

                double radius = familySymbol.LookupParameterDouble(radiusNames, true);
                if (!double.IsNaN(radius) && !double.IsNaN(thickness))
                {
                    return BHS.Create.TubeProfile(radius * 2, thickness);
                }
            }

            return null;
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
