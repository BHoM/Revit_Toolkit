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
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.DB.Structure;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;
using BH.oM.Structure.Elements;
using System;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****             Interface Methods             ****/
        /***************************************************/

        [Description("Interface method that tries to find a suitable BHoM type to convert the given Revit Element to.")]
        [Input("element", "Revit Element to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit Element to.")]
        public static Type IBHoMType(this Element element, Discipline discipline, RevitSettings settings = null)
        {
            return BHoMType(element as dynamic, discipline, settings);
        }

        /***************************************************/
        /****              Public Methods               ****/
        /***************************************************/

        [Description("Finds a suitable BHoM type to convert the given Revit ProjectInfo to, based on the requested engineering discipline and adapter settings.")]
        [Input("projectInfo", "Revit ProjectInfo to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit ProjectInfo to.")]
        public static Type BHoMType(this ProjectInfo projectInfo, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return typeof(BH.oM.Environment.Elements.Building);
                default:
                    return null;
            }
        }

        /***************************************************/

        [Description("Finds a suitable BHoM type to convert the given Revit FamilyInstance to, based on the requested engineering discipline and adapter settings.")]
        [Input("familyInstance", "Revit FamilyInstance to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit FamilyInstance to.")]
        public static Type BHoMType(this FamilyInstance familyInstance, Discipline discipline, RevitSettings settings = null)
        {
            string familyName = familyInstance?.Symbol?.FamilyName;
            BuiltInCategory category = (BuiltInCategory)familyInstance.Category.Id.IntegerValue;

            switch (discipline)
            {
                case Discipline.Structural:
                    if (familyInstance.StructuralType != Autodesk.Revit.DB.Structure.StructuralType.NonStructural
                        && typeof(Bar).BuiltInCategories().Contains(category))
                        return typeof(Bar);
                    break;
                case Discipline.Physical:
                case Discipline.Architecture:
                    if (typeof(BH.oM.Physical.Elements.Window).BuiltInCategories().Contains(category))
                        return typeof(BH.oM.Physical.Elements.Window);
                    else if (typeof(BH.oM.Physical.Elements.Door).BuiltInCategories().Contains(category))
                        return typeof(BH.oM.Physical.Elements.Door);
                    else if (typeof(BH.oM.Physical.Elements.Column).BuiltInCategories().Contains(category)
                            || familyInstance.StructuralType == Autodesk.Revit.DB.Structure.StructuralType.Column)
                        return typeof(BH.oM.Physical.Elements.Column);
                    else if (typeof(BH.oM.Physical.Elements.Bracing).BuiltInCategories().Contains(category)
                            || familyInstance.StructuralUsage == StructuralInstanceUsage.Brace
                            || familyInstance.StructuralUsage == StructuralInstanceUsage.HorizontalBracing
                            || familyInstance.StructuralUsage == StructuralInstanceUsage.KickerBracing
                            || familyInstance.StructuralType == Autodesk.Revit.DB.Structure.StructuralType.Brace)
                        return typeof(BH.oM.Physical.Elements.Bracing);
                    else if (typeof(BH.oM.Physical.Elements.Beam).BuiltInCategories().Contains(category))
                        return typeof(BH.oM.Physical.Elements.Beam);
                    else if (typeof(BH.oM.MEP.System.Fittings.Fitting).BuiltInCategories().Contains(category))
                        return typeof(BH.oM.MEP.System.Fittings.Fitting);
                    else if (typeof(BH.oM.Architecture.BuildersWork.Opening).BuiltInCategories().Contains(category)
                            && settings.MappingSettings.MappedFamilyNames(typeof(BH.oM.Architecture.BuildersWork.Opening)).Contains(familyName))
                        return typeof(BH.oM.Architecture.BuildersWork.Opening);
                    else if (typeof(BH.oM.Lighting.Elements.Luminaire).BuiltInCategories().Contains(category))
                        return typeof(BH.oM.Lighting.Elements.Luminaire);
                    break;
                case Discipline.Environmental:
                    if (typeof(BH.oM.MEP.System.Fittings.Fitting).BuiltInCategories().Contains(category))
                        return typeof(BH.oM.MEP.System.Fittings.Fitting);
                    else
                        return typeof(BH.oM.Environment.Elements.Panel);
                case Discipline.Facade:
                    if (typeof(BH.oM.Facade.Elements.Opening).BuiltInCategories().Contains(category))
                        return typeof(BH.oM.Facade.Elements.Opening);
                    else if (typeof(BH.oM.Facade.Elements.FrameEdge).BuiltInCategories().Contains(category))
                        return typeof(BH.oM.Facade.Elements.FrameEdge);
                    break;
            }

            return null;
        }

        /***************************************************/

        [Description("Finds a suitable BHoM type to convert the given Revit FamilySymbol to, based on the requested engineering discipline and adapter settings.")]
        [Input("familySymbol", "Revit FamilySymbol to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit FamilySymbol to.")]
        public static Type BHoMType(this FamilySymbol familySymbol, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                default:
                    if (typeof(BH.oM.Spatial.ShapeProfiles.IProfile).BuiltInCategories().Contains((BuiltInCategory)familySymbol.Category.Id.IntegerValue))
                        return typeof(BH.oM.Spatial.ShapeProfiles.IProfile);
                    else if (typeof(BH.oM.Lighting.Elements.LuminaireType).BuiltInCategories().Contains((BuiltInCategory)familySymbol.Category.Id.IntegerValue))
                        return typeof(BH.oM.Lighting.Elements.LuminaireType);
                    else
                        return null;
            }
        }

        /***************************************************/

        [Description("Finds a suitable BHoM type to convert the given Revit Wall to, based on the requested engineering discipline and adapter settings.")]
        [Input("wall", "Revit Wall to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit Wall to.")]
        public static Type BHoMType(this Wall wall, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return typeof(BH.oM.Environment.Elements.Panel);
                case Discipline.Structural:
                    if (wall.LookupParameterInteger(BuiltInParameter.WALL_STRUCTURAL_SIGNIFICANT) == 1)
                        return typeof(BH.oM.Structure.Elements.Panel);
                    break;
                case Discipline.Facade:
                    if (wall.CurtainGrid != null)
                        return typeof(BH.oM.Facade.Elements.CurtainWall);
                    else
                        return typeof(BH.oM.Facade.Elements.Panel);
                case Discipline.Architecture:
                case Discipline.Physical:
                    return typeof(BH.oM.Physical.Elements.Wall);
            }

            return null;
        }

        /***************************************************/

        [Description("Finds a suitable BHoM type to convert the given Revit Ceiling to, based on the requested engineering discipline and adapter settings.")]
        [Input("ceiling", "Revit Ceiling to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit Ceiling to.")]
        public static Type BHoMType(this Ceiling ceiling, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return typeof(BH.oM.Environment.Elements.Panel);
                case Discipline.Facade:
                case Discipline.Architecture:
                case Discipline.Physical:
                    return typeof(BH.oM.Architecture.Elements.Ceiling);
            }

            return null;
        }

        /***************************************************/

        [Description("Finds a suitable BHoM type to convert the given Revit Floor to, based on the requested engineering discipline and adapter settings.")]
        [Input("floor", "Revit Floor to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit Floor to.")]
        public static Type BHoMType(this Floor floor, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return typeof(BH.oM.Environment.Elements.Panel);
                case Discipline.Structural:
                    if (floor.LookupParameterInteger(BuiltInParameter.FLOOR_PARAM_IS_STRUCTURAL) == 1)
                        return typeof(BH.oM.Structure.Elements.Panel);
                    break;
                case Discipline.Facade:
                case Discipline.Architecture:
                case Discipline.Physical:
                    return typeof(BH.oM.Physical.Elements.Floor);
            }

            return null;
        }

        /***************************************************/

        [Description("Finds a suitable BHoM type to convert the given Revit RoofBase to, based on the requested engineering discipline and adapter settings.")]
        [Input("roofBase", "Revit RoofBase to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit RoofBase to.")]
        public static Type BHoMType(this RoofBase roofBase, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return typeof(BH.oM.Environment.Elements.Panel);
                case Discipline.Structural:
                    return typeof(BH.oM.Structure.Elements.Panel);
                case Discipline.Facade:
                case Discipline.Architecture:
                case Discipline.Physical:
                    return typeof(BH.oM.Physical.Elements.Roof);
            }

            return null;
        }

        /***************************************************/

        [Description("Finds a suitable BHoM type to convert the given Revit HostObjAttributes to, based on the requested engineering discipline and adapter settings.")]
        [Input("hostObjAttributes", "Revit HostObjAttributes to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit HostObjAttributes to.")]
        public static Type BHoMType(this HostObjAttributes hostObjAttributes, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                case Discipline.Structural:
                    return typeof(BH.oM.Structure.SurfaceProperties.ISurfaceProperty);
                case Discipline.Facade:
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Environmental:
                    return typeof(BH.oM.Physical.Constructions.Construction);
            }

            return null;
        }

        /***************************************************/

        [Description("Finds a suitable BHoM type to convert the given Revit CableTray to, based on the requested engineering discipline and adapter settings.")]
        [Input("cableTray", "Revit CableTray to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit CableTray to.")]
        public static Type BHoMType(this Autodesk.Revit.DB.Electrical.CableTray cableTray, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Environmental:
                    return typeof(BH.oM.MEP.System.CableTray);
            }

            return null;
        }

        /***************************************************/

        [Description("Finds a suitable BHoM type to convert the given Revit Duct to, based on the requested engineering discipline and adapter settings.")]
        [Input("duct", "Revit Duct to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit Duct to.")]
        public static Type BHoMType(this Autodesk.Revit.DB.Mechanical.Duct duct, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Environmental:
                    return typeof(BH.oM.MEP.System.Duct);
            }

            return null;
        }

        /***************************************************/

        [Description("Finds a suitable BHoM type to convert the given Revit Pipe to, based on the requested engineering discipline and adapter settings.")]
        [Input("pipe", "Revit Pipe to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit Pipe to.")]
        public static Type BHoMType(this Autodesk.Revit.DB.Plumbing.Pipe pipe, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Environmental:
                    return typeof(BH.oM.MEP.System.Pipe);
            }

            return null;
        }

        /***************************************************/

        [Description("Finds a suitable BHoM type to convert the given Revit Wire to, based on the requested engineering discipline and adapter settings.")]
        [Input("wire", "Revit Wire to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit Wire to.")]
        public static Type BHoMType(this Autodesk.Revit.DB.Electrical.Wire wire, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Environmental:
                    return typeof(BH.oM.MEP.System.Wire);
            }

            return null;
        }

        /***************************************************/

        [Description("Finds a suitable BHoM type to convert the given Revit Level to, based on the requested engineering discipline and adapter settings.")]
        [Input("level", "Revit Level to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit Level to.")]
        public static Type BHoMType(this Level level, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                default:
                    return typeof(BH.oM.Spatial.SettingOut.Level);
            }
        }

        /***************************************************/

        [Description("Finds a suitable BHoM type to convert the given Revit Grid to, based on the requested engineering discipline and adapter settings.")]
        [Input("grid", "Revit Grid to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit Grid to.")]
        public static Type BHoMType(this Grid grid, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                default:
                    return typeof(BH.oM.Spatial.SettingOut.Grid);
            }
        }

        /***************************************************/

        [Description("Finds a suitable BHoM type to convert the given Revit MultiSegmentGrid to, based on the requested engineering discipline and adapter settings.")]
        [Input("grid", "Revit MultiSegmentGrid to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit MultiSegmentGrid to.")]
        public static Type BHoMType(this MultiSegmentGrid grid, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                default:
                    return typeof(BH.oM.Spatial.SettingOut.Grid);
            }
        }

        /***************************************************/

        [Description("Finds a suitable BHoM type to convert the given Revit ElementType to, based on the requested engineering discipline and adapter settings.")]
        [Input("elementType", "Revit ElementType to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit ElementType to.")]
        public static Type BHoMType(this ElementType elementType, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                default:
                    return typeof(BH.oM.Adapters.Revit.Properties.InstanceProperties);
            }
        }

        /***************************************************/

        [Description("Finds a suitable BHoM type to convert the given Revit GraphicsStyle to, based on the requested engineering discipline and adapter settings.")]
        [Input("graphicStyle", "Revit GraphicsStyle to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit GraphicsStyle to.")]
        public static Type BHoMType(this GraphicsStyle graphicStyle, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                default:
                    return typeof(BH.oM.Adapters.Revit.Properties.InstanceProperties);
            }
        }

        /***************************************************/

        [Description("Finds a suitable BHoM type to convert the given Revit SpatialElement to, based on the requested engineering discipline and adapter settings.")]
        [Input("spatialElement", "Revit SpatialElement to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit SpatialElement to.")]
        public static Type BHoMType(this SpatialElement spatialElement, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return typeof(BH.oM.Environment.Elements.Space);
                case Discipline.Facade:
                case Discipline.Architecture:
                case Discipline.Physical:
                    return typeof(BH.oM.Architecture.Elements.Room);
            }

            return null;
        }

        /***************************************************/

        [Description("Finds a suitable BHoM type to convert the given Revit EnergyAnalysisSpace to, based on the requested engineering discipline and adapter settings.")]
        [Input("energyAnalysisSpace", "Revit EnergyAnalysisSpace to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit EnergyAnalysisSpace to.")]
        public static Type BHoMType(this EnergyAnalysisSpace energyAnalysisSpace, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                case Discipline.Facade:
                case Discipline.Environmental:
                case Discipline.Architecture:
                case Discipline.Physical:
                    return typeof(BH.oM.Environment.Elements.Space);
            }

            return null;
        }

        /***************************************************/

        [Description("Finds a suitable BHoM type to convert the given Revit EnergyAnalysisSurface to, based on the requested engineering discipline and adapter settings.")]
        [Input("energyAnalysisSurface", "Revit EnergyAnalysisSurface to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit EnergyAnalysisSurface to.")]
        public static Type BHoMType(this EnergyAnalysisSurface energyAnalysisSurface, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                case Discipline.Facade:
                case Discipline.Environmental:
                case Discipline.Architecture:
                case Discipline.Physical:
                    return typeof(BH.oM.Environment.Elements.Panel);
            }

            return null;
        }

        /***************************************************/

        [Description("Finds a suitable BHoM type to convert the given Revit EnergyAnalysisOpening to, based on the requested engineering discipline and adapter settings.")]
        [Input("energyAnalysisOpening", "Revit EnergyAnalysisOpening to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit EnergyAnalysisOpening to.")]
        public static Type BHoMType(this EnergyAnalysisOpening energyAnalysisOpening, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                case Discipline.Facade:
                case Discipline.Environmental:
                case Discipline.Architecture:
                case Discipline.Physical:
                    return typeof(BH.oM.Environment.Elements.Panel);
            }

            return null;
        }

        /***************************************************/

        [Description("Finds a suitable BHoM type to convert the given Revit ViewSheet to, based on the requested engineering discipline and adapter settings.")]
        [Input("viewSheet", "Revit ViewSheet to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit ViewSheet to.")]
        public static Type BHoMType(this ViewSheet viewSheet, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                default:
                    return typeof(BH.oM.Adapters.Revit.Elements.Sheet);
            }
        }

        /***************************************************/

        [Description("Finds a suitable BHoM type to convert the given Revit Viewport to, based on the requested engineering discipline and adapter settings.")]
        [Input("viewport", "Revit Viewport to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit Viewport to.")]
        public static Type BHoMType(this Viewport viewport, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                default:
                    return typeof(BH.oM.Adapters.Revit.Elements.Viewport);
            }
        }

        /***************************************************/

        [Description("Finds a suitable BHoM type to convert the given Revit ViewPlan to, based on the requested engineering discipline and adapter settings.")]
        [Input("viewPlan", "Revit ViewPlan to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit ViewPlan to.")]
        public static Type BHoMType(this ViewPlan viewPlan, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                default:
                    return typeof(BH.oM.Adapters.Revit.Elements.ViewPlan);
            }
        }

        /***************************************************/

        [Description("Finds a suitable BHoM type to convert the given Revit Material to, based on the requested engineering discipline and adapter settings.")]
        [Input("material", "Revit Material to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit Material to.")]
        public static Type BHoMType(this Material material, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return typeof(BH.oM.Environment.MaterialFragments.SolidMaterial);
                case Discipline.Structural:
                    return typeof(BH.oM.Structure.MaterialFragments.IMaterialFragment);
                case Discipline.Facade:
                case Discipline.Physical:
                    return typeof(BH.oM.Physical.Materials.Material);

            }

            return null;
        }

        /***************************************************/

        [Description("Finds a suitable BHoM type to convert the given Revit Family to, based on the requested engineering discipline and adapter settings.")]
        [Input("family", "Revit Family to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit Family to.")]
        public static Type BHoMType(this Family family, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                default:
                    return typeof(BH.oM.Adapters.Revit.Elements.Family);
            }
        }

        /***************************************************/

        [Description("Finds a suitable BHoM type to convert the given Revit CurveElement to, based on the requested engineering discipline and adapter settings.")]
        [Input("curveElement", "Revit CurveElement to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit CurveElement to.")]
        public static Type BHoMType(this CurveElement curveElement, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                default:
                    return typeof(BH.oM.Adapters.Revit.Elements.IInstance);
            }
        }

        /***************************************************/

        [Description("Finds a suitable BHoM type to convert the given Revit AssemblyInstance to, based on the requested engineering discipline and adapter settings.")]
        [Input("assemblyInstance", "Revit AssemblyInstance to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit AssemblyInstance to.")]
        public static Type BHoMType(this AssemblyInstance assemblyInstance, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                default:
                    return typeof(BH.oM.Adapters.Revit.Elements.Assembly);
            }
        }

        /***************************************************/

        [Description("Finds a suitable BHoM type to convert the given Revit FilledRegion to, based on the requested engineering discipline and adapter settings.")]
        [Input("filledRegion", "Revit FilledRegion to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit FilledRegion to.")]
        public static Type BHoMType(this FilledRegion filledRegion, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                default:
                    return typeof(BH.oM.Adapters.Revit.Elements.DraftingInstance);
            }

            return null;
        }


        /***************************************************/
        /****             Fallback Methods              ****/
        /***************************************************/

        [Description("Fallback method when no suitable BHoMType method is found for the given Revit Element.")]
        [Input("element", "Revit Element to find a correspondent BHoM type.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the search for the correspondent type.")]
        [Output("bHoMType", "A suitable BHoM type to convert the given Revit Element to.")]
        private static Type BHoMType(this Element element, Discipline discipline, RevitSettings settings = null)
        {
            return null;
        }

        /***************************************************/
    }
}



