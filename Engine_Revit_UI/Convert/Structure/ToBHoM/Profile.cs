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
        /****               Public Methods              ****/
        /***************************************************/

        public static IProfile ToBHoMProfile(this FamilySymbol familySymbol, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            IProfile aProfile = pullSettings.FindRefObject<IProfile>(familySymbol.Id.IntegerValue);
            if (aProfile != null)
                return aProfile;

            string familyName = familySymbol.Family.Name;
            Parameter sectionShapeParam = familySymbol.get_Parameter(BuiltInParameter.STRUCTURAL_SECTION_SHAPE);
            StructuralSectionShape sectionShape = sectionShapeParam == null ? sectionShape = StructuralSectionShape.NotDefined : (StructuralSectionShape)sectionShapeParam.AsInteger();
            
            List<Type> aTypes = Query.BHoMTypes(sectionShape).ToList();
            if (aTypes.Count == 0)
                aTypes.AddRange(Query.BHoMTypes(familyName));

            if (aTypes.Contains(typeof(CircleProfile)))
            {
                double diameter;
                StructuralSection section = familySymbol.GetStructuralSection();
                if (section is StructuralSectionConcreteRound)
                    diameter = (section as StructuralSectionConcreteRound).Diameter.ToSI(UnitType.UT_Section_Dimension);
                else if (section is StructuralSectionConcreteRound)
                    diameter = (section as StructuralSectionConcreteRound).Diameter.ToSI(UnitType.UT_Section_Dimension);
                else
                    diameter = familySymbol.LookupDouble(diameterNames);

                if (!double.IsNaN(diameter))
                {
                    aProfile = BHG.Create.CircleProfile(diameter);
                }
                else
                {
                    double radius = familySymbol.LookupDouble(radiusNames);
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
                    height = familySymbol.LookupDouble(heightNames);
                    topFlangeWidth = familySymbol.LookupDouble(topFlangeWidthNames);
                    botFlangeWidth = familySymbol.LookupDouble(botFlangeWidthNames);
                    webThickness = familySymbol.LookupDouble(webThicknessNames);
                    topFlangeThickness = familySymbol.LookupDouble(topFlangeThicknessNames);
                    botFlangeThickness = familySymbol.LookupDouble(botFlangeThicknessNames);
                    weldSize = familySymbol.LookupDouble(weldSizeNames1);
                }

                if (double.IsNaN(weldSize))
                {
                    weldSize = familySymbol.LookupDouble(weldSizeNames2);
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
                    height = familySymbol.LookupDouble(heightNames);
                    width = familySymbol.LookupDouble(widthNames);
                    cornerRadius = familySymbol.LookupDouble(cornerRadiusNames);
                }

                if (double.IsNaN(cornerRadius)) cornerRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width))
                {
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
                    height = familySymbol.LookupDouble(heightNames);
                    width = familySymbol.LookupDouble(widthNames);
                    webThickness = familySymbol.LookupDouble(webThicknessNames);
                    flangeThickness = familySymbol.LookupDouble(flangeThicknessNames);
                    rootRadius = familySymbol.LookupDouble(rootRadiusNames);
                    toeRadius = familySymbol.LookupDouble(toeRadiusNames);
                }

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness))
                {
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
                    height = sectionType.Height.ToSI(UnitType.UT_Section_Dimension);
                    width = sectionType.Width.ToSI(UnitType.UT_Section_Dimension);
                    thickness = sectionType.WallNominalThickness.ToSI(UnitType.UT_Section_Dimension);
                    outerRadius = sectionType.OuterFillet.ToSI(UnitType.UT_Section_Dimension);
                    innerRadius = sectionType.InnerFillet.ToSI(UnitType.UT_Section_Dimension);
                }
                else
                {
                    height = familySymbol.LookupDouble(heightNames);
                    width = familySymbol.LookupDouble(widthNames);
                    thickness = familySymbol.LookupDouble(wallThicknessNames);
                    outerRadius = familySymbol.LookupDouble(outerRadiusNames);
                    innerRadius = familySymbol.LookupDouble(innerRadiusNames);
                }

                if (double.IsNaN(outerRadius)) outerRadius = 0;
                if (double.IsNaN(innerRadius)) innerRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(thickness))
                {
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
                    height = familySymbol.LookupDouble(heightNames);
                    flangeWidth = familySymbol.LookupDouble(widthNames);
                    webThickness = familySymbol.LookupDouble(webThicknessNames);
                    flangeThickness = familySymbol.LookupDouble(flangeThicknessNames);
                    rootRadius = familySymbol.LookupDouble(rootRadiusNames);
                    toeRadius = familySymbol.LookupDouble(toeRadiusNames);
                }

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(flangeWidth) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness))
                {
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
                    height = familySymbol.LookupDouble(heightNames);
                    width = familySymbol.LookupDouble(widthNames);
                    webThickness = familySymbol.LookupDouble(webThicknessNames);
                    flangeThickness = familySymbol.LookupDouble(flangeThicknessNames);
                    rootRadius = familySymbol.LookupDouble(rootRadiusNames);
                    toeRadius = familySymbol.LookupDouble(toeRadiusNames);
                }

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness))
                {
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
                    height = familySymbol.LookupDouble(heightNames);
                    width = familySymbol.LookupDouble(widthNames);
                    webThickness = familySymbol.LookupDouble(webThicknessNames);
                    flangeThickness = familySymbol.LookupDouble(flangeThicknessNames);
                    rootRadius = familySymbol.LookupDouble(rootRadiusNames);
                    toeRadius = familySymbol.LookupDouble(toeRadiusNames);
                }

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness))
                {
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
                    height = sectionType.Height.ToSI(UnitType.UT_Section_Dimension);
                    flangeWidth = sectionType.BottomFlangeLength.ToSI(UnitType.UT_Section_Dimension);
                    webThickness = sectionType.WallNominalThickness.ToSI(UnitType.UT_Section_Dimension);
                    flangeThickness = sectionType.WallNominalThickness.ToSI(UnitType.UT_Section_Dimension);
                    rootRadius = sectionType.InnerFillet.ToSI(UnitType.UT_Section_Dimension);
                    toeRadius = 0;
                }
                else
                {
                    height = familySymbol.LookupDouble(heightNames);
                    flangeWidth = familySymbol.LookupDouble(widthNames);
                    webThickness = familySymbol.LookupDouble(webThicknessNames);
                    flangeThickness = familySymbol.LookupDouble(flangeThicknessNames);
                    rootRadius = familySymbol.LookupDouble(rootRadiusNames);
                    toeRadius = familySymbol.LookupDouble(toeRadiusNames);
                }

                if (double.IsNaN(rootRadius)) rootRadius = 0;
                if (double.IsNaN(toeRadius)) toeRadius = 0;

                if (!double.IsNaN(height) && !double.IsNaN(flangeWidth) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness))
                {
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
                    thickness = familySymbol.LookupDouble(wallThicknessNames);
                    diameter = familySymbol.LookupDouble(diameterNames);
                }

                if (!double.IsNaN(diameter) && !double.IsNaN(thickness))
                {
                    aProfile = BHG.Create.TubeProfile(diameter, thickness);
                }

                double radius = familySymbol.LookupDouble(radiusNames);
                if (!double.IsNaN(radius) && !double.IsNaN(thickness))
                {
                    aProfile = BHG.Create.TubeProfile(radius * 2, thickness);
                }
            }
            
            if (aProfile == null)
                return null;

            aProfile = Modify.SetIdentifiers(aProfile, familySymbol) as IProfile;
            if (pullSettings.CopyCustomData)
                aProfile = Modify.SetCustomData(aProfile, familySymbol) as IProfile;

            aProfile.Name = familySymbol.Name;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aProfile);

            return aProfile;
        }

        /***************************************************/

        public static IProfile BHoMFreeFormProfile(this FamilyInstance familyInstance, PullSettings pullSettings)
        {
            pullSettings = pullSettings.DefaultIfNull();

            IProfile profile = pullSettings.FindRefObject<IProfile>(familyInstance.Symbol.Id.IntegerValue);
            if (profile != null)
                return profile;

            List<oM.Geometry.ICurve> profileCurves = new List<oM.Geometry.ICurve>();

            // Check if one and only one solid exists to make sure that the column is a single piece
            Solid solid = null;
            Options options = new Options();
            options.IncludeNonVisibleObjects = false;
            foreach (GeometryObject obj in familyInstance.Symbol.get_Geometry(options))
            {
                if (obj is Solid)
                {
                    Solid s = obj as Solid;
                    if (!s.Faces.IsEmpty)
                    {
                        if (solid == null)
                            solid = s;
                        else
                        {
                            BH.Engine.Reflection.Compute.RecordWarning("The element consists of more than one solid. ElementId: " + familyInstance.Id.IntegerValue.ToString());
                            return null;
                        }
                    }
                }
            }

            if (solid == null)
            {
                BH.Engine.Reflection.Compute.RecordWarning("The profile of an element could not be found. ElementId: " + familyInstance.Id.IntegerValue.ToString());
                return null;
            }

            //TODO: direction should be driven based on the relation between GetTotalTransform(), handOrientation and facingOrientation?

            BH.oM.Geometry.Point centroid = solid.ComputeCentroid().ToBHoM();

            XYZ direction;
            if (familyInstance.Symbol.Family.FamilyPlacementType == FamilyPlacementType.CurveDrivenStructural)
                direction = XYZ.BasisX;
            else if (familyInstance.Symbol.Family.FamilyPlacementType == FamilyPlacementType.TwoLevelsBased)
                direction = XYZ.BasisZ;
            else
            {
                BH.Engine.Reflection.Compute.RecordWarning("The profile of an element could not be found. ElementId: " + familyInstance.Id.IntegerValue.ToString());
                return null;
            }

            Face face = null;
            foreach (Face f in solid.Faces)
            {
                if (f is PlanarFace && (f as PlanarFace).FaceNormal.Normalize().IsAlmostEqualTo(direction, 0.001))
                {
                    if (face == null)
                        face = f;
                    else
                    {
                        BH.Engine.Reflection.Compute.RecordWarning("The profile of an element could not be found. ElementId: " + familyInstance.Id.IntegerValue.ToString());
                        return null;
                    }
                }
            }

            if (face == null)
            {
                BH.Engine.Reflection.Compute.RecordWarning("The profile of an element could not be found. ElementId: " + familyInstance.Id.IntegerValue.ToString());
                return null;
            }

            foreach (EdgeArray curveArray in (face as PlanarFace).EdgeLoops)
            {
                foreach (Edge c in curveArray)
                {
                    BH.oM.Geometry.ICurve curve = c.AsCurve().ToBHoM();
                    if (curve == null)
                    {
                        BH.Engine.Reflection.Compute.RecordWarning("The profile of an element could not be converted due to curve conversion issues. ElementId: " + familyInstance.Id.IntegerValue.ToString());
                        return null;
                    }
                    profileCurves.Add(curve);
                }
            }

            if (profileCurves.Count != 0)
            {
                // Checking if the curves are in the horizontal plane, if not rotating them.
                BH.oM.Geometry.Vector tan = direction.ToBHoMVector();

                double angle = BH.Engine.Geometry.Query.Angle(tan, BH.oM.Geometry.Vector.ZAxis);
                BH.oM.Geometry.Vector rotAxis = BH.Engine.Geometry.Query.CrossProduct(tan, BH.oM.Geometry.Vector.ZAxis);
                if (angle > BH.oM.Geometry.Tolerance.Angle)
                    profileCurves = profileCurves.Select(x => BH.Engine.Geometry.Modify.IRotate(x, centroid, rotAxis, angle)).ToList();

                // Adjustment of the origin to global (0,0,0)
                BH.oM.Geometry.Vector adjustment = BH.Engine.Geometry.Query.DotProduct(centroid - BH.Engine.Geometry.Query.IStartPoint(profileCurves[0]), BH.oM.Geometry.Vector.ZAxis) * BH.oM.Geometry.Vector.ZAxis;

                if (BH.Engine.Geometry.Query.Distance(centroid, oM.Geometry.Point.Origin) > oM.Geometry.Tolerance.Distance)
                    profileCurves = profileCurves.Select(x => BH.Engine.Geometry.Modify.ITranslate(x, oM.Geometry.Point.Origin - centroid + adjustment)).ToList();

                // Rotation of the profile to align its local Z with global Y.
                angle = BH.Engine.Geometry.Query.Angle(tan, rotAxis);
                if (angle > BH.oM.Geometry.Tolerance.Angle)
                    profileCurves = profileCurves.Select(x => BH.Engine.Geometry.Modify.IRotate(x, centroid, BH.oM.Geometry.Vector.ZAxis, angle)).ToList();
            }

            profile = new FreeFormProfile(profileCurves);

            profile = Modify.SetIdentifiers(profile, familyInstance.Symbol) as IProfile;
            if (pullSettings.CopyCustomData)
                profile = Modify.SetCustomData(profile, familyInstance.Symbol) as IProfile;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(profile);

            return profile;
        }


        /***************************************************/
        /****              Private helpers              ****/
        /***************************************************/

        private static readonly string[] diameterNames = { "BHE_Diameter", "Diameter", "d", "D", "OD" };
        private static readonly string[] radiusNames = { "BHE_Radius", "Radius", "r", "R" };
        private static readonly string[] heightNames = { "BHE_Height", "BHE_Depth", "Height", "Depth", "d", "h", "D", "H", "Ht", "b" };
        private static readonly string[] widthNames = { "BHE_Width", "Width", "b", "B", "bf", "w", "W", "D" };
        private static readonly string[] cornerRadiusNames = { "Corner Radius", "r", "r1" };
        private static readonly string[] topFlangeWidthNames = { "Top Flange Width", "bt", "bf_t", "bft", "b1", "b", "B", "Bt" };
        private static readonly string[] botFlangeWidthNames = { "Bottom Flange Width", "bb", "bf_b", "bfb", "b2", "b", "B", "Bb" };
        private static readonly string[] webThicknessNames = { "Web Thickness", "Stem Width", "tw", "t", "T" };
        private static readonly string[] topFlangeThicknessNames = { "Top Flange Thickness", "tft", "tf_t", "tf", "T", "t" };
        private static readonly string[] botFlangeThicknessNames = { "Bottom Flange Thickness", "tfb", "tf_b", "tf", "T", "t" };
        private static readonly string[] flangeThicknessNames = { "Flange Thickness", "Slab Depth", "tf", "T", "t" };
        private static readonly string[] weldSizeNames1 = { "Weld Size" };                                            // weld size, diagonal
        private static readonly string[] weldSizeNames2 = { "k" };                                                    // weld size counted from bar's vertical axis
        private static readonly string[] rootRadiusNames = { "Web Fillet", "Root Radius", "r", "r1", "tr", "kr", "R1", "R", "t" };
        private static readonly string[] toeRadiusNames = { "Flange Fillet", "Toe Radius", "r2", "R2", "t" };
        private static readonly string[] innerRadiusNames = { "Inner Fillet", "Inner Radius", "r1", "R1", "ri", "t" };
        private static readonly string[] outerRadiusNames = { "Outer Fillet", "Outer Radius", "r2", "R2", "ro", "tr" };
        private static readonly string[] wallThicknessNames = { "Wall Nominal Thickness", "Wall Thickness", "t", "T" };

        /***************************************************/
    }
}