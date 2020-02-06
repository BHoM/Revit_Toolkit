/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure.StructuralSections;

using BH.oM.Environment.Elements;
using BH.oM.Environment.Fragments;
using BH.oM.Geometry.ShapeProfiles;
using BH.oM.Structure.SurfaceProperties;
using BH.oM.Adapters.Revit.Elements;

namespace BH.UI.Revit.Engine
{
    /***************************************************/
    /****              Public methods               ****/
    /***************************************************/
    
    public static partial class Query
    {
        public static IEnumerable<Type> BHoMTypes(this Element element)
        {
            List<Type> result = new List<Type>();

            if (element is FamilyInstance)
            {
                switch(element.Category.CategoryType)
                {
                    case CategoryType.Model:
                        result.Add(typeof(ModelInstance));
                        break;
                    case CategoryType.Annotation:
                        result.Add(typeof(DraftingInstance));
                        break;
                }
                
                //Structural framing
                switch ((BuiltInCategory)element.Category.Id.IntegerValue)
                {
                    //case BuiltInCategory.OST_StructuralFoundation:
                    case Autodesk.Revit.DB.BuiltInCategory.OST_StructuralColumns:
                    case Autodesk.Revit.DB.BuiltInCategory.OST_Columns:
                        result.Add(typeof(BH.oM.Physical.Elements.IFramingElement));
                        result.Add(typeof(BH.oM.Physical.Elements.Column));
                        return result;
                    case Autodesk.Revit.DB.BuiltInCategory.OST_VerticalBracing:
                    case Autodesk.Revit.DB.BuiltInCategory.OST_HorizontalBracing:
                        result.Add(typeof(BH.oM.Physical.Elements.IFramingElement));
                        result.Add(typeof(BH.oM.Physical.Elements.Bracing));
                        return result;
                    case Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFraming:
                    case Autodesk.Revit.DB.BuiltInCategory.OST_Truss:
                    case Autodesk.Revit.DB.BuiltInCategory.OST_StructuralTruss:
                    case Autodesk.Revit.DB.BuiltInCategory.OST_Purlin:
                    case Autodesk.Revit.DB.BuiltInCategory.OST_Joist:
                    case Autodesk.Revit.DB.BuiltInCategory.OST_Girder:
                    case Autodesk.Revit.DB.BuiltInCategory.OST_StructuralStiffener:
                    case Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFramingOther:
                        result.Add(typeof(BH.oM.Physical.Elements.IFramingElement));
                        result.Add(typeof(BH.oM.Physical.Elements.Beam));
                        return result;
                }

                //Environmental Windows and Doors
                switch ((BuiltInCategory)element.Category.Id.IntegerValue)
                {
                    case Autodesk.Revit.DB.BuiltInCategory.OST_Windows:
                        result.Add(typeof(oM.Environment.Elements.Panel));
                        result.Add(typeof(oM.Physical.Elements.Window));
                        return result;
                    case Autodesk.Revit.DB.BuiltInCategory.OST_Doors:
                        result.Add(typeof(oM.Environment.Elements.Panel));
                        result.Add(typeof(oM.Physical.Elements.Door));
                        return result;
                }
            }

            if(element is FamilySymbol)
            {
                //Environmental Windows and Doors
                switch ((BuiltInCategory)element.Category.Id.IntegerValue)
                {
                    case Autodesk.Revit.DB.BuiltInCategory.OST_Windows:
                    case Autodesk.Revit.DB.BuiltInCategory.OST_Doors:
                        return result;
                }
            }

            if (element is CeilingType)
            {
                result.Add(typeof(BH.oM.Common.Properties.Object2DProperties));
                return result;
            }

            if (element is Ceiling)
            {
                result.Add(typeof(oM.Architecture.Elements.Ceiling));
                return result;
            }

            if (element is Autodesk.Revit.DB.Panel)
            {
                result.Add(typeof(oM.Physical.Elements.Window));
                return result;
            }

            if (element is WallType)
            {
                result.Add(typeof(ISurfaceProperty));
                result.Add(typeof(oM.Common.Properties.Object2DProperties));
                return result;
            }

            if(element is Material)
            {
                result.Add(typeof(oM.Common.Materials.Material));
                return result;
            }

            if (element is FloorType)
            {
                result.Add(typeof(ISurfaceProperty));
                result.Add(typeof(oM.Common.Properties.Object2DProperties));
                return result;
            }

            if (element is RoofType)
            {
                result.Add(typeof(ISurfaceProperty));
                result.Add(typeof(oM.Common.Properties.Object2DProperties));
                return result;
            }

            if(element is ElementType)
            {
                result.Add(typeof(oM.Adapters.Revit.Properties.InstanceProperties));
                return result;
            }

            if (BH.Engine.Adapters.Revit.Query.IsAssignableFromByFullName(element.GetType(), typeof(SpatialElement)))
            {
                if(element is Autodesk.Revit.DB.Mechanical.Space)
                {
                    result.Add(typeof(Space));
                    return result;
                }
            }

            if (element is Wall)
            {
                result.Add(typeof(oM.Environment.Elements.Panel));
                result.Add(typeof(oM.Structure.Elements.Panel));
                result.Add(typeof(oM.Architecture.Elements.Wall));
                result.Add(typeof(oM.Physical.Elements.Wall));
                return result;
            }

            if (element is Ceiling)
            {
                result.Add(typeof(oM.Environment.Elements.Panel));
                return result;
            }


            if (BH.Engine.Adapters.Revit.Query.IsAssignableFromByFullName(element.GetType(), typeof(RoofBase)))
            {
                result.Add(typeof(oM.Environment.Elements.Panel));
                result.Add(typeof(oM.Architecture.Elements.Roof));
                result.Add(typeof(oM.Physical.Elements.Roof));
                return result;
            }

            if (element is Floor)
            {
                result.Add(typeof(oM.Environment.Elements.Panel));
                result.Add(typeof(oM.Structure.Elements.Panel));
                result.Add(typeof(oM.Architecture.Elements.Floor));
                result.Add(typeof(oM.Physical.Elements.Floor));
                return result;
            }

            if (element is Level)
            {
                result.Add(typeof(Level));
                return result;
            }

            if (element is Grid)
            {
                result.Add(typeof(oM.Geometry.SettingOut.Grid));
                return result;
            }

            if (element is MultiSegmentGrid)
            {
                result.Add(typeof(oM.Geometry.SettingOut.Grid));
                return result;
            }

            if (element is ViewSheet)
            {
                result.Add(typeof(Sheet));
                return result;
            }

            if (element is Autodesk.Revit.DB.Viewport)
            {
                result.Add(typeof(oM.Adapters.Revit.Elements.Viewport));
                return result;
            }

            if (element is Autodesk.Revit.DB.ViewPlan)
            {
                result.Add(typeof(oM.Adapters.Revit.Elements.ViewPlan));
                return result;
            }

            if (element is ProjectInfo)
            {
                result.Add(typeof(Building));
                return result;
            }

            if(element is CurveElement)
            {
                result.Add(typeof(ModelInstance));
                result.Add(typeof(DraftingInstance));
                return result;
            }

            if(element is GraphicsStyle)
            {
                result.Add(typeof(oM.Adapters.Revit.Properties.InstanceProperties));
                return result;
            }

            if (element is Autodesk.Revit.DB.Mechanical.Space)
            {
                result.Add(typeof(oM.Environment.Elements.Space));
                return result;
            }

            if (element is Autodesk.Revit.DB.Architecture.Room)
            {
                result.Add(typeof(oM.Architecture.Elements.Room));
                return result;
            }

            if (element is Autodesk.Revit.DB.FilledRegion)
            {
                result.Add(typeof(DraftingInstance));
                return result;
            }

            return null; //TODO: shouldn't it be result?
        }

