/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using BH.Engine.Architecture;
using BH.Engine.Physical;
using BH.Engine.Revit;
using BH.Engine.Spatial;
using BH.Engine.Structure;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Dimensional;
using BH.oM.Geometry;
using BH.oM.Reflection.Attributes;
using BH.oM.Structure.Elements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****      Convert Revit elements to BHoM       ****/
        /***************************************************/

        //[Description("Converts a Revit ProjectInfo to a BHoM object based on the requested engineering discipline.")]
        //[Input("projectInfo", "Revit ProjectInfo to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit ProjectInfo.")]
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

        //[Description("Converts a Revit EnergyAnalysisDetailModel to a BHoM object based on the requested engineering discipline.")]
        //[Input("energyAnalysisModel", "Revit EnergyAnalysisDetailModel to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit EnergyAnalysisDetailModel.")]
        //public static IEnumerable<IBHoMObject> BHoMType(this EnergyAnalysisDetailModel energyAnalysisModel, Discipline discipline, RevitSettings settings = null)
        //{
        //    switch (discipline)
        //    {
        //        case Discipline.Environmental:
        //            return energyAnalysisModel.EnergyAnalysisModelFromRevit(settings, refObjects);
        //        default:
        //            return null;
        //    }
        //}

        /***************************************************/

        //[Description("Converts a Revit FamilyInstance to a BHoM object based on its discipline, if it's an adaptive component and, more importantly, on its category. A multitude of instances will fall into this converter, therefore a special care is needed with the category enums in the backend.")]
        //[Input("familyInstance", "Revit FamilyInstance to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit FamilyInstance.")]
        public static Type BHoMType(this FamilyInstance familyInstance, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                case Discipline.Structural:
                    if (typeof(Bar).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue))
                        return typeof(Bar);
                    break;
                case Discipline.Physical:
                case Discipline.Architecture:
                    if (typeof(BH.oM.Physical.Elements.Window).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue))
                        return typeof(BH.oM.Physical.Elements.Window);
                    else if (typeof(BH.oM.Physical.Elements.Door).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue))
                        return typeof(BH.oM.Physical.Elements.Door);
                    else if (typeof(BH.oM.Physical.Elements.Column).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue) || familyInstance.StructuralType == Autodesk.Revit.DB.Structure.StructuralType.Column)
                        return typeof(BH.oM.Physical.Elements.Column);
                    else if (typeof(BH.oM.Physical.Elements.Bracing).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue)
                            || familyInstance.StructuralUsage == StructuralInstanceUsage.Brace
                            || familyInstance.StructuralUsage == StructuralInstanceUsage.HorizontalBracing
                            || familyInstance.StructuralUsage == StructuralInstanceUsage.KickerBracing
                            || familyInstance.StructuralType == Autodesk.Revit.DB.Structure.StructuralType.Brace)
                        return typeof(BH.oM.Physical.Elements.Bracing);
                    else if (typeof(BH.oM.Physical.Elements.Beam).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue))
                        return typeof(BH.oM.Physical.Elements.Beam);
                    else if (typeof(BH.oM.MEP.System.Fittings.Fitting).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue))
                        return typeof(BH.oM.MEP.System.Fittings.Fitting);
                    break;
                case Discipline.Environmental:
                    if (typeof(BH.oM.MEP.System.Fittings.Fitting).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue))
                        return typeof(BH.oM.MEP.System.Fittings.Fitting);
                    else
                        return typeof(BH.oM.Environment.Elements.Panel);
                case Discipline.Facade:
                    if (typeof(BH.oM.Facade.Elements.Opening).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue))
                        return typeof(BH.oM.Facade.Elements.Opening);
                    else if (typeof(BH.oM.Facade.Elements.FrameEdge).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue))
                        return typeof(BH.oM.Facade.Elements.FrameEdge);
                    break;
            }

            return null;
        }

        /***************************************************/

        //[Description("Converts a Revit FamilySymbol to a BHoM object based on the requested engineering discipline.")]
        //[Input("familySymbol", "Revit FamilySymbol to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit FamilySymbol.")]
        public static Type BHoMType(this FamilySymbol familySymbol, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                default:
                    if (typeof(BH.oM.Spatial.ShapeProfiles.IProfile).BuiltInCategories().Contains((BuiltInCategory)familySymbol.Category.Id.IntegerValue))
                        return typeof(BH.oM.Spatial.ShapeProfiles.IProfile);
                    else
                        return null;
            }
        }

        /***************************************************/

        //[Description("Converts a Revit Wall to a BHoM object based on the requested engineering discipline.")]
        //[Input("wall", "Revit Wall to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit Wall.")]
        public static Type BHoMType(this Wall wall, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return typeof(BH.oM.Environment.Elements.Panel);
                case Discipline.Structural:
                    return typeof(BH.oM.Structure.Elements.Panel);
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

        //[Description("Converts a Revit Ceiling to a BHoM object based on the requested engineering discipline.")]
        //[Input("ceiling", "Revit Ceiling to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit Ceiling.")]
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

        //[Description("Converts a Revit Floor to a BHoM object based on the requested engineering discipline.")]
        //[Input("floor", "Revit Floor to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit Floor.")]
        public static Type BHoMType(this Floor floor, Discipline discipline, RevitSettings settings = null)
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
                    return typeof(BH.oM.Physical.Elements.Floor);
            }

            return null;
        }

        /***************************************************/

        //[Description("Converts a Revit RoofBase to a BHoM object based on the requested engineering discipline.")]
        //[Input("roofBase", "Revit RoofBase to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit RoofBase.")]
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

        //[Description("Converts a Revit HostObjAttributes to a BHoM object based on the requested engineering discipline.")]
        //[Input("hostObjAttributes", "Revit HostObjAttributes to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit HostObjAttributes.")]
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

        //[Description("Converts a Revit CableTray to a BHoM object based on the requested engineering discipline.")]
        //[Input("cableTray", "Revit CableTray to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit CableTray.")]
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

        //[Description("Converts a Revit Duct to a BHoM object based on the requested engineering discipline.")]
        //[Input("duct", "Revit Duct to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit Duct.")]
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

        //[Description("Converts a Revit Pipe to a BHoM object based on the requested engineering discipline.")]
        //[Input("pipe", "Revit Pipe to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit Pipe.")]
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

        //[Description("Converts a Revit Wire to a BHoM object based on the requested engineering discipline.")]
        //[Input("wire", "Revit Wire to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit Wire.")]
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

        //[Description("Converts a Revit Level to a BHoM object based on the requested engineering discipline.")]
        //[Input("level", "Revit Level to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit Level.")]
        public static Type BHoMType(this Level level, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                default:
                    return typeof(BH.oM.Geometry.SettingOut.Level);
            }

            return null;
        }

        /***************************************************/

        //[Description("Converts a Revit Grid to a BHoM object based on the requested engineering discipline.")]
        //[Input("grid", "Revit Grid to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit Grid.")]
        public static Type BHoMType(this Grid grid, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                default:
                    return typeof(BH.oM.Geometry.SettingOut.Grid);
            }

            return null;
        }

        /***************************************************/

        //[Description("Converts a Revit MultiSegmentGrid to a BHoM object based on the requested engineering discipline.")]
        //[Input("grid", "Revit MultiSegmentGrid to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit MultiSegmentGrid.")]
        public static Type BHoMType(this MultiSegmentGrid grid, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                default:
                    return typeof(BH.oM.Geometry.SettingOut.Grid);
            }

            return null;
        }

        /***************************************************/

        //[Description("Converts a Revit ElementType to a BHoM object based on the requested engineering discipline.")]
        //[Input("elementType", "Revit ElementType to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit ElementType.")]
        public static Type BHoMType(this ElementType elementType, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                default:
                    return typeof(BH.oM.Adapters.Revit.Properties.InstanceProperties);
            }

            return null;
        }

        /***************************************************/

        //[Description("Converts a Revit GraphicsStyle to a BHoM object based on the requested engineering discipline.")]
        //[Input("graphicStyle", "Revit GraphicsStyle to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit GraphicsStyle.")]
        public static Type BHoMType(this GraphicsStyle graphicStyle, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                default:
                    return typeof(BH.oM.Adapters.Revit.Properties.InstanceProperties);
            }

            return null;
        }

        /***************************************************/

        //[Description("Converts a Revit SpatialElement to a BHoM object based on the requested engineering discipline.")]
        //[Input("spatialElement", "Revit SpatialElement to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit SpatialElement.")]
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

        //[Description("Converts a Revit EnergyAnalysisSpace to a BHoM object based on the requested engineering discipline.")]
        //[Input("energyAnalysisSpace", "Revit EnergyAnalysisSpace to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit EnergyAnalysisSpace.")]
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

        //[Description("Converts a Revit EnergyAnalysisSurface to a BHoM object based on the requested engineering discipline.")]
        //[Input("energyAnalysisSurface", "Revit EnergyAnalysisSurface to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit EnergyAnalysisSurface.")]
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

        //[Description("Converts a Revit EnergyAnalysisOpening to a BHoM object based on the requested engineering discipline.")]
        //[Input("energyAnalysisOpening", "Revit EnergyAnalysisOpening to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit EnergyAnalysisOpening.")]
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

        //[Description("Converts a Revit ViewSheet to a BHoM object based on the requested engineering discipline.")]
        //[Input("viewSheet", "Revit ViewSheet to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit ViewSheet.")]
        public static Type BHoMType(this ViewSheet viewSheet, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                default:
                    return typeof(BH.oM.Adapters.Revit.Elements.Sheet);
            }

            return null;
        }

        /***************************************************/

        //[Description("Converts a Revit Viewport to a BHoM object based on the requested engineering discipline.")]
        //[Input("viewport", "Revit Viewport to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit Viewport.")]
        public static Type BHoMType(this Viewport viewport, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                default:
                    return typeof(BH.oM.Adapters.Revit.Elements.Viewport);
            }

            return null;
        }

        /***************************************************/

        //[Description("Converts a Revit ViewPlan to a BHoM object based on the requested engineering discipline.")]
        //[Input("viewPlan", "Revit ViewPlan to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit ViewPlan.")]
        public static Type BHoMType(this ViewPlan viewPlan, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                default:
                    return typeof(BH.oM.Adapters.Revit.Elements.ViewPlan);
            }

            return null;
        }

        /***************************************************/

        //[Description("Converts a Revit Material to a BHoM object based on the requested engineering discipline.")]
        //[Input("material", "Revit Material to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit Material.")]
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

        //[Description("Converts a Revit Family to a BHoM object based on the requested engineering discipline.")]
        //[Input("family", "Revit Family to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit Family.")]
        public static Type BHoMType(this Family family, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                default:
                    return typeof(BH.oM.Adapters.Revit.Elements.Family);
            }

            return null;
        }

        /***************************************************/

        //[Description("Converts a Revit CurveElement to a BHoM object based on the requested engineering discipline.")]
        //[Input("curveElement", "Revit CurveElement to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit CurveElement.")]
        public static Type BHoMType(this CurveElement curveElement, Discipline discipline, RevitSettings settings = null)
        {
            switch (discipline)
            {
                default:
                    return typeof(BH.oM.Adapters.Revit.Elements.IInstance);
            }

            return null;
        }

        /***************************************************/

        //[Description("Converts a Revit FilledRegion to a BHoM object based on the requested engineering discipline.")]
        //[Input("filledRegion", "Revit FilledRegion to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit FilledRegion.")]
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

        //[Description("Fallback method when no suitable Element FromRevit is found.")]
        //[Input("element", "Revit Element to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Null resulted from no suitable Element FromRevit method.")]
        public static Type BHoMType(this Element element, Discipline discipline, RevitSettings settings = null)
        {
            return null;
        }


        /***************************************************/
        /****             Interface Methods             ****/
        /***************************************************/

        //[Description("Interface method that tries to find a suitable FromRevit convert for any Revit Element.")]
        //[Input("element", "Revit Element to be converted.")]
        //[Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        //[Input("transform", "Optional, a transform to apply to the converted object.")]
        //[Input("settings", "Optional, Revit adapter settings.")]
        //[Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        //[Output("fromRevit", "Resulted BHoM object converted from a Revit Element.")]
        public static Type IBHoMType(this Element element, Discipline discipline, RevitSettings settings = null)
        {
            return BHoMType(element as dynamic, discipline, settings);
        }

        /***************************************************/
    }
}

