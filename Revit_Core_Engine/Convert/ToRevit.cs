/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Converts BH.oM.Spatial.SettingOut.Grid to a Revit Grid or MultiSegmentGrid.")]
        [Input("grid", "BH.oM.Spatial.SettingOut.Grid to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("grid", "Revit Grid or MultiSegmentGrid resulting from converting the input BH.oM.Spatial.SettingOut.Grid.")]
        public static Element ToRevit(this oM.Spatial.SettingOut.Grid grid, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return grid.ToRevitGrid(document, settings, refObjects);
        }

        /***************************************************/

        [Description("Converts BH.oM.Spatial.SettingOut.Level to a Revit Level.")]
        [Input("level", "BH.oM.Spatial.SettingOut.Level to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("level", "Revit Level resulting from converting the input BH.oM.Spatial.SettingOut.Level.")]
        public static Element ToRevit(this oM.Spatial.SettingOut.Level level, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return level.ToRevitLevel(document, settings, refObjects);
        }

        /***************************************************/

        [Description("Converts BH.oM.Physical.Elements.Wall to a Revit Wall.")]
        [Input("wall", "BH.oM.Physical.Elements.Wall to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("wall", "Revit Wall resulting from converting the input BH.oM.Physical.Elements.Wall.")]
        public static Element ToRevit(this oM.Physical.Elements.Wall wall, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return wall.ToRevitWall(document, settings, refObjects);
        }

        /***************************************************/

        [Description("Converts BH.oM.Physical.Elements.Floor to a Revit Floor.")]
        [Input("floor", "BH.oM.Physical.Elements.Floor to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("floor", "Revit Floor resulting from converting the input BH.oM.Physical.Elements.Floor.")]
        public static Element ToRevit(this oM.Physical.Elements.Floor floor, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return floor.ToRevitFloor(document, settings, refObjects);
        }

        /***************************************************/

        [Description("Converts BH.oM.Physical.Elements.Roof to a Revit RoofBase.")]
        [Input("roof", "BH.oM.Physical.Elements.Roof to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("roofBase", "Revit RoofBase resulting from converting the input BH.oM.Physical.Elements.Roof.")]
        public static Element ToRevit(this oM.Physical.Elements.Roof roof, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return roof.ToRevitRoofBase(document, settings, refObjects);
        }

        /***************************************************/

        [Description("Converts BH.oM.Adapters.Revit.Elements.ModelInstance to a Revit Element.")]
        [Input("modelInstance", "BH.oM.Adapters.Revit.Elements.ModelInstance to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("element", "Revit Element resulting from converting the input BH.oM.Adapters.Revit.Elements.ModelInstance.")]
        public static Element ToRevit(this ModelInstance modelInstance, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            switch (modelInstance.BuiltInCategory(document))
            {
                case BuiltInCategory.OST_Lines:
                    return modelInstance.ToCurveElement(document, settings, refObjects);
                default:
                    return modelInstance.ToRevitElement(document, settings, refObjects);
            }
        }

        /***************************************************/

        [Description("Converts BH.oM.Adapters.Revit.Elements.DraftingInstance to a Revit Element.")]
        [Input("draftingInstance", "BH.oM.Adapters.Revit.Elements.DraftingInstance to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("element", "Revit Element resulting from converting the input BH.oM.Adapters.Revit.Elements.DraftingInstance.")]
        public static Element ToRevit(this DraftingInstance draftingInstance, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            switch (draftingInstance.BuiltInCategory(document))
            {
                case BuiltInCategory.OST_Lines:
                    return draftingInstance.ToCurveElement(document, settings, refObjects);
                default:
                    return draftingInstance.ToRevitElement(document, settings, refObjects);
            }
        }

        /***************************************************/

        [Description("Converts BH.oM.Physical.Materials.Material to a Revit Material.")]
        [Input("material", "BH.oM.Physical.Materials.Material to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("material", "Revit Material resulting from converting the input BH.oM.Physical.Materials.Material.")]
        public static Element ToRevit(this BH.oM.Physical.Materials.Material material, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return material.ToRevitMaterial(document, settings, refObjects);
        }

        /***************************************************/

        [Description("Converts BH.oM.Adapters.Revit.Elements.ViewPlan to a Revit ViewPlan.")]
        [Input("viewPlan", "BH.oM.Adapters.Revit.Elements.ViewPlan to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("viewPlan", "Revit ViewPlan resulting from converting the input BH.oM.Adapters.Revit.Elements.ViewPlan.")]
        public static Element ToRevit(this oM.Adapters.Revit.Elements.ViewPlan viewPlan, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return viewPlan.ToRevitViewPlan(document, settings, refObjects);
        }

        /***************************************************/

        [Description("Converts BH.oM.Adapters.Revit.Elements.Viewport to a Revit Viewport.")]
        [Input("viewport", "BH.oM.Adapters.Revit.Elements.Viewport to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("viewport", "Revit Viewport resulting from converting the input BH.oM.Adapters.Revit.Elements.Viewport.")]
        public static Element ToRevit(this oM.Adapters.Revit.Elements.Viewport viewport, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return viewport.ToRevitViewport(document, settings, refObjects);
        }

        /***************************************************/

        [Description("Converts BH.oM.Adapters.Revit.Elements.Sheet to a Revit ViewSheet.")]
        [Input("sheet", "BH.oM.Adapters.Revit.Elements.Sheet to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("viewSheet", "Revit ViewSheet resulting from converting the input BH.oM.Adapters.Revit.Elements.Sheet.")]
        public static Element ToRevit(this Sheet sheet, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return sheet.ToRevitSheet(document, settings, refObjects);
        }

        /***************************************************/

        [Description("Converts BH.oM.Environment.Elements.Space to a Revit Space.")]
        [Input("space", "BH.oM.Environment.Elements.Space to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("space", "Revit Space resulting from converting the input BH.oM.Environment.Elements.Space.")]
        public static Element ToRevit(this BH.oM.Environment.Elements.Space space, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return space.ToRevitSpace(document, settings, refObjects);
        }

        /***************************************************/

        [Description("Converts BH.oM.Physical.Elements.IFramingElement to a Revit FamilyInstance.")]
        [Input("framingElement", "BH.oM.Physical.Elements.IFramingElement to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("instance", "Revit FamilyInstance resulting from converting the input BH.oM.Physical.Elements.IFramingElement.")]
        public static Element ToRevit(this BH.oM.Physical.Elements.IFramingElement framingElement, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return framingElement.IToRevitFamilyInstance(document, settings, refObjects);
        }

        /***************************************************/

        [Description("Converts BH.oM.Adapters.Revit.Elements.Family to a Revit Family.")]
        [Input("family", "BH.oM.Adapters.Revit.Elements.Family to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("family", "Revit Family resulting from converting the input BH.oM.Adapters.Revit.Elements.Family.")]
        public static Element ToRevit(this oM.Adapters.Revit.Elements.Family family, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return family.ToRevitFamily(document, settings, refObjects);
        }

        /***************************************************/

        [Description("Converts BH.oM.Adapters.Revit.Properties.InstanceProperties to a Revit ElementType.")]
        [Input("instanceProperties", "BH.oM.Adapters.Revit.Properties.InstanceProperties to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("elementType", "Revit ElementType resulting from converting the input BH.oM.Adapters.Revit.Properties.InstanceProperties.")]
        public static Element ToRevit(this oM.Adapters.Revit.Properties.InstanceProperties instanceProperties, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return instanceProperties.ToRevitElementType(document, settings, refObjects);
        }

        /***************************************************/

        [Description("Converts BH.oM.MEP.System.Duct to a Revit Duct.")]
        [Input("duct", "BH.oM.MEP.System.Duct to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("duct", "Revit Duct resulting from converting the input BH.oM.MEP.System.Duct.")]
        public static Element ToRevit(this BH.oM.MEP.System.Duct duct, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return duct.ToRevitDuct(document, settings, refObjects);
        }

        /***************************************************/

        [Description("Converts BH.oM.MEP.System.Pipe to a Revit Pipe.")]
        [Input("pipe", "BH.oM.MEP.System.Pipe to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("pipe", "Revit Pipe resulting from converting the input BH.oM.MEP.System.Pipe.")]
        public static Element ToRevit(this BH.oM.MEP.System.Pipe pipe, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return pipe.ToRevitPipe(document, settings, refObjects);
        }

        /***************************************************/

        [Description("Converts BH.oM.MEP.System.CableTray to a Revit CableTray.")]
        [Input("cableTray", "BH.oM.MEP.System.CableTray to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("cableTray", "Revit CableTray resulting from converting the input BH.oM.MEP.System.CableTray.")]
        public static Element ToRevit(this BH.oM.MEP.System.CableTray cableTray, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return cableTray.ToRevitCableTray(document, settings, refObjects);
        }

        /***************************************************/

        [Description("Converts BH.oM.Physical.Reinforcement.IReinforcingBar to a Revit Rebar.")]
        [Input("reinforcement", "BH.oM.Physical.Reinforcement.IReinforcingBar to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("rebar", "Revit Rebar resulting from converting the input BH.oM.Physical.Reinforcement.IReinforcingBar.")]
        public static Element ToRevit(this BH.oM.Physical.Reinforcement.IReinforcingBar reinforcement, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return reinforcement.IToRevitRebar(document, settings, refObjects);
        }

        /***************************************************/

        [Description("Converts BH.oM.Adapters.Revit.ClonedType to a Revit ElementType.")]
        [Input("clonedType", "BH.oM.Adapters.Revit.ClonedType to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("elementType", "Revit ElementType resulting from converting the input BH.oM.Adapters.Revit.ClonedType.")]
        public static Element ToRevit(this ClonedType clonedType, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return clonedType.ToRevitElementType(document, settings, refObjects);
        }

        /***************************************************/

        [Description("Converts BH.oM.Adapters.Revit.Parameters.ParameterDefinition to a Revit ParameterElement.")]
        [Input("parameterDefinition", "BH.oM.Adapters.Revit.Parameters.ParameterDefinition to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("parameterElement", "Revit ParameterElement resulting from converting the input BH.oM.Adapters.Revit.Parameters.ParameterDefinition.")]
        public static Element ToRevit(this ParameterDefinition parameterDefinition, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return parameterDefinition.ToRevitParameterElement(document, settings, refObjects);
        }

        /***************************************************/

        [Description("Converts BH.oM.Architecture.BuildersWork.Opening to a Revit FamilyInstance.")]
        [Input("opening", "BH.oM.Architecture.BuildersWork.Opening to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("familyInstance", "Revit FamilyInstance resulting from converting the input BH.oM.Architecture.BuildersWork.Opening.")]
        public static Element ToRevit(this BH.oM.Architecture.BuildersWork.Opening opening, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return opening.ToRevitFamilyInstance(document, settings, refObjects);
        }

        /***************************************************/

        [Description("Converts BH.oM.Adapters.Revit.Elements.Assembly to a Revit AssemblyInstance.")]
        [Input("assembly", "BH.oM.Adapters.Revit.Elements.Assembly to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("assemblyInstance", "Revit AssemblyInstance resulting from converting the input BH.oM.Adapters.Revit.Elements.Assembly.")]
        public static Element ToRevit(this Assembly assembly, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return assembly.ToRevitAssembly(document, settings, refObjects);
        }

        /***************************************************/

        [Description("Converts BH.oM.Adapters.Revit.Elements.Group to a Revit Group.")]
        [Input("group", "BH.oM.Adapters.Revit.Elements.Group to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("group", "Revit Group resulting from converting the input BH.oM.Adapters.Revit.Elements.Group.")]
        public static Element ToRevit(this BH.oM.Adapters.Revit.Elements.Group group, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return group.ToRevitGroup(document, settings, refObjects);
        }


        /***************************************************/
        /****             Disallowed Types              ****/
        /***************************************************/

        [Description("Raises an error about converting BH.oM.Structure.Elements.Bar being not supported and returns null.")]
        [Input("bar", "BH.oM.Structure.Elements.Bar to be attempted to convert.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("element", "Always null because of converting BH.oM.Structure.Elements.Bar being not supported.")]
        public static Element ToRevit(this oM.Structure.Elements.Bar bar, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            bar.ConvertBeforePushError(typeof(oM.Physical.Elements.IFramingElement));
            return null;
        }

        /***************************************************/

        [Description("Raises an error about converting BH.oM.Structure.Elements.Panel being not supported and returns null.")]
        [Input("panel", "BH.oM.Structure.Elements.Panel to be attempted to convert.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("element", "Always null because of converting BH.oM.Structure.Elements.Panel being not supported.")]
        public static Element ToRevit(this oM.Structure.Elements.Panel panel, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            panel.ConvertBeforePushError(typeof(oM.Physical.Elements.ISurface));
            return null;
        }

        /***************************************************/

        [Description("Raises an error about converting BH.oM.Environment.Elements.Panel being not supported and returns null.")]
        [Input("panel", "BH.oM.Environment.Elements.Panel to be attempted to convert.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("element", "Always null because of converting BH.oM.Environment.Elements.Panel being not supported.")]
        public static Element ToRevit(this oM.Environment.Elements.Panel panel, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            panel.ConvertBeforePushError(typeof(oM.Physical.Elements.ISurface));
            return null;
        }


        /***************************************************/
        /****             Fallback Methods              ****/
        /***************************************************/

        private static Element ToRevit(this IBHoMObject obj, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            return null;
        }


        /***************************************************/
        /****             Interface Methods             ****/
        /***************************************************/

        [Description("Interface method that tries to find a suitable ToRevit convert for any BHoM object and then call it.")]
        [Input("obj", "BHoM object to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("element", "Revit Element resulting from converting the input BHoM object.")]
        public static Element IToRevit(this IBHoMObject obj, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (obj == null)
            {
                BH.Engine.Base.Compute.RecordWarning("Revit Element could not be created because BHoM object does not exist.");
                return null;
            }

            Element result = ToRevit(obj as dynamic, document, settings, refObjects);
            if (result == null)
                obj.NotConvertedError();

            return result;
        }

        /***************************************************/
    }
}


