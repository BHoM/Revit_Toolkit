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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****      Convert Revit elements to BHoM       ****/
        /***************************************************/

        [Description("Converts a Revit ProjectInfo to a BHoM object based on its discipline.")]
        [Input("projectInfo", "Revit ProjectInfo to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit ProjectInfo.")]
        public static IBHoMObject FromRevit(this ProjectInfo projectInfo, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return projectInfo.BuildingFromRevit(settings, refObjects);
                default:
                    return null;
            }
        }

        /***************************************************/

        [Description("Converts a Revit EnergyAnalysisDetailModel to a BHoM object based on its discipline.")]
        [Input("energyAnalysisModel", "Revit EnergyAnalysisDetailModel to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit EnergyAnalysisDetailModel.")]
        public static IEnumerable<IBHoMObject> FromRevit(this EnergyAnalysisDetailModel energyAnalysisModel, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return energyAnalysisModel.EnergyAnalysisModelFromRevit(settings, refObjects);
                default:
                    return null;
            }
        }

        /***************************************************/
        
        [Description("Converts a Revit FamilyInstance to a BHoM object based on its discipline, if it's an adaptive component and, more importantly, on its category. A multitude of instances will fall into this converter, therefore a special care is needed with the category enums in the backend.")]
        [Input("familyInstance", "Revit FamilyInstance to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit FamilyInstance.")]
        public static IEnumerable<IBHoMObject> FromRevit(this FamilyInstance familyInstance, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (AdaptiveComponentInstanceUtils.IsAdaptiveComponentInstance(familyInstance))
            {
                BH.oM.Adapters.Revit.Elements.ModelInstance instance = familyInstance.ModelInstanceFromRevit(settings, refObjects);

                if (instance != null && transform?.IsIdentity == false)
                {
                    TransformMatrix bHoMTransform = transform.FromRevit();
                    instance = instance.Transform(bHoMTransform) as BH.oM.Adapters.Revit.Elements.ModelInstance;
                }

                return new List<IBHoMObject> { instance };
            }
            else
            {
                IEnumerable<IElement> result = null;
                switch (discipline)
                {
                    case Discipline.Structural:
                        if (typeof(Bar).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue))
                            result = familyInstance.BarsFromRevit(settings, refObjects);
                        break;
                    case Discipline.Physical:
                    case Discipline.Architecture:
                        if (typeof(BH.oM.Physical.Elements.Window).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue))
                            result = new List<IElement> { familyInstance.WindowFromRevit(settings, refObjects) };
                        else if (typeof(BH.oM.Physical.Elements.Door).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue))
                            result = new List<IElement> { familyInstance.DoorFromRevit(settings, refObjects) };
                        else if (typeof(BH.oM.Physical.Elements.Column).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue) || familyInstance.StructuralType == StructuralType.Column)
                            result = new List<IElement> { familyInstance.ColumnFromRevit(settings, refObjects) };
                        else if (typeof(BH.oM.Physical.Elements.Bracing).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue)
                                || familyInstance.StructuralUsage == StructuralInstanceUsage.Brace
                                || familyInstance.StructuralUsage == StructuralInstanceUsage.HorizontalBracing
                                || familyInstance.StructuralUsage == StructuralInstanceUsage.KickerBracing
                                || familyInstance.StructuralType == StructuralType.Brace)
                            result = new List<IElement> { familyInstance.BracingFromRevit(settings, refObjects) };
                        else if (typeof(BH.oM.Physical.Elements.Beam).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue))
                            result = new List<IElement> { familyInstance.BeamFromRevit(settings, refObjects) };
                        else if (typeof(BH.oM.MEP.System.Fittings.Fitting).BuiltInCategories().Contains((BuiltInCategory) familyInstance.Category.Id.IntegerValue))
                            result = new List<IElement> { familyInstance.FittingFromRevit(settings, refObjects) };
                        break;
                    case Discipline.Environmental:
                        if (typeof(BH.oM.MEP.System.Fittings.Fitting).BuiltInCategories().Contains((BuiltInCategory) familyInstance.Category.Id.IntegerValue))
                            result = new List<IElement> { familyInstance.FittingFromRevit(settings, refObjects) };
                        else
                            result = new List<IElement> { familyInstance.EnvironmentPanelFromRevit(settings, refObjects) };
                        break;
                    case Discipline.Facade:
                        if (typeof(BH.oM.Facade.Elements.Opening).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue))
                            result = new List<IElement> { familyInstance.FacadeOpeningFromRevit(settings, refObjects) };
                        else if (typeof(BH.oM.Facade.Elements.FrameEdge).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue))
                            result = new List<IElement> { familyInstance.FrameEdgeFromRevit(settings, refObjects) };
                        break;
                }

                if (result != null && transform?.IsIdentity == false)
                {
                    TransformMatrix bHoMTransform = transform.FromRevit();
                    result = result.Select(x => x?.ITransform(bHoMTransform));
                }

                return result?.Cast<IBHoMObject>().ToList();
            }
        }

        /***************************************************/

        [Description("Converts a Revit FamilySymbol to a BHoM object based on its discipline.")]
        [Input("familySymbol", "Revit FamilySymbol to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit FamilySymbol.")]
        public static IBHoMObject FromRevit(this FamilySymbol familySymbol, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                default:
                    if (typeof(BH.oM.Spatial.ShapeProfiles.IProfile).BuiltInCategories().Contains((BuiltInCategory)familySymbol.Category.Id.IntegerValue))
                        return familySymbol.ProfileFromRevit(settings, refObjects);
                    else
                        return null;
            }
        }

        /***************************************************/

        [Description("Converts a Revit Wall to a BHoM object based on its discipline.")]
        [Input("wall", "Revit Wall to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit Wall.")]
        public static IEnumerable<IBHoMObject> FromRevit(this Wall wall, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            IEnumerable<IElement2D> result = null;
            switch (discipline)
            {
                case Discipline.Environmental:
                    result = wall.EnvironmentPanelsFromRevit(settings, refObjects);
                    break;
                case Discipline.Structural:
                    result = wall.StructuralPanelsFromRevit(settings, refObjects);
                    break;
                case Discipline.Facade:
                    if (wall.CurtainGrid != null)
                        result = new List<IElement2D> { wall.CurtainWallFromRevit(settings, refObjects) };
                    else
                        result = new List<IElement2D> { wall.FacadePanelFromRevit(settings, refObjects) };
                    break;
                case Discipline.Architecture:
                case Discipline.Physical:
                    result = new List<IElement2D> { wall.WallFromRevit(settings, refObjects) };
                    break;
            }

            if (result != null && transform?.IsIdentity == false)
            {
                TransformMatrix bHoMTransform = transform.FromRevit();
                result = result.Select(x => x.ITransform(bHoMTransform));
            }

            return result?.Cast<IBHoMObject>().ToList();
        }

        /***************************************************/

        [Description("Converts a Revit Ceiling to a BHoM object based on its discipline.")]
        [Input("ceiling", "Revit Ceiling to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit Ceiling.")]
        public static IEnumerable<IBHoMObject> FromRevit(this Ceiling ceiling, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            IEnumerable<IElement2D> result = null;
            switch (discipline)
            {
                case Discipline.Environmental:
                    result = ceiling.EnvironmentPanelsFromRevit(settings, refObjects);
                    break;
                case Discipline.Facade:
                case Discipline.Architecture:
                case Discipline.Physical:
                    result = new List<IElement2D> { ceiling.CeilingFromRevit(settings, refObjects) };
                    break;
            }

            if (result != null && transform?.IsIdentity == false)
            {
                TransformMatrix bHoMTransform = transform.FromRevit();
                result = result.Select(x => x.ITransform(bHoMTransform));
            }

            return result?.Cast<IBHoMObject>().ToList();
        }

        /***************************************************/

        [Description("Converts a Revit Floor to a BHoM object based on its discipline.")]
        [Input("floor", "Revit Floor to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit Floor.")]
        public static IEnumerable<IBHoMObject> FromRevit(this Floor floor, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            IEnumerable<IElement2D> result = null;
            switch (discipline)
            {
                case Discipline.Environmental:
                    result = floor.EnvironmentPanelsFromRevit(settings, refObjects);
                    break;
                case Discipline.Structural:
                    result = floor.StructuralPanelsFromRevit(settings, refObjects);
                    break;
                case Discipline.Facade:
                case Discipline.Architecture:
                case Discipline.Physical:
                    result = new List<IElement2D> { floor.FloorFromRevit(settings, refObjects) };
                    break;
            }

            if (result != null && transform?.IsIdentity == false)
            {
                TransformMatrix bHoMTransform = transform.FromRevit();
                result = result.Select(x => x.ITransform(bHoMTransform));
            }

            return result?.Cast<IBHoMObject>().ToList();
        }

        /***************************************************/

        [Description("Converts a Revit RoofBase to a BHoM object based on its discipline.")]
        [Input("roofBase", "Revit RoofBase to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit RoofBase.")]
        public static IEnumerable<IBHoMObject> FromRevit(this RoofBase roofBase, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            IEnumerable<IElement2D> result = null;
            switch (discipline)
            {
                case Discipline.Environmental:
                    result = roofBase.EnvironmentPanelsFromRevit(settings, refObjects);
                    break;
                case Discipline.Structural:
                    result = roofBase.StructuralPanelsFromRevit(settings, refObjects);
                    break;
                case Discipline.Facade:
                case Discipline.Architecture:
                case Discipline.Physical:
                    result = new List<IElement2D> { roofBase.RoofFromRevit(settings, refObjects) };
                    break;
            }

            if (result != null && transform?.IsIdentity == false)
            {
                TransformMatrix bHoMTransform = transform.FromRevit();
                result = result.Select(x => x.ITransform(bHoMTransform));
            }

            return result?.Cast<IBHoMObject>().ToList();
        }

        /***************************************************/

        [Description("Converts a Revit HostObjAttributes to a BHoM object based on its discipline.")]
        [Input("hostObjAttributes", "Revit HostObjAttributes to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit HostObjAttributes.")]
        public static IBHoMObject FromRevit(this HostObjAttributes hostObjAttributes, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Structural:
                    return hostObjAttributes.SurfacePropertyFromRevit(null, settings, refObjects) as IBHoMObject;
                case Discipline.Facade:
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Environmental:
                    return hostObjAttributes.ConstructionFromRevit(null, settings, refObjects);
                default:
                    return null;
            }
        }

        /***************************************************/

        [Description("Converts a Revit CableTray to a BHoM object based on its discipline.")]
        [Input("cableTray", "Revit CableTray to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit CableTray.")]
        public static IEnumerable<IBHoMObject> FromRevit(this Autodesk.Revit.DB.Electrical.CableTray cableTray, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            IEnumerable<IElement1D> result = null;
            switch (discipline)
            {
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Environmental:
                    result = new List<IElement1D>(cableTray.CableTrayFromRevit(settings, refObjects));
                    break;
            }

            if (result != null && transform?.IsIdentity == false)
            {
                TransformMatrix bHoMTransform = transform.FromRevit();
                result = result.Select(x => x.ITransform(bHoMTransform));
            }

            return result?.Cast<IBHoMObject>().ToList();
        }
        
        /***************************************************/

        [Description("Converts a Revit Duct to a BHoM object based on its discipline.")]
        [Input("duct", "Revit Duct to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit Duct.")]
        public static IEnumerable<IBHoMObject> FromRevit(this Autodesk.Revit.DB.Mechanical.Duct duct, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            IEnumerable<IElement1D> result = null;
            switch (discipline)
            {
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Environmental:
                    result = new List<IElement1D>(duct.DuctFromRevit(settings, refObjects));
                    break;
            }

            if (result != null && transform?.IsIdentity == false)
            {
                TransformMatrix bHoMTransform = transform.FromRevit();
                result = result.Select(x => x.ITransform(bHoMTransform));
            }

            return result?.Cast<IBHoMObject>().ToList();
        }

        /***************************************************/
        
        [Description("Converts a Revit Pipe to a BHoM object based on its discipline.")]
        [Input("pipe", "Revit Pipe to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit Pipe.")]
        public static IEnumerable<IBHoMObject> FromRevit(this Autodesk.Revit.DB.Plumbing.Pipe pipe, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            IEnumerable<IElement1D> result = null;
            switch (discipline)
            {
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Environmental:
                    result = new List<IElement1D>(pipe.PipeFromRevit(settings, refObjects));
                    break;
            }

            if (result != null && transform?.IsIdentity == false)
            {
                TransformMatrix bHoMTransform = transform.FromRevit();
                result = result.Select(x => x.ITransform(bHoMTransform));
            }

            return result?.Cast<IBHoMObject>().ToList();
        }

        /***************************************************/

        [Description("Converts a Revit Wire to a BHoM object based on its discipline.")]
        [Input("wire", "Revit Wire to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit Wire.")]
        public static IBHoMObject FromRevit(this Autodesk.Revit.DB.Electrical.Wire wire, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            IElement1D result = null;
            switch (discipline)
            {
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Environmental:
                    result = wire.WireFromRevit(settings, refObjects);
                    break;
            }

            if (result != null && transform?.IsIdentity == false)
            {
                TransformMatrix bHoMTransform = transform.FromRevit();
                result = result.ITransform(bHoMTransform);
            }

            return result as IBHoMObject;
        }

        /***************************************************/

        [Description("Converts a Revit Level to a BHoM object based on its discipline.")]
        [Input("level", "Revit Level to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit Level.")]
        public static IBHoMObject FromRevit(this Level level, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                default:
                    BH.oM.Geometry.SettingOut.Level result = level.LevelFromRevit(settings, refObjects);
                    if (result != null && transform?.IsIdentity == false)
                        result.Elevation += transform.Origin.Z.ToSI(UnitType.UT_Length);

                    return result;
            }
        }

        /***************************************************/

        [Description("Converts a Revit Grid to a BHoM object based on its discipline.")]
        [Input("grid", "Revit Grid to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit Grid.")]
        public static IBHoMObject FromRevit(this Grid grid, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            IElement1D result = null;
            switch (discipline)
            {
                default:
                    result = grid.GridFromRevit(settings, refObjects);
                    break;
            }

            if (result != null && transform?.IsIdentity == false)
            {
                TransformMatrix bHoMTransform = transform.FromRevit();
                result = result.ITransform(bHoMTransform);
            }

            return result as IBHoMObject;
        }

        /***************************************************/

        [Description("Converts a Revit MultiSegmentGrid to a BHoM object based on its discipline.")]
        [Input("grid", "Revit MultiSegmentGrid to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit MultiSegmentGrid.")]
        public static IBHoMObject FromRevit(this MultiSegmentGrid grid, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            IElement1D result = null;
            switch (discipline)
            {
                default:
                    result = grid.GridFromRevit(settings, refObjects);
                    break;
            }

            if (result != null && transform?.IsIdentity == false)
            {
                TransformMatrix bHoMTransform = transform.FromRevit();
                result = result.ITransform(bHoMTransform);
            }

            return result as IBHoMObject;
        }

        /***************************************************/

        [Description("Converts a Revit ElementType to a BHoM object based on its discipline.")]
        [Input("elementType", "Revit ElementType to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit ElementType.")]
        public static IBHoMObject FromRevit(this ElementType elementType, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                default:
                    return elementType.InstancePropertiesFromRevit(settings, refObjects);
            }
        }

        /***************************************************/

        [Description("Converts a Revit GraphicsStyle to a BHoM object based on its discipline.")]
        [Input("graphicStyle", "Revit GraphicsStyle to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit GraphicsStyle.")]
        public static IBHoMObject FromRevit(this GraphicsStyle graphicStyle, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                default:
                    return graphicStyle.InstancePropertiesFromRevit(settings, refObjects);
            }
        }

        /***************************************************/

        [Description("Converts a Revit SpatialElement to a BHoM object based on its discipline.")]
        [Input("spatialElement", "Revit SpatialElement to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit SpatialElement.")]
        public static IBHoMObject FromRevit(this SpatialElement spatialElement, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            IElement2D result = null;
            switch (discipline)
            {
                case Discipline.Environmental:
                    result = spatialElement.SpaceFromRevit(settings, refObjects);
                    break;
                case Discipline.Facade:
                case Discipline.Architecture:
                case Discipline.Physical:
                    result = spatialElement.RoomFromRevit(settings, refObjects);
                    break;
            }

            if (result != null && transform?.IsIdentity == false)
            {
                TransformMatrix bHoMTransform = transform.FromRevit();
                result = result.ITransform(bHoMTransform);
            }

            return result as IBHoMObject;
        }

        /***************************************************/

        [Description("Converts a Revit EnergyAnalysisSpace to a BHoM object based on its discipline.")]
        [Input("energyAnalysisSpace", "Revit EnergyAnalysisSpace to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit EnergyAnalysisSpace.")]
        public static IBHoMObject FromRevit(this EnergyAnalysisSpace energyAnalysisSpace, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            IElement2D result = null;
            switch (discipline)
            {
                case Discipline.Facade:
                case Discipline.Environmental:
                case Discipline.Architecture:
                case Discipline.Physical:
                    result = energyAnalysisSpace.SpaceFromRevit(settings, refObjects);
                    break;
            }

            if (result != null && transform?.IsIdentity == false)
            {
                TransformMatrix bHoMTransform = transform.FromRevit();
                result = result.ITransform(bHoMTransform);
            }

            return result as IBHoMObject;
        }

        /***************************************************/

        [Description("Converts a Revit EnergyAnalysisSurface to a BHoM object based on its discipline.")]
        [Input("energyAnalysisSurface", "Revit EnergyAnalysisSurface to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit EnergyAnalysisSurface.")]
        public static IBHoMObject FromRevit(this EnergyAnalysisSurface energyAnalysisSurface, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            IElement2D result = null;
            switch (discipline)
            {
                case Discipline.Facade:
                case Discipline.Environmental:
                case Discipline.Architecture:
                case Discipline.Physical:
                    result = energyAnalysisSurface.EnvironmentPanelFromRevit(settings, refObjects);
                    break;
            }

            if (result != null && transform?.IsIdentity == false)
            {
                TransformMatrix bHoMTransform = transform.FromRevit();
                result = result.ITransform(bHoMTransform);
            }

            return result as IBHoMObject;
        }

        /***************************************************/

        [Description("Converts a Revit EnergyAnalysisOpening to a BHoM object based on its discipline.")]
        [Input("energyAnalysisOpening", "Revit EnergyAnalysisOpening to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit EnergyAnalysisOpening.")]
        public static IBHoMObject FromRevit(this EnergyAnalysisOpening energyAnalysisOpening, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            IElement2D result = null;
            switch (discipline)
            {
                case Discipline.Facade:
                case Discipline.Environmental:
                case Discipline.Architecture:
                case Discipline.Physical:
                    result = energyAnalysisOpening.EnvironmentPanelFromRevit(null, settings, refObjects);
                    break;
            }

            if (result != null && transform?.IsIdentity == false)
            {
                TransformMatrix bHoMTransform = transform.FromRevit();
                result = result.ITransform(bHoMTransform);
            }

            return result as IBHoMObject;
        }

        /***************************************************/

        [Description("Converts a Revit ViewSheet to a BHoM object based on its discipline.")]
        [Input("viewSheet", "Revit ViewSheet to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit ViewSheet.")]
        public static IBHoMObject FromRevit(this ViewSheet viewSheet, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                default:
                    return viewSheet.SheetFromRevit(settings, refObjects);
            }
        }

        /***************************************************/
        
        [Description("Converts a Revit Viewport to a BHoM object based on its discipline.")]
        [Input("viewport", "Revit Viewport to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit Viewport.")]
        public static IBHoMObject FromRevit(this Viewport viewport, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                default:
                    return viewport.ViewportFromRevit(settings, refObjects);
            }
        }

        /***************************************************/

        [Description("Converts a Revit ViewPlan to a BHoM object based on its discipline.")]
        [Input("viewPlan", "Revit ViewPlan to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit ViewPlan.")]
        public static IBHoMObject FromRevit(this ViewPlan viewPlan, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                default:
                    return viewPlan.ViewPlanFromRevit(settings, refObjects);
            }
        }

        /***************************************************/

        [Description("Converts a Revit Material to a BHoM object based on its discipline.")]
        [Input("material", "Revit Material to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit Material.")]
        public static IBHoMObject FromRevit(this Material material, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return material.SolidMaterialFromRevit(settings, refObjects);
                case Discipline.Structural:
                    return material.MaterialFragmentFromRevit(null, settings, refObjects);
                case Discipline.Facade:
                case Discipline.Physical:
                    return material.MaterialFromRevit(null, settings, refObjects);
                default:
                    return null;
            }
        }

        /***************************************************/

        [Description("Converts a Revit Family to a BHoM object based on its discipline.")]
        [Input("family", "Revit Family to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit Family.")]
        public static IBHoMObject FromRevit(this Family family, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                default:
                    return family.FamilyFromRevit(settings, refObjects);
            }
        }

        /***************************************************/

        [Description("Converts a Revit CurveElement to a BHoM object based on its discipline.")]
        [Input("curveElement", "Revit CurveElement to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit CurveElement.")]
        public static IBHoMObject FromRevit(this CurveElement curveElement, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            BH.oM.Adapters.Revit.Elements.IInstance result = null;
            switch (discipline)
            {
                default:
                    result = curveElement.InstanceFromRevit(settings, refObjects);
                    break;
            }

            if (result is BH.oM.Adapters.Revit.Elements.ModelInstance && transform?.IsIdentity == false)
            {
                TransformMatrix bHoMTransform = transform.FromRevit();
                result = result.Transform(bHoMTransform);
            }

            return result;
        }

        /***************************************************/

        [Description("Converts a Revit FilledRegion to a BHoM object based on its discipline.")]
        [Input("filledRegion", "Revit FilledRegion to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit FilledRegion.")]
        public static IBHoMObject FromRevit(this FilledRegion filledRegion, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                default:
                    return filledRegion.DraftingInstanceFromRevit(settings, refObjects);
            }
        }


        /***************************************************/
        /****             Fallback Methods              ****/
        /***************************************************/

        [Description("Fallback method when no suitable Element FromRevit is found.")]
        [Input("element", "Revit Element to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Null resulted from no suitable Element FromRevit method.")]
        public static IBHoMObject FromRevit(this Element element, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            return null;
        }

        /***************************************************/

        [Description("Fallback method when no suitable FromRevit for Location is found, e.g. when it's not LocationPoint nor LocationCurve.")]
        [Input("location", "Revit Location to be converted.")]
        [Output("fromRevit", "Null resulted from no suitable Location FromRevit method.")]
        public static IGeometry FromRevit(this Location location)
        {
            return null;
        }


        /***************************************************/
        /****             Interface Methods             ****/
        /***************************************************/

        [Description("Interface method that tries to find a suitable FromRevit convert for any Revit Element.")]
        [Input("element", "Revit Element to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("transform", "Optional, a transform to apply to the converted object.")]
        [Input("settings", "Optional, Revit adapter settings.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit Element.")]
        public static object IFromRevit(this Element element, Discipline discipline, Transform transform = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (element == null)
            {
                BH.Engine.Reflection.Compute.RecordWarning("BHoM object could not be read because Revit element does not exist.");
                return null;
            }

            var result = FromRevit(element as dynamic, discipline, transform, settings, refObjects);
            if (result == null || (typeof(IEnumerable<object>).IsAssignableFrom(result.GetType()) && ((IEnumerable<object>)result).Count(x => x != null) == 0))
            {
                result = element.ObjectFromRevit(discipline, settings, refObjects);
                if (result is BH.oM.Adapters.Revit.Elements.ModelInstance && transform?.IsIdentity == false)
                {
                    TransformMatrix bHoMTransform = transform.FromRevit();
                    result = (result as BH.oM.Adapters.Revit.Elements.IInstance).Transform(bHoMTransform);
                }

                element.NotConvertedWarning(discipline);
            }

            return result;
        }

        /***************************************************/

        [Description("Interface method that tries to find a suitable FromRevit convert for a Revit Location.")]
        [Input("location", "Revit Location to be converted.")]
        [Output("fromRevit", "Resulted BHoM object converted from a Revit Location.")]
        public static IGeometry IFromRevit(this Location location)
        {
            if (location == null)
                return null;

            return FromRevit(location as dynamic);
        }

        /***************************************************/
    }
}