        /***************************************************/

        public static IEnumerable<Type> BHoMTypes(this StructuralSectionShape sectionShape)
        {
            int shapeNum = (int)sectionShape;
            List<Type> result = new List<Type>();

            if (shapeNum == 2)
            {
                result.Add(typeof(RectangleProfile));
                return result;
            }

            if (shapeNum == 5)
            {
                result.Add(typeof(TubeProfile));
                return result;
            }

            if (shapeNum == 6)
            {
                result.Add(typeof(ISectionProfile));
                return result;
            }

            if (shapeNum == 8)
            {
                result.Add(typeof(ISectionProfile));
                return result;
            }

            if (shapeNum == 9)
            {
                result.Add(typeof(ChannelProfile));
                return result;
            }

            if (shapeNum == 11)
            {
                result.Add(typeof(AngleProfile));
                return result;
            }

            if (shapeNum == 12)
            {
                result.Add(typeof(RectangleProfile));
                return result;
            }

            if (shapeNum == 13)
            {
                result.Add(typeof(CircleProfile));
                return result;
            }

            if (shapeNum == 14)
            {
                result.Add(typeof(BoxProfile));
                return result;
            }

            if (shapeNum == 15)
            {
                result.Add(typeof(TubeProfile));
                return result;
            }

            if (shapeNum == 16)
            {
                result.Add(typeof(FabricatedISectionProfile));
                return result;
            }

            if (shapeNum == 17)
            {
                result.Add(typeof(TSectionProfile));
                return result;
            }

            if (shapeNum == 19)
            {
                result.Add(typeof(TSectionProfile));
                return result;
            }

            if (shapeNum == 20)
            {
                result.Add(typeof(ChannelProfile));
                return result;
            }

            if (shapeNum == 23)
            {
                result.Add(typeof(AngleProfile));
                return result;
            }

            if (shapeNum == 25)
            {
                result.Add(typeof(ZSectionProfile));
                return result;
            }

            if (shapeNum == 31)
            {
                result.Add(typeof(RectangleProfile));
                return result;
            }

            if (shapeNum == 33)
            {
                result.Add(typeof(TSectionProfile));
                return result;
            }

            if (shapeNum == 35)
            {
                result.Add(typeof(CircleProfile));
                return result;
            }

            return result;
        }

