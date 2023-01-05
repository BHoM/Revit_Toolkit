/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using Autodesk.Revit.DB.Structure.StructuralSections;
using BH.Engine.Adapters.Revit;
using BH.Engine.Base;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Geometry;
using BH.oM.Base.Attributes;
using BH.oM.Spatial.ShapeProfiles;
using BH.oM.Structure.SectionProperties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BHS = BH.Engine.Spatial;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/
        
        [Description("Converts a Revit FamilySymbol to BH.oM.Spatial.ShapeProfiles.IProfile.")]
        [Input("familySymbol", "Revit FamilySymbol to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("profile", "BH.oM.Spatial.ShapeProfiles.IProfile resulting from converting the input Revit FamilySymbol.")]
        public static IProfile ProfileFromRevit(this FamilySymbol familySymbol, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            IProfile profile = refObjects.GetValue<IProfile>(familySymbol.Id);
            if (profile != null)
                return profile;

            IGeometricalSection property = BH.Engine.Library.Query.Match("Structure\\SectionProperties", familySymbol.Name) as IGeometricalSection;
            if (property != null)
                profile = property.SectionProfile.DeepClone();

            if (profile == null)
            {
                Parameter sectionShapeParam = familySymbol.get_Parameter(BuiltInParameter.STRUCTURAL_SECTION_SHAPE);
                StructuralSectionShape sectionShape = sectionShapeParam == null ? sectionShape = StructuralSectionShape.NotDefined : (StructuralSectionShape)sectionShapeParam.AsInteger();

                if (sectionShape == StructuralSectionShape.NotDefined)
                    sectionShape = familySymbol.Family.Name.SectionShape();

                switch (sectionShape)
                {
                    case StructuralSectionShape.RoundBar:
                    case StructuralSectionShape.ConcreteRound:
                        profile = familySymbol.CircleProfileFromRevit();
                        break;
                    case StructuralSectionShape.IWelded:
                        profile = familySymbol.FabricatedISectionProfileFromRevit();
                        break;
                    case StructuralSectionShape.RectangleParameterized:
                    case StructuralSectionShape.RectangularBar:
                    case StructuralSectionShape.ConcreteRectangle:
                        profile = familySymbol.RectangleProfileFromRevit();
                        break;
                    case StructuralSectionShape.LAngle:
                    case StructuralSectionShape.LProfile:
                        profile = familySymbol.AngleProfileFromRevit();
                        break;
                    case StructuralSectionShape.RectangleHSS:
                        profile = familySymbol.BoxProfileFromRevit();
                        break;
                    case StructuralSectionShape.CParallelFlange:
                    case StructuralSectionShape.CProfile:
                        profile = familySymbol.ChannelProfileFromRevit();
                        break;
                    case StructuralSectionShape.IParallelFlange:
                    case StructuralSectionShape.IWideFlange:
                        profile = familySymbol.ISectionProfileFromRevit();
                        break;
                    case StructuralSectionShape.ISplitParallelFlange:
                    case StructuralSectionShape.StructuralTees:
                    case StructuralSectionShape.ConcreteT:
                        profile = familySymbol.TSectionProfileFromRevit();
                        break;
                    case StructuralSectionShape.ZProfile:
                        profile = familySymbol.ZSectionProfileFromRevit();
                        break;
                    case StructuralSectionShape.PipeStandard:
                    case StructuralSectionShape.RoundHSS:
                        profile = familySymbol.TubeProfileFromRevit();
                        break;
                }
            }

            if (profile == null)
                profile = familySymbol.FreeFormProfileFromRevit(settings);
            
            if (profile == null)
                return null;

            //Set identifiers, parameters & custom data
            profile.SetIdentifiers(familySymbol);
            profile.CopyParameters(familySymbol, settings.MappingSettings);
            profile.SetProperties(familySymbol, settings.MappingSettings);

            profile.Name = familySymbol.Name;
            refObjects.AddOrReplace(familySymbol.Id, profile);

            return profile;
        }


        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        [Description("Converts a Revit FamilySymbol to BH.oM.Spatial.ShapeProfiles.CircleProfile.")]
        [Input("familySymbol", "Revit FamilySymbol to be converted.")]
        [Output("profile", "BH.oM.Spatial.ShapeProfiles.CircleProfile resulting from converting the input Revit FamilySymbol.")]
        private static CircleProfile CircleProfileFromRevit(this FamilySymbol familySymbol)
        {
            double diameter;
            StructuralSection section = familySymbol.GetStructuralSection();
            if (section is StructuralSectionConcreteRound)
                diameter = (section as StructuralSectionConcreteRound).Diameter.ToSI(SpecTypeId.SectionDimension);
            else if (section is StructuralSectionConcreteRound)
                diameter = (section as StructuralSectionConcreteRound).Diameter.ToSI(SpecTypeId.SectionDimension);
            else
            {
                BH.Engine.Base.Compute.RecordWarning($"Dimensions of section profile {familySymbol.Name} (Revit ElementId {familySymbol.Id.IntegerValue}) were extracted using name matching and may be incorrect in some cases.");
                diameter = familySymbol.LookupParameterDouble(diameterNames, dimensionGroups);
            }

            if (!double.IsNaN(diameter))
                return BHS.Create.CircleProfile(diameter);
            else
            {
                double radius = familySymbol.LookupParameterDouble(radiusNames, dimensionGroups);
                if (!double.IsNaN(radius))
                    return BHS.Create.CircleProfile(radius * 2);
            }

            return null;
        }

        /***************************************************/

        [Description("Converts a Revit FamilySymbol to BH.oM.Spatial.ShapeProfiles.FabricatedISectionProfile.")]
        [Input("familySymbol", "Revit FamilySymbol to be converted.")]
        [Output("profile", "BH.oM.Spatial.ShapeProfiles.FabricatedISectionProfile resulting from converting the input Revit FamilySymbol.")]
        private static FabricatedISectionProfile FabricatedISectionProfileFromRevit(this FamilySymbol familySymbol)
        {
            double height, topFlangeWidth, botFlangeWidth, webThickness, topFlangeThickness, botFlangeThickness, weldSize;
            StructuralSection section = familySymbol.GetStructuralSection();
            if (section is StructuralSectionIWelded)
            {
                StructuralSectionIWelded sectionType = section as StructuralSectionIWelded;
                height = sectionType.Height.ToSI(SpecTypeId.SectionDimension);
                topFlangeWidth = sectionType.TopFlangeWidth.ToSI(SpecTypeId.SectionDimension);
                botFlangeWidth = sectionType.BottomFlangeWidth.ToSI(SpecTypeId.SectionDimension);
                webThickness = sectionType.WebThickness.ToSI(SpecTypeId.SectionDimension);
                topFlangeThickness = sectionType.TopFlangeThickness.ToSI(SpecTypeId.SectionDimension);
                botFlangeThickness = sectionType.BottomFlangeThickness.ToSI(SpecTypeId.SectionDimension);
                weldSize = 0;
            }
            else
            {
                BH.Engine.Base.Compute.RecordWarning($"Dimensions of section profile {familySymbol.Name} (Revit ElementId {familySymbol.Id.IntegerValue}) were extracted using name matching and may be incorrect in some cases.");
                height = familySymbol.LookupParameterDouble(heightNames, dimensionGroups);
                topFlangeWidth = familySymbol.LookupParameterDouble(topFlangeWidthNames, dimensionGroups);
                botFlangeWidth = familySymbol.LookupParameterDouble(botFlangeWidthNames, dimensionGroups);
                webThickness = familySymbol.LookupParameterDouble(webThicknessNames, dimensionGroups);
                topFlangeThickness = familySymbol.LookupParameterDouble(topFlangeThicknessNames, dimensionGroups);
                botFlangeThickness = familySymbol.LookupParameterDouble(botFlangeThicknessNames, dimensionGroups);
                weldSize = familySymbol.LookupParameterDouble(weldSizeNames1, dimensionGroups);
            }

            if (double.IsNaN(weldSize))
            {
                weldSize = familySymbol.LookupParameterDouble(weldSizeNames2, dimensionGroups);
                if (!double.IsNaN(weldSize) && !double.IsNaN(webThickness))
                    weldSize = (weldSize - webThickness / 2) / (Math.Sqrt(2));
                else
                    weldSize = 0;
            }

            if (!double.IsNaN(height) && !double.IsNaN(topFlangeWidth) && !double.IsNaN(botFlangeWidth) && !double.IsNaN(webThickness) && !double.IsNaN(topFlangeThickness) && !double.IsNaN(botFlangeThickness))
                return BHS.Create.FabricatedISectionProfile(height, topFlangeWidth, botFlangeWidth, webThickness, topFlangeThickness, botFlangeThickness, weldSize);

            return null;
        }

        /***************************************************/

        [Description("Converts a Revit FamilySymbol to BH.oM.Spatial.ShapeProfiles.RectangleProfile.")]
        [Input("familySymbol", "Revit FamilySymbol to be converted.")]
        [Output("profile", "BH.oM.Spatial.ShapeProfiles.RectangleProfile resulting from converting the input Revit FamilySymbol.")]
        private static RectangleProfile RectangleProfileFromRevit(this FamilySymbol familySymbol)
        {
            double height, width, cornerRadius;
            StructuralSection section = familySymbol.GetStructuralSection();
            if (section is StructuralSectionConcreteRectangle)
            {
                StructuralSectionConcreteRectangle sectionType = section as StructuralSectionConcreteRectangle;
                height = sectionType.Height.ToSI(SpecTypeId.SectionDimension);
                width = sectionType.Width.ToSI(SpecTypeId.SectionDimension);
                cornerRadius = 0;
            }
            else if (section is StructuralSectionRectangularBar)
            {
                StructuralSectionRectangularBar sectionType = section as StructuralSectionRectangularBar;
                height = sectionType.Height.ToSI(SpecTypeId.SectionDimension);
                width = sectionType.Width.ToSI(SpecTypeId.SectionDimension);
                cornerRadius = 0;
            }
            else if (section is StructuralSectionRectangleParameterized)
            {
                StructuralSectionRectangleParameterized sectionType = section as StructuralSectionRectangleParameterized;
                height = sectionType.Height.ToSI(SpecTypeId.SectionDimension);
                width = sectionType.Width.ToSI(SpecTypeId.SectionDimension);
                cornerRadius = 0;
            }
            else
            {
                BH.Engine.Base.Compute.RecordWarning($"Dimensions of section profile {familySymbol.Name} (Revit ElementId {familySymbol.Id.IntegerValue}) were extracted using name matching and may be incorrect in some cases.");
                height = familySymbol.LookupParameterDouble(heightNames, dimensionGroups);
                width = familySymbol.LookupParameterDouble(widthNames, dimensionGroups);
                cornerRadius = familySymbol.LookupParameterDouble(cornerRadiusNames, dimensionGroups);
            }

            if (double.IsNaN(cornerRadius))
                cornerRadius = 0;

            if (!double.IsNaN(height) && !double.IsNaN(width))
                return BHS.Create.RectangleProfile(height, width, cornerRadius);

            return null;
        }

        /***************************************************/

        [Description("Converts a Revit FamilySymbol to BH.oM.Spatial.ShapeProfiles.AngleProfile.")]
        [Input("familySymbol", "Revit FamilySymbol to be converted.")]
        [Output("profile", "BH.oM.Spatial.ShapeProfiles.AngleProfile resulting from converting the input Revit FamilySymbol.")]
        private static AngleProfile AngleProfileFromRevit(this FamilySymbol familySymbol)
        {
            double height, width, webThickness, flangeThickness, rootRadius, toeRadius;
            StructuralSection section = familySymbol.GetStructuralSection();
            if (section is StructuralSectionLAngle)
            {
                StructuralSectionLAngle sectionType = section as StructuralSectionLAngle;
                height = sectionType.Height.ToSI(SpecTypeId.SectionDimension);
                width = sectionType.Width.ToSI(SpecTypeId.SectionDimension);
                webThickness = sectionType.WebThickness.ToSI(SpecTypeId.SectionDimension);
                flangeThickness = sectionType.FlangeThickness.ToSI(SpecTypeId.SectionDimension);
                rootRadius = sectionType.WebFillet.ToSI(SpecTypeId.SectionDimension);
                toeRadius = sectionType.FlangeFillet.ToSI(SpecTypeId.SectionDimension);
            }
            else if (section is StructuralSectionLProfile)
            {
                //TODO: Implement cold-formed profiles?
                StructuralSectionLProfile sectionType = section as StructuralSectionLProfile;
                height = sectionType.Height.ToSI(SpecTypeId.SectionDimension);
                width = sectionType.Width.ToSI(SpecTypeId.SectionDimension);
                webThickness = sectionType.WallNominalThickness.ToSI(SpecTypeId.SectionDimension);
                flangeThickness = sectionType.WallNominalThickness.ToSI(SpecTypeId.SectionDimension);
                rootRadius = sectionType.InnerFillet.ToSI(SpecTypeId.SectionDimension);
                toeRadius = 0;
            }
            else
            {
                BH.Engine.Base.Compute.RecordWarning($"Dimensions of section profile {familySymbol.Name} (Revit ElementId {familySymbol.Id.IntegerValue}) were extracted using name matching and may be incorrect in some cases.");
                height = familySymbol.LookupParameterDouble(heightNames, dimensionGroups);
                width = familySymbol.LookupParameterDouble(widthNames, dimensionGroups);
                webThickness = familySymbol.LookupParameterDouble(webThicknessNames, dimensionGroups);
                flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, dimensionGroups);
                rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, dimensionGroups);
                toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, dimensionGroups);
            }

            if (double.IsNaN(rootRadius))
                rootRadius = 0;

            if (double.IsNaN(toeRadius))
                toeRadius = 0;

            if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness))
                return BHS.Create.AngleProfile(height, width, webThickness, flangeThickness, rootRadius, toeRadius);

            return null;
        }

        /***************************************************/

        [Description("Converts a Revit FamilySymbol to BH.oM.Spatial.ShapeProfiles.BoxProfile.")]
        [Input("familySymbol", "Revit FamilySymbol to be converted.")]
        [Output("profile", "BH.oM.Spatial.ShapeProfiles.BoxProfile resulting from converting the input Revit FamilySymbol.")]
        private static BoxProfile BoxProfileFromRevit(this FamilySymbol familySymbol)
        {
            double height, width, thickness, outerRadius, innerRadius;
            StructuralSection section = familySymbol.GetStructuralSection();
            if (section is StructuralSectionRectangleHSS)
            {
                StructuralSectionRectangleHSS sectionType = section as StructuralSectionRectangleHSS;
                height = sectionType.Height.ToSI(SpecTypeId.SectionDimension);
                width = sectionType.Width.ToSI(SpecTypeId.SectionDimension);
                thickness = sectionType.WallNominalThickness.ToSI(SpecTypeId.SectionDimension);
                outerRadius = sectionType.OuterFillet.ToSI(SpecTypeId.SectionDimension);
                innerRadius = sectionType.InnerFillet.ToSI(SpecTypeId.SectionDimension);
            }
            else
            {
                BH.Engine.Base.Compute.RecordWarning($"Dimensions of section profile {familySymbol.Name} (Revit ElementId {familySymbol.Id.IntegerValue}) were extracted using name matching and may be incorrect in some cases.");
                height = familySymbol.LookupParameterDouble(heightNames, dimensionGroups);
                width = familySymbol.LookupParameterDouble(widthNames, dimensionGroups);
                thickness = familySymbol.LookupParameterDouble(wallThicknessNames, dimensionGroups);
                outerRadius = familySymbol.LookupParameterDouble(outerRadiusNames, dimensionGroups);
                innerRadius = familySymbol.LookupParameterDouble(innerRadiusNames, dimensionGroups);
            }

            if (double.IsNaN(outerRadius))
                outerRadius = 0;

            if (double.IsNaN(innerRadius))
                innerRadius = 0;

            if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(thickness))
                return BHS.Create.BoxProfile(height, width, thickness, outerRadius, innerRadius);

            return null;
        }

        /***************************************************/

        [Description("Converts a Revit FamilySymbol to BH.oM.Spatial.ShapeProfiles.ChannelProfile.")]
        [Input("familySymbol", "Revit FamilySymbol to be converted.")]
        [Output("profile", "BH.oM.Spatial.ShapeProfiles.ChannelProfile resulting from converting the input Revit FamilySymbol.")]
        private static ChannelProfile ChannelProfileFromRevit(this FamilySymbol familySymbol)
        {
            double height, flangeWidth, webThickness, flangeThickness, rootRadius, toeRadius;
            StructuralSection section = familySymbol.GetStructuralSection();
            if (section is StructuralSectionCParallelFlange)
            {
                StructuralSectionCParallelFlange sectionType = section as StructuralSectionCParallelFlange;
                height = sectionType.Height.ToSI(SpecTypeId.SectionDimension);
                flangeWidth = sectionType.Width.ToSI(SpecTypeId.SectionDimension);
                webThickness = sectionType.WebThickness.ToSI(SpecTypeId.SectionDimension);
                flangeThickness = sectionType.FlangeThickness.ToSI(SpecTypeId.SectionDimension);
                rootRadius = sectionType.WebFillet.ToSI(SpecTypeId.SectionDimension);
                toeRadius = sectionType.FlangeFillet.ToSI(SpecTypeId.SectionDimension);
            }
            else if (section is StructuralSectionCProfile)
            {
                //TODO: Implement cold-formed profiles?
                StructuralSectionCProfile sectionType = section as StructuralSectionCProfile;
                height = sectionType.Height.ToSI(SpecTypeId.SectionDimension);
                flangeWidth = sectionType.Width.ToSI(SpecTypeId.SectionDimension);
                webThickness = sectionType.WallNominalThickness.ToSI(SpecTypeId.SectionDimension);
                flangeThickness = sectionType.WallNominalThickness.ToSI(SpecTypeId.SectionDimension);
                rootRadius = sectionType.InnerFillet.ToSI(SpecTypeId.SectionDimension);
                toeRadius = 0;
            }
            else
            {
                BH.Engine.Base.Compute.RecordWarning($"Dimensions of section profile {familySymbol.Name} (Revit ElementId {familySymbol.Id.IntegerValue}) were extracted using name matching and may be incorrect in some cases.");
                height = familySymbol.LookupParameterDouble(heightNames, dimensionGroups);
                flangeWidth = familySymbol.LookupParameterDouble(widthNames, dimensionGroups);
                webThickness = familySymbol.LookupParameterDouble(webThicknessNames, dimensionGroups);
                flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, dimensionGroups);
                rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, dimensionGroups);
                toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, dimensionGroups);
            }

            if (double.IsNaN(rootRadius))
                rootRadius = 0;

            if (double.IsNaN(toeRadius))
                toeRadius = 0;

            if (!double.IsNaN(height) && !double.IsNaN(flangeWidth) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness))
                return BHS.Create.ChannelProfile(height, flangeWidth, webThickness, flangeThickness, rootRadius, toeRadius);

            return null;
        }

        /***************************************************/

        [Description("Converts a Revit FamilySymbol to BH.oM.Spatial.ShapeProfiles.ISectionProfile.")]
        [Input("familySymbol", "Revit FamilySymbol to be converted.")]
        [Output("profile", "BH.oM.Spatial.ShapeProfiles.ISectionProfile resulting from converting the input Revit FamilySymbol.")]
        private static ISectionProfile ISectionProfileFromRevit(this FamilySymbol familySymbol)
        {
            double height, width, webThickness, flangeThickness, rootRadius, toeRadius;
            StructuralSection section = familySymbol.GetStructuralSection();
            if (section is StructuralSectionIParallelFlange)
            {
                StructuralSectionIParallelFlange sectionType = section as StructuralSectionIParallelFlange;
                height = sectionType.Height.ToSI(SpecTypeId.SectionDimension);
                width = sectionType.Width.ToSI(SpecTypeId.SectionDimension);
                webThickness = sectionType.WebThickness.ToSI(SpecTypeId.SectionDimension);
                flangeThickness = sectionType.FlangeThickness.ToSI(SpecTypeId.SectionDimension);
                rootRadius = sectionType.WebFillet.ToSI(SpecTypeId.SectionDimension);
                toeRadius = sectionType.FlangeFillet.ToSI(SpecTypeId.SectionDimension);
            }
            else if (section is StructuralSectionIWideFlange)
            {
                StructuralSectionIWideFlange sectionType = section as StructuralSectionIWideFlange;
                height = sectionType.Height.ToSI(SpecTypeId.SectionDimension);
                width = sectionType.Width.ToSI(SpecTypeId.SectionDimension);
                webThickness = sectionType.WebThickness.ToSI(SpecTypeId.SectionDimension);
                flangeThickness = sectionType.FlangeThickness.ToSI(SpecTypeId.SectionDimension);
                rootRadius = sectionType.WebFillet.ToSI(SpecTypeId.SectionDimension);
                toeRadius = 0;
            }
            else
            {
                BH.Engine.Base.Compute.RecordWarning($"Dimensions of section profile {familySymbol.Name} (Revit ElementId {familySymbol.Id.IntegerValue}) were extracted using name matching and may be incorrect in some cases.");
                height = familySymbol.LookupParameterDouble(heightNames, dimensionGroups);
                width = familySymbol.LookupParameterDouble(widthNames, dimensionGroups);
                webThickness = familySymbol.LookupParameterDouble(webThicknessNames, dimensionGroups);
                flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, dimensionGroups);
                rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, dimensionGroups);
                toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, dimensionGroups);
            }

            if (double.IsNaN(rootRadius))
                rootRadius = 0;

            if (double.IsNaN(toeRadius))
                toeRadius = 0;

            if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness))
                return BHS.Create.ISectionProfile(height, width, webThickness, flangeThickness, rootRadius, toeRadius);

            return null;
        }

        /***************************************************/

        [Description("Converts a Revit FamilySymbol to BH.oM.Spatial.ShapeProfiles.TSectionProfile.")]
        [Input("familySymbol", "Revit FamilySymbol to be converted.")]
        [Output("profile", "BH.oM.Spatial.ShapeProfiles.TSectionProfile resulting from converting the input Revit FamilySymbol.")]
        private static TSectionProfile TSectionProfileFromRevit(this FamilySymbol familySymbol)
        {
            double height, width, webThickness, flangeThickness, rootRadius, toeRadius;

            StructuralSection section = familySymbol.GetStructuralSection();
            if (section is StructuralSectionISplitParallelFlange)
            {
                StructuralSectionISplitParallelFlange sectionType = section as StructuralSectionISplitParallelFlange;
                height = sectionType.Height.ToSI(SpecTypeId.SectionDimension);
                width = sectionType.Width.ToSI(SpecTypeId.SectionDimension);
                webThickness = sectionType.WebThickness.ToSI(SpecTypeId.SectionDimension);
                flangeThickness = sectionType.FlangeThickness.ToSI(SpecTypeId.SectionDimension);
                rootRadius = sectionType.WebFillet.ToSI(SpecTypeId.SectionDimension);
                toeRadius = sectionType.FlangeFillet.ToSI(SpecTypeId.SectionDimension);
            }
            else if (section is StructuralSectionStructuralTees)
            {
                StructuralSectionStructuralTees sectionType = section as StructuralSectionStructuralTees;
                height = sectionType.Height.ToSI(SpecTypeId.SectionDimension);
                width = sectionType.Width.ToSI(SpecTypeId.SectionDimension);
                webThickness = sectionType.WebThickness.ToSI(SpecTypeId.SectionDimension);
                flangeThickness = sectionType.FlangeThickness.ToSI(SpecTypeId.SectionDimension);
                rootRadius = sectionType.WebFillet.ToSI(SpecTypeId.SectionDimension);
                toeRadius = sectionType.FlangeFillet.ToSI(SpecTypeId.SectionDimension);
            }
            else if (section is StructuralSectionConcreteT)
            {
                StructuralSectionConcreteT sectionType = section as StructuralSectionConcreteT;
                height = sectionType.Height.ToSI(SpecTypeId.SectionDimension);
                width = (sectionType.Width + 2 * sectionType.CantileverLength).ToSI(SpecTypeId.SectionDimension);
                webThickness = sectionType.Width.ToSI(SpecTypeId.SectionDimension);
                flangeThickness = sectionType.CantileverHeight.ToSI(SpecTypeId.SectionDimension);
                rootRadius = 0;
                toeRadius = 0;
            }
            else
            {
                BH.Engine.Base.Compute.RecordWarning($"Dimensions of section profile {familySymbol.Name} (Revit ElementId {familySymbol.Id.IntegerValue}) were extracted using name matching and may be incorrect in some cases.");
                height = familySymbol.LookupParameterDouble(heightNames, dimensionGroups);
                width = familySymbol.LookupParameterDouble(widthNames, dimensionGroups);
                webThickness = familySymbol.LookupParameterDouble(webThicknessNames, dimensionGroups);
                flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, dimensionGroups);
                rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, dimensionGroups);
                toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, dimensionGroups);
            }

            if (double.IsNaN(rootRadius))
                rootRadius = 0;

            if (double.IsNaN(toeRadius))
                toeRadius = 0;

            if (!double.IsNaN(height) && !double.IsNaN(width) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness))
                return BHS.Create.TSectionProfile(height, width, webThickness, flangeThickness, rootRadius, toeRadius);

            return null;
        }

        /***************************************************/

        [Description("Converts a Revit FamilySymbol to BH.oM.Spatial.ShapeProfiles.ZSectionProfile.")]
        [Input("familySymbol", "Revit FamilySymbol to be converted.")]
        [Output("profile", "BH.oM.Spatial.ShapeProfiles.ZSectionProfile resulting from converting the input Revit FamilySymbol.")]
        private static ZSectionProfile ZSectionProfileFromRevit(this FamilySymbol familySymbol)
        {
            double height, flangeWidth, webThickness, flangeThickness, rootRadius, toeRadius;
            StructuralSection section = familySymbol.GetStructuralSection();
            if (section is StructuralSectionZProfile)
            {
                StructuralSectionZProfile sectionType = section as StructuralSectionZProfile;
                height = sectionType.Height.ToSI(SpecTypeId.SectionDimension);
                flangeWidth = sectionType.BottomFlangeLength.ToSI(SpecTypeId.SectionDimension);
                webThickness = sectionType.WallNominalThickness.ToSI(SpecTypeId.SectionDimension);
                flangeThickness = sectionType.WallNominalThickness.ToSI(SpecTypeId.SectionDimension);
                rootRadius = sectionType.InnerFillet.ToSI(SpecTypeId.SectionDimension);
                toeRadius = 0;
            }
            else
            {
                BH.Engine.Base.Compute.RecordWarning($"Dimensions of section profile {familySymbol.Name} (Revit ElementId {familySymbol.Id.IntegerValue}) were extracted using name matching and may be incorrect in some cases.");
                height = familySymbol.LookupParameterDouble(heightNames, dimensionGroups);
                flangeWidth = familySymbol.LookupParameterDouble(widthNames, dimensionGroups);
                webThickness = familySymbol.LookupParameterDouble(webThicknessNames, dimensionGroups);
                flangeThickness = familySymbol.LookupParameterDouble(flangeThicknessNames, dimensionGroups);
                rootRadius = familySymbol.LookupParameterDouble(rootRadiusNames, dimensionGroups);
                toeRadius = familySymbol.LookupParameterDouble(toeRadiusNames, dimensionGroups);
            }

            if (double.IsNaN(rootRadius))
                rootRadius = 0;

            if (double.IsNaN(toeRadius))
                toeRadius = 0;

            if (!double.IsNaN(height) && !double.IsNaN(flangeWidth) && !double.IsNaN(webThickness) && !double.IsNaN(flangeThickness))
                return BHS.Create.ZSectionProfile(height, flangeWidth, webThickness, flangeThickness, rootRadius, toeRadius);

            return null;
        }

        /***************************************************/

        [Description("Converts a Revit FamilySymbol to BH.oM.Spatial.ShapeProfiles.TubeProfile.")]
        [Input("familySymbol", "Revit FamilySymbol to be converted.")]
        [Output("profile", "BH.oM.Spatial.ShapeProfiles.TubeProfile resulting from converting the input Revit FamilySymbol.")]
        private static TubeProfile TubeProfileFromRevit(this FamilySymbol familySymbol)
        {
            double thickness, diameter;
            StructuralSection section = familySymbol.GetStructuralSection();
            if (section is StructuralSectionPipeStandard)
            {
                StructuralSectionPipeStandard sectionType = section as StructuralSectionPipeStandard;
                thickness = sectionType.WallNominalThickness.ToSI(SpecTypeId.SectionDimension);
                diameter = sectionType.Diameter.ToSI(SpecTypeId.SectionDimension);
            }
            else if (section is StructuralSectionRoundHSS)
            {
                StructuralSectionRoundHSS sectionType = section as StructuralSectionRoundHSS;
                thickness = sectionType.WallNominalThickness.ToSI(SpecTypeId.SectionDimension);
                diameter = sectionType.Diameter.ToSI(SpecTypeId.SectionDimension);
            }
            else
            {
                BH.Engine.Base.Compute.RecordWarning($"Dimensions of section profile {familySymbol.Name} (Revit ElementId {familySymbol.Id.IntegerValue}) were extracted using name matching and may be incorrect in some cases.");
                thickness = familySymbol.LookupParameterDouble(wallThicknessNames, dimensionGroups);
                diameter = familySymbol.LookupParameterDouble(diameterNames, dimensionGroups);
            }

            if (!double.IsNaN(diameter) && !double.IsNaN(thickness))
                return BHS.Create.TubeProfile(diameter, thickness);

            double radius = familySymbol.LookupParameterDouble(radiusNames, dimensionGroups);
            if (!double.IsNaN(radius) && !double.IsNaN(thickness))
                return BHS.Create.TubeProfile(radius * 2, thickness);

            return null;
        }

        /***************************************************/

        [Description("Converts a Revit FamilySymbol to BH.oM.Spatial.ShapeProfiles.FreeFormProfile.")]
        [Input("familySymbol", "Revit FamilySymbol to be converted.")]
        [Output("profile", "BH.oM.Spatial.ShapeProfiles.FreeFormProfile resulting from converting the input Revit FamilySymbol.")]
        private static FreeFormProfile FreeFormProfileFromRevit(this FamilySymbol familySymbol, RevitSettings settings)
        {
            XYZ direction;
            if (familySymbol.Family.FamilyPlacementType == FamilyPlacementType.CurveDrivenStructural)
                direction = XYZ.BasisX;
            else if (familySymbol.Family.FamilyPlacementType == FamilyPlacementType.TwoLevelsBased)
                direction = XYZ.BasisZ;
            else
            {
                BH.Engine.Base.Compute.RecordWarning("The profile of a family type could not be found. ElementId: " + familySymbol.Id.IntegerValue.ToString());
                return null;
            }

            // Check if one and only one solid exists to make sure that the column is a single piece
            Solid solid = null;
            Options options = new Options();
            options.DetailLevel = ViewDetailLevel.Fine;
            options.IncludeNonVisibleObjects = false;

            // Activate the symbol temporarily if not active, then extract its geometry (open either transaction or subtransaction, depending on whether one is already open).
            if (!familySymbol.IsActive)
            {
                Document doc = familySymbol.Document;
                if (!doc.IsModifiable)
                {
                    using(Transaction tempTransaction = new Transaction(doc, "Temp activate family symbol"))
                    {
                        tempTransaction.Start();

                        familySymbol.Activate();
                        doc.Regenerate();
                        solid = familySymbol.get_Geometry(options).SingleSolid();
                        if (solid != null)
                            solid = SolidUtils.Clone(solid);

                        tempTransaction.RollBack();
                    }
                }
                else
                {
                    using (SubTransaction tempTransaction = new SubTransaction(doc))
                    {
                        tempTransaction.Start();

                        familySymbol.Activate();
                        doc.Regenerate();
                        solid = familySymbol.get_Geometry(options).SingleSolid();
                        if (solid != null)
                            solid = SolidUtils.Clone(solid);

                        tempTransaction.RollBack();
                    }
                }
            }
            else
                solid = familySymbol.get_Geometry(options).SingleSolid();

            if (solid == null)
            {
                BH.Engine.Base.Compute.RecordWarning("The profile of a family type could not be found because it is empty or it consists of more than one solid. ElementId: " + familySymbol.Id.IntegerValue.ToString());
                return null;
            }

            Autodesk.Revit.DB.Face face = null;
            foreach (Autodesk.Revit.DB.Face f in solid.Faces)
            {
                if (f is PlanarFace && (f as PlanarFace).FaceNormal.Normalize().IsAlmostEqualTo(-direction, 0.001))
                {
                    if (face == null)
                        face = f;
                    else
                    {
                        BH.Engine.Base.Compute.RecordWarning("The profile of a family type could not be found. ElementId: " + familySymbol.Id.IntegerValue.ToString());
                        return null;
                    }
                }
            }

            if (face == null)
            {
                BH.Engine.Base.Compute.RecordWarning("The profile of a family type could not be found. ElementId: " + familySymbol.Id.IntegerValue.ToString());
                return null;
            }
            
            List<ICurve> profileCurves = new List<ICurve>();
            foreach (EdgeArray curveArray in (face as PlanarFace).EdgeLoops)
            {
                foreach (Edge c in curveArray)
                {
                    ICurve curve = c.AsCurve().IFromRevit();
                    if (curve == null)
                    {
                        BH.Engine.Base.Compute.RecordWarning("The profile of a family type could not be converted due to curve conversion issues. ElementId: " + familySymbol.Id.IntegerValue.ToString());
                        return null;
                    }
                    profileCurves.Add(curve);
                }
            }

            if (profileCurves.Count != 0)
            {
                BH.oM.Geometry.Point centroid = solid.ComputeCentroid().PointFromRevit();
                Vector tan = direction.VectorFromRevit().Normalise();

                // Adjustment of the origin to global (0,0,0).
                Vector adjustment = oM.Geometry.Point.Origin - centroid + (centroid - profileCurves[0].IStartPoint()).DotProduct(tan) * tan;
                if (adjustment.Length() > settings.DistanceTolerance)
                    profileCurves = profileCurves.Select(x => x.ITranslate(adjustment)).ToList();

                // Check if the curves are in the horizontal plane, if not then align them.
                if (familySymbol.Family.FamilyPlacementType == FamilyPlacementType.CurveDrivenStructural)
                {
                    // First rotate the profile to align its local plane with global XY, then rotate to align its local Z with global Y.
                    double angle = -Math.PI * 0.5;
                    profileCurves = profileCurves.Select(x => x.IRotate(oM.Geometry.Point.Origin, Vector.YAxis, angle)).ToList();
                    profileCurves = profileCurves.Select(x => x.IRotate(oM.Geometry.Point.Origin, Vector.ZAxis, angle)).ToList();
                }
            }

            return BHS.Create.FreeFormProfile(profileCurves);
        }

        /***************************************************/

        [Description("Queries a single solid from the Revit GeometryElement. If there is more than one solid inside the GeometryElement, null is returned.")]
        [Input("geometryElement", "Revit GeometryElement to be queried.")]
        [Output("solid", "Single solid from the input Revit GeometryElement. If there is more than one solid inside the GeometryElement, null is returned.")]
        private static Solid SingleSolid(this GeometryElement geometryElement)
        {
            Solid solid = null;
            foreach (GeometryObject obj in geometryElement)
            {
                if (obj is Solid)
                {
                    Solid s = obj as Solid;
                    if (!s.Faces.IsEmpty)
                    {
                        if (solid != null)
                            return null;

                        solid = s;
                    }
                }
            }

            return solid;
        }


        /***************************************************/
        /****              Private helpers              ****/
        /***************************************************/

        private static readonly BuiltInParameterGroup[] dimensionGroups = { BuiltInParameterGroup.PG_STRUCTURAL_SECTION_GEOMETRY, BuiltInParameterGroup.PG_GEOMETRY };

        private static readonly string[] diameterNames = { "BHE_Diameter", "Diameter", "d", "D", "OD" };
        private static readonly string[] radiusNames = { "BHE_Radius", "Radius", "r", "R" };
        private static readonly string[] heightNames = { "BHE_Height", "BHE_Depth", "Height", "Depth", "d", "h", "D", "H", "Ht", "b" };
        private static readonly string[] widthNames = { "BHE_Width", "Width", "b", "B", "bf", "w", "W", "D" };
        private static readonly string[] cornerRadiusNames = { "Corner Radius", "r", "r1" };
        private static readonly string[] topFlangeWidthNames = { "Top Flange Width", "bt", "bf_t", "bft", "b1", "b", "B", "Bt", "bf" };
        private static readonly string[] botFlangeWidthNames = { "Bottom Flange Width", "bb", "bf_b", "bfb", "b2", "b", "B", "Bb", "bf" };
        private static readonly string[] webThicknessNames = { "Web Thickness", "Stem Width", "tw", "t", "T" };
        private static readonly string[] topFlangeThicknessNames = { "Top Flange Thickness", "tft", "tf_t", "tf", "T", "t" };
        private static readonly string[] botFlangeThicknessNames = { "Bottom Flange Thickness", "tfb", "tf_b", "tf", "T", "t" };
        private static readonly string[] flangeThicknessNames = { "Flange Thickness", "Slab Depth", "tf", "T", "t" };
        private static readonly string[] weldSizeNames1 = { "Weld Size" };                                            // weld size, diagonal
        private static readonly string[] weldSizeNames2 = { "k" };                                                    // weld size counted from bar's vertical axis
        private static readonly string[] rootRadiusNames = { "Web Fillet", "Root Radius", "r", "r1", "tr", "kr", "R1", "R" };
        private static readonly string[] toeRadiusNames = { "Flange Fillet", "Toe Radius", "r2", "R2" };
        private static readonly string[] innerRadiusNames = { "Inner Fillet", "Inner Radius", "r1", "R1", "ri", "t" };
        private static readonly string[] outerRadiusNames = { "Outer Fillet", "Outer Radius", "r2", "R2", "ro", "tr", "kr" };
        private static readonly string[] wallThicknessNames = { "Wall Nominal Thickness", "Wall Thickness", "t", "T" };

        /***************************************************/
    }
}



