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

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.DB.Structure;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Geometry;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using BH.oM.MEP.Elements;
using BH.oM.Reflection.Attributes;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****      Convert Revit elements to BHoM       ****/
        /***************************************************/

        public static IBHoMObject FromRevit(this ProjectInfo projectInfo, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
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

        public static IEnumerable<IBHoMObject> FromRevit(this EnergyAnalysisDetailModel energyAnalysisModel, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
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

        public static IEnumerable<IBHoMObject> FromRevit(this FamilyInstance familyInstance, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Structural:
                    if (typeof(BH.oM.Structure.Elements.Bar).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue))
                        return familyInstance.BarsFromRevit(settings, refObjects).Cast<IBHoMObject>();
                    else
                        return null;
                case Discipline.Physical:
                case Discipline.Architecture:
                    if (typeof(BH.oM.Physical.Elements.Window).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue))
                        return new List<IBHoMObject> { familyInstance.WindowFromRevit(settings, refObjects) };
                    if (typeof(BH.oM.Physical.Elements.Door).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue))
                        return new List<IBHoMObject> { familyInstance.DoorFromRevit(settings, refObjects) };
                    if (typeof(BH.oM.Physical.Elements.Column).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue) || familyInstance.StructuralType == StructuralType.Column)
                        return new List<IBHoMObject> { familyInstance.ColumnFromRevit(settings, refObjects) };
                    if (typeof(BH.oM.Physical.Elements.Bracing).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue) || familyInstance.StructuralType == StructuralType.Brace)
                        return new List<IBHoMObject> { familyInstance.BracingFromRevit(settings, refObjects) };
                    if (typeof(BH.oM.Physical.Elements.Beam).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue))
                        return new List<IBHoMObject> { familyInstance.BeamFromRevit(settings, refObjects) };
                    else
                        return null;
                case Discipline.Environmental:
                    return new List<IBHoMObject> { familyInstance.EnvironmentPanelFromRevit(settings, refObjects) };
                default:
                    return null;
            }
        }

        /***************************************************/

        public static IBHoMObject FromRevit(this FamilySymbol familySymbol, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
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

        public static IEnumerable<IBHoMObject> FromRevit(this Wall wall, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return wall.EnvironmentPanelsFromRevit(settings, refObjects);
                case Discipline.Structural:
                    return wall.StructuralPanelsFromRevit(settings, refObjects);
                case Discipline.Architecture:
                case Discipline.Physical:
                    return new List<IBHoMObject> { wall.WallFromRevit(settings, refObjects) };
                default:
                    return null;
            }
        }

        /***************************************************/

        public static IEnumerable<IBHoMObject> FromRevit(this Ceiling ceiling, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return ceiling.EnvironmentPanelsFromRevit(settings, refObjects);
                case Discipline.Architecture:
                case Discipline.Physical:
                    return new List<IBHoMObject> { ceiling.CeilingFromRevit(settings, refObjects) };
                default:
                    return null;
            }
        }

        /***************************************************/

        public static IEnumerable<IBHoMObject> FromRevit(this Floor floor, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return floor.EnvironmentPanelsFromRevit(settings, refObjects);
                case Discipline.Structural:
                    return floor.StructuralPanelsFromRevit(settings, refObjects);
                case Discipline.Architecture:
                case Discipline.Physical:
                    return new List<IBHoMObject> { floor.FloorFromRevit(settings, refObjects) };
                default:
                    return null;
            }
        }

        /***************************************************/

        public static IEnumerable<IBHoMObject> FromRevit(this RoofBase roofBase, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return roofBase.EnvironmentPanelsFromRevit(settings, refObjects);
                case Discipline.Structural:
                    return roofBase.StructuralPanelsFromRevit(settings, refObjects);
                case Discipline.Architecture:
                case Discipline.Physical:
                    return new List<IBHoMObject> { roofBase.RoofFromRevit(settings, refObjects) };
                default:
                    return null;
            }
        }

        /***************************************************/

        public static IBHoMObject FromRevit(this HostObjAttributes hostObjAttributes, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Structural:
                    return hostObjAttributes.SurfacePropertyFromRevit(null, settings, refObjects) as IBHoMObject;
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Environmental:
                    return hostObjAttributes.ConstructionFromRevit(null, settings, refObjects);
                default:
                    return null;
            }
        }

        /***************************************************/

        [Description("Convert a Revit cable tray into a BHoM cable tray.")]
        [Input("cableTray", "Revit cable tray to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings.")]
        [Input("refObjects", "A collection of objects processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("cableTray", "BHoM cable tray converted from a Revit cable tray.")]
        public static List<IBHoMObject> FromRevit(this Autodesk.Revit.DB.Electrical.CableTray cableTray, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Environmental: 
                    List<IBHoMObject> result = new List<IBHoMObject>();
                    foreach (CableTray ct in cableTray.CableTrayFromRevit(settings, refObjects))
                    {
                        result.Add(ct);
                    }
                    return result;
                default:
                    return null;
            }
        }
        
        /***************************************************/

        [Description("Convert a Revit duct into a BHoM duct.")]
        [Input("duct", "Revit duct to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings.")]
        [Input("refObjects", "A collection of objects processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("duct", "BHoM duct converted from a Revit duct.")]
        public static IBHoMObject FromRevit(this Autodesk.Revit.DB.Mechanical.Duct duct, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Environmental:
                    return duct.DuctFromRevit(settings, refObjects);
                default:
                    return null;
            }
        }

        /***************************************************/
        
        [Description("Convert a Revit pipe into a BHoM pipe.")]
        [Input("pipe", "Revit pipe to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings.")]
        [Input("refObjects", "A collection of objects processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("pipe", "BHoM pipe converted from a Revit pipe.")]
        public static IBHoMObject FromRevit(this Autodesk.Revit.DB.Plumbing.Pipe pipe, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Environmental:
                    return pipe.PipeFromRevit(settings, refObjects);
                default:
                    return null;
            }
        }

        /***************************************************/

        [Description("Convert a Revit wire into a BHoM wire.")]
        [Input("wire", "Revit wire to be converted.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings.")]
        [Input("refObjects", "A collection of objects processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("wire", "BHoM wire converted from a Revit wire.")]
        public static IBHoMObject FromRevit(this Autodesk.Revit.DB.Electrical.Wire wire, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Architecture:
                case Discipline.Physical:
                case Discipline.Environmental:
                    return wire.WireFromRevit(settings, refObjects);
                default:
                    return null;
            }
        }

        /***************************************************/

        public static IBHoMObject FromRevit(this Level level, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                default:
                    return level.LevelFromRevit(settings, refObjects);
            }
        }

        /***************************************************/

        public static IBHoMObject FromRevit(this Grid grid, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                default:
                    return grid.GridFromRevit(settings, refObjects);
            }
        }

        /***************************************************/

        public static IBHoMObject FromRevit(this MultiSegmentGrid grid, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                default:
                    return grid.GridFromRevit(settings, refObjects);
            }
        }

        /***************************************************/

        public static IBHoMObject FromRevit(this ElementType elementType, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                default:
                    return elementType.InstancePropertiesFromRevit(settings, refObjects);
            }
        }

        /***************************************************/

        public static IBHoMObject FromRevit(this GraphicsStyle graphicStyle, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                default:
                    return graphicStyle.InstancePropertiesFromRevit(settings, refObjects);
            }
        }

        /***************************************************/

        public static IBHoMObject FromRevit(this SpatialElement spatialElement, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return spatialElement.SpaceFromRevit(settings, refObjects);
                case Discipline.Architecture:
                case Discipline.Physical:
                    return spatialElement.RoomFromRevit(settings, refObjects);
                default:
                    return null;
            }
        }

        /***************************************************/

        public static IBHoMObject FromRevit(this EnergyAnalysisSpace energyAnalysisSpace, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                case Discipline.Architecture:
                case Discipline.Physical:
                    return energyAnalysisSpace.SpaceFromRevit(settings, refObjects);
                default:
                    return null;
            }
        }

        /***************************************************/

        public static IBHoMObject FromRevit(this EnergyAnalysisSurface energyAnalysisSurface, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                case Discipline.Architecture:
                case Discipline.Physical:
                    return energyAnalysisSurface.EnvironmentPanelFromRevit(settings, refObjects);
                default:
                    return null;
            }
        }

        /***************************************************/

        public static IBHoMObject FromRevit(this EnergyAnalysisOpening energyAnalysisOpening, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                case Discipline.Architecture:
                case Discipline.Physical:
                    return energyAnalysisOpening.EnvironmentPanelFromRevit(null, settings, refObjects);
                default:
                    return null;
            }
        }

        /***************************************************/

        public static IBHoMObject FromRevit(this ViewSheet viewSheet, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                default:
                    return viewSheet.SheetFromRevit(settings, refObjects);
            }
        }

        /***************************************************/

        public static IBHoMObject FromRevit(this Viewport viewport, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                default:
                    return viewport.ViewportFromRevit(settings, refObjects);
            }
        }

        /***************************************************/

        public static IBHoMObject FromRevit(this ViewPlan viewPlan, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                default:
                    return viewPlan.ViewPlanFromRevit(settings, refObjects);
            }
        }

        /***************************************************/

        public static IBHoMObject FromRevit(this Material material, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    return material.SolidMaterialFromRevit(settings, refObjects);
                case Discipline.Structural:
                    return material.MaterialFragmentFromRevit(null, settings, refObjects);
                case Discipline.Physical:
                    return material.MaterialFromRevit(null, settings, refObjects);
                default:
                    return null;
            }
        }

        /***************************************************/

        public static IBHoMObject FromRevit(this Family family, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                default:
                    return family.FamilyFromRevit(settings, refObjects);
            }
        }

        /***************************************************/

        public static IBHoMObject FromRevit(this CurveElement curveElement, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            switch (discipline)
            {
                default:
                    return curveElement.InstanceFromRevit(settings, refObjects);
            }
        }

        /***************************************************/

        public static IBHoMObject FromRevit(this FilledRegion filledRegion, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
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

        public static IBHoMObject FromRevit(this Element element, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            return null;
        }

        /***************************************************/

        public static IGeometry FromRevit(this Location location)
        {
            return null;
        }


        /***************************************************/
        /****             Interface Methods             ****/
        /***************************************************/

        public static object IFromRevit(this Element element, Discipline discipline, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (element == null)
            {
                BH.Engine.Reflection.Compute.RecordWarning("BHoM object could not be read because Revit element does not exist.");
                return null;
            }

            var result = FromRevit(element as dynamic, discipline, settings, refObjects);
            if (result == null || (typeof(IEnumerable<object>).IsAssignableFrom(result.GetType()) && ((IEnumerable<object>)result).Count(x => x != null) == 0))
            {
                result = element.ObjectFromRevit(discipline, settings, refObjects);
                element.NotConvertedWarning(discipline);
            }

            return result;
        }

        /***************************************************/

        public static IGeometry IFromRevit(this Location location)
        {
            if (location == null)
                return null;

            return FromRevit(location as dynamic);
        }

        /***************************************************/
        
    }
}