        /***************************************************/

        public static IEnumerable<Type> BHoMTypes(this string familyName)
        {
            List<Type> result = new List<Type>();

            if (familyName.EndsWith("_Concrete-RectangularBeam"))
            {
                result.Add(typeof(RectangleProfile));
                return result;
            }

            if (familyName.EndsWith("_ConcreteRectangular"))
            {
                result.Add(typeof(RectangleProfile));
                return result;
            }

            if (familyName.EndsWith("_ConcreteRectangularWithCL"))
            {
                result.Add(typeof(RectangleProfile));
                return result;
            }

            if (familyName.EndsWith("_ConcreteSquare"))
            {
                result.Add(typeof(RectangleProfile));
                return result;
            }

            if (familyName.EndsWith("_ConcreteSquareWithCL"))
            {
                result.Add(typeof(RectangleProfile));
                return result;
            }

            if (familyName.EndsWith("_Precast-RectangularBeam"))
            {
                result.Add(typeof(RectangleProfile));
                return result;
            }

            if (familyName.EndsWith("_Precast-RectangularColumn"))
            {
                result.Add(typeof(RectangleProfile));
                return result;
            }

            if (familyName.EndsWith("_ConcreteRectangular-PrecastWithCL"))
            {
                result.Add(typeof(RectangleProfile));
                return result;
            }

            if (familyName.EndsWith("_ConcreteRectangular-Precast"))
            {
                result.Add(typeof(RectangleProfile));
                return result;
            }

            if (familyName.EndsWith("_Precast-SquareColumnWithCL"))
            {
                result.Add(typeof(RectangleProfile));
                return result;
            }

            if (familyName.EndsWith("_Precast-SquareColumn"))
            {
                result.Add(typeof(RectangleProfile));
                return result;
            }

            if (familyName.EndsWith("_Precast-RectangularColumnWithCL"))
            {
                result.Add(typeof(RectangleProfile));
                return result;
            }

            if (familyName == "BHm_StructuralFraming_Timber")
            {
                result.Add(typeof(RectangleProfile));
                return result;
            }

            if (familyName == "BHm_StructuralColumns_Timber")
            {
                result.Add(typeof(RectangleProfile));
                return result;
            }

            if (familyName.EndsWith("_LaminatedVeneerLumber"))
            {
                result.Add(typeof(RectangleProfile));
                return result;
            }

            if (familyName.EndsWith("_ParallelStrandLumberWithCL"))
            {
                result.Add(typeof(RectangleProfile));
                return result;
            }

            if (familyName.EndsWith("_ParallelStrandLumber"))
            {
                result.Add(typeof(RectangleProfile));
                return result;
            }

            if (familyName.EndsWith("_Glulam(1)"))
            {
                result.Add(typeof(RectangleProfile));
                return result;
            }

            if (familyName.EndsWith("_Glulam(1)WithCL"))
            {
                result.Add(typeof(RectangleProfile));
                return result;
            }

            if (familyName.EndsWith("_Glulam(2)"))
            {
                result.Add(typeof(RectangleProfile));
                return result;
            }

            if (familyName.EndsWith("_Glulam(2)WithCL"))
            {
                result.Add(typeof(RectangleProfile));
                return result;
            }

            if (familyName.EndsWith("_DimensionLumber"))
            {
                result.Add(typeof(RectangleProfile));
                return result;
            }

            if (familyName.EndsWith("_DimensionLumberWithCL"))
            {
                result.Add(typeof(RectangleProfile));
                return result;
            }

            if (familyName.EndsWith("_TimberWithCL"))
            {
                result.Add(typeof(RectangleProfile));
                return result;
            }

            if (familyName.EndsWith("_Plate"))
            {
                result.Add(typeof(RectangleProfile));
                return result;
            }

            if (familyName.EndsWith("_RSJ-RolledSteelJoists"))
            {
                result.Add(typeof(ISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_UC-UniversalColumns"))
            {
                result.Add(typeof(ISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_UC-UniversalColumns-Column"))
            {
                result.Add(typeof(ISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_UB-UniversalBeams"))
            {
                result.Add(typeof(ISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_UB-UniversalBeams-Column"))
            {
                result.Add(typeof(ISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_UBP-UniversalBearingPile"))
            {
                result.Add(typeof(ISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_UBP-UniversalBearingPile-Column"))
            {
                result.Add(typeof(ISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_ASB-Beams"))
            {
                result.Add(typeof(ISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_UKC-UKColumns"))
            {
                result.Add(typeof(ISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_UKC-UKColumns-Column"))
            {
                result.Add(typeof(ISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_UKB-UKBeams"))
            {
                result.Add(typeof(ISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_UKB-UKBeams-Column"))
            {
                result.Add(typeof(ISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_UKBP-UKBearingPiles"))
            {
                result.Add(typeof(ISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_UKBP-UKBearingPiles-Column"))
            {
                result.Add(typeof(ISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_IPN-Beams"))
            {
                result.Add(typeof(ISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_IPN-Column"))
            {
                result.Add(typeof(ISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_IPE-Beams"))
            {
                result.Add(typeof(ISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_IPE-Column"))
            {
                result.Add(typeof(ISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_H-WideFlangeBeams"))
            {
                result.Add(typeof(ISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_H-WideFlange-Column"))
            {
                result.Add(typeof(ISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_M-MiscellaneousWideFlange-Column"))
            {
                result.Add(typeof(ISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_W-WideFlange-Column"))
            {
                result.Add(typeof(ISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_HP-BearingPile-Column"))
            {
                result.Add(typeof(ISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_RSJ-RolledSteelJoists-Column"))
            {
                result.Add(typeof(ISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_RoundBar"))
            {
                result.Add(typeof(CircleProfile));
                return result;
            }

            if (familyName.EndsWith("_ConcreteRound"))
            {
                result.Add(typeof(CircleProfile));
                return result;
            }

            if (familyName.EndsWith("_ConcreteRoundWithCL"))
            {
                result.Add(typeof(CircleProfile));
                return result;
            }

            if (familyName.EndsWith("_PlateGirder"))
            {
                result.Add(typeof(FabricatedISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_WeldedWideFlange"))
            {
                result.Add(typeof(FabricatedISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_WeldedReducedFlange"))
            {
                result.Add(typeof(FabricatedISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_Plate-Column"))
            {
                result.Add(typeof(FabricatedISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_WWF-WeldedWideFlange-Column"))
            {
                result.Add(typeof(FabricatedISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_WRF-WeldedReducedFlange-Column"))
            {
                result.Add(typeof(FabricatedISectionProfile));
                return result;
            }

            if (familyName.EndsWith("_L-Angles"))
            {
                result.Add(typeof(AngleProfile));
                return result;
            }

            if (familyName.EndsWith("_L-EqualLegAngles"))
            {
                result.Add(typeof(AngleProfile));
                return result;
            }

            if (familyName.EndsWith("_L-EqualLegAngles-Column"))
            {
                result.Add(typeof(AngleProfile));
                return result;
            }

            if (familyName.EndsWith("_L-UnequalLegAngles"))
            {
                result.Add(typeof(AngleProfile));
                return result;
            }

            if (familyName.EndsWith("_L-UnequalLegAngles-Column"))
            {
                result.Add(typeof(AngleProfile));
                return result;
            }

            if (familyName.EndsWith("_UKA-UKAngles"))
            {
                result.Add(typeof(AngleProfile));
                return result;
            }

            if (familyName.EndsWith("_UKA-UKAngles-Column"))
            {
                result.Add(typeof(AngleProfile));
                return result;
            }

            if (familyName.EndsWith("_C-Channels"))
            {
                result.Add(typeof(ChannelProfile));
                return result;
            }

            if (familyName.EndsWith("_UKPFC-ParallelFlangeChannels"))
            {
                result.Add(typeof(ChannelProfile));
                return result;
            }

            if (familyName.EndsWith("_UKPFC-ParallelFlangeChannels-Column"))
            {
                result.Add(typeof(ChannelProfile));
                return result;
            }

            if (familyName.EndsWith("_PFC-ParallelFlangeChannels"))
            {
                result.Add(typeof(ChannelProfile));
                return result;
            }

            if (familyName.EndsWith("_PFC-ParallelFlangeChannels-Column"))
            {
                result.Add(typeof(ChannelProfile));
                return result;
            }

            if (familyName.EndsWith("_U-ParallelFlangeChannels"))
            {
                result.Add(typeof(ChannelProfile));
                return result;
            }

            if (familyName.EndsWith("_U-Channels"))
            {
                result.Add(typeof(ChannelProfile));
                return result;
            }

            if (familyName.EndsWith("_Precast-SingleTee"))
            {
                result.Add(typeof(TSectionProfile));
                return result;
            }

            if (familyName.EndsWith("_UKT-UKTeesSplitfromUKC"))
            {
                result.Add(typeof(TSectionProfile));
                return result;
            }

            if (familyName.EndsWith("_UKT-UKTeesSplitfromUKC-Column"))
            {
                result.Add(typeof(TSectionProfile));
                return result;
            }

            if (familyName.EndsWith("_UKT-UKTeesSplitfromUKB"))
            {
                result.Add(typeof(TSectionProfile));
                return result;
            }

            if (familyName.EndsWith("_UKT-UKTeesSplitfromUKB-Column"))
            {
                result.Add(typeof(TSectionProfile));
                return result;
            }

            if (familyName.EndsWith("_T-TeesfromUniversalColumns"))
            {
                result.Add(typeof(TSectionProfile));
                return result;
            }

            if (familyName.EndsWith("_T-TeesfromUniversalColumns-Column"))
            {
                result.Add(typeof(TSectionProfile));
                return result;
            }

            if (familyName.EndsWith("_T-TeesfromUniversalBeams"))
            {
                result.Add(typeof(TSectionProfile));
                return result;
            }

            if (familyName.EndsWith("_T-TeesfromUniversalBeams-Column"))
            {
                result.Add(typeof(TSectionProfile));
                return result;
            }

            if (familyName.EndsWith("_MH-TeesfromH-Beams"))
            {
                result.Add(typeof(TSectionProfile));
                return result;
            }

            if (familyName.EndsWith("_T-Tees"))
            {
                result.Add(typeof(TSectionProfile));
                return result;
            }

            if (familyName.EndsWith("_MIPE-TeesfromIPE"))
            {
                result.Add(typeof(TSectionProfile));
                return result;
            }

            if (familyName.EndsWith("_SquareHollowSections"))
            {
                result.Add(typeof(BoxProfile));
                return result;
            }

            if (familyName.EndsWith("_SquareHollowSections-Column"))
            {
                result.Add(typeof(BoxProfile));
                return result;
            }

            if (familyName.EndsWith("_RectangularHollowSections"))
            {
                result.Add(typeof(BoxProfile));
                return result;
            }

            if (familyName.EndsWith("_RectangularHollowSections-Column"))
            {
                result.Add(typeof(BoxProfile));
                return result;
            }

            if (familyName.EndsWith("_SHS-SquareHollowSections(Cold)"))
            {
                result.Add(typeof(BoxProfile));
                return result;
            }

            if (familyName.EndsWith("_SHS-SquareHollowSections-Column(Cold)"))
            {
                result.Add(typeof(BoxProfile));
                return result;
            }

            if (familyName.EndsWith("_SHS-SquareHollowSections"))
            {
                result.Add(typeof(BoxProfile));
                return result;
            }

            if (familyName.EndsWith("_SHS-SquareHollowSections-Column"))
            {
                result.Add(typeof(BoxProfile));
                return result;
            }

            if (familyName.EndsWith("_RHS-RectangularHollowSections(Cold)"))
            {
                result.Add(typeof(BoxProfile));
                return result;
            }

            if (familyName.EndsWith("_RHS-RectangularHollowSections-Column(Cold)"))
            {
                result.Add(typeof(BoxProfile));
                return result;
            }

            if (familyName.EndsWith("_RHS-RectangularHollowSections"))
            {
                result.Add(typeof(BoxProfile));
                return result;
            }

            if (familyName.EndsWith("_RHS-RectangularHollowSections-Column"))
            {
                result.Add(typeof(BoxProfile));
                return result;
            }

            if (familyName.EndsWith("_RectangularandSquareHollowSections"))
            {
                result.Add(typeof(BoxProfile));
                return result;
            }

            if (familyName.EndsWith("_RectangularandSquareHollowSections-Column"))
            {
                result.Add(typeof(BoxProfile));
                return result;
            }

            if (familyName.EndsWith("_CircularHollowSections"))
            {
                result.Add(typeof(TubeProfile));
                return result;
            }

            if (familyName.EndsWith("_CircularHollowSections-Column"))
            {
                result.Add(typeof(TubeProfile));
                return result;
            }

            if (familyName.EndsWith("_CHS-CircularHollowSections(Cold)"))
            {
                result.Add(typeof(TubeProfile));
                return result;
            }

            if (familyName.EndsWith("_CHS-CircularHollowSections-Column(Cold)"))
            {
                result.Add(typeof(TubeProfile));
                return result;
            }

            if (familyName.EndsWith("_CHS-CircularHollowSections"))
            {
                result.Add(typeof(TubeProfile));
                return result;
            }

            if (familyName.EndsWith("_CHS-CircularHollowSections-Column"))
            {
                result.Add(typeof(TubeProfile));
                return result;
            }

            if (familyName.EndsWith("_CircularHollowSections"))
            {
                result.Add(typeof(TubeProfile));
                return result;
            }

            if (familyName.EndsWith("_Pipe-Column"))
            {
                result.Add(typeof(TubeProfile));
                return result;
            }

            return result;
        }

        /***************************************************/
    }
}