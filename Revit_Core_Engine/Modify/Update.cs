/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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

using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.MEP.Equipment.Parts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Color = Autodesk.Revit.DB.Color;
using OverrideGraphicSettings = Autodesk.Revit.DB.OverrideGraphicSettings;
using View = Autodesk.Revit.DB.View;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****             Interface Methods             ****/
        /***************************************************/

        [Description("Updates the existing Revit Element based on the given BHoM object.")]
        [Input("element", "Revit Element to be updated.")]
        [Input("bHoMObject", "BHoM object, based on which the Revit element will be updated.")]
        [Input("settings", "Revit adapter settings to be used while performing the action.")]
        [Input("setLocationOnUpdate", "If false, only parameters and properties of the Element will be updated, if true, its location will be updated too.")]
        [Output("success", "True if the Revit element has been updated successfully based on the input BHoM object.")]
        public static bool IUpdate(this Element element, IBHoMObject bHoMObject, RevitSettings settings, bool setLocationOnUpdate)
        {
            if (element == null)
            {
                BH.Engine.Base.Compute.RecordWarning("The element could not be updated because Revit element does not exist.");
                return false;
            }

            if (bHoMObject == null)
            {
                BH.Engine.Base.Compute.RecordWarning("The element could not be updated because BHoM object does not exist.");
                return false;
            }

            if (element.Pinned)
            {
                BH.Engine.Base.Compute.RecordError(String.Format("Element could not be updated because it is pinned. ElementId: {0}", element.Id));
                return false;
            }

            return Update(element as dynamic, bHoMObject as dynamic, settings, setLocationOnUpdate);
        }


        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Updates the existing Revit Element based on the given BHoM object.")]
        [Input("element", "Revit Element to be updated.")]
        [Input("bHoMObject", "BHoM object, based on which the Revit element will be updated.")]
        [Input("settings", "Revit adapter settings to be used while performing the action.")]
        [Input("setLocationOnUpdate", "If false, only parameters and properties of the Element will be updated, if true, its location will be updated too.")]
        [Output("success", "True if the Revit element has been updated successfully based on the input BHoM object.")]
        public static bool Update(this Element element, IBHoMObject bHoMObject, RevitSettings settings, bool setLocationOnUpdate)
        {
            bool isElement = new ElementIsElementTypeFilter(true).PassesFilter(element);
            if (isElement)
            {
                if (element.ISetType(bHoMObject, settings))
                    element.Document.Regenerate();
            }

            element.CopyParameters(bHoMObject, settings);

            if (!string.IsNullOrWhiteSpace(bHoMObject.Name) && element.Name != bHoMObject.Name)
            {
                try
                {
                    element.Name = bHoMObject.Name;
                }
                catch
                {

                }
            }

            if (setLocationOnUpdate && isElement)
                element.ISetLocation(bHoMObject, settings);

            return true;
        }

        /***************************************************/

        [Description("Updates the existing Revit FamilyInstance based on the given BHoM builders work Opening.")]
        [Input("element", "Revit FamilyInstance to be updated.")]
        [Input("bHoMObject", "BHoM builders work Opening, based on which the Revit element will be updated.")]
        [Input("settings", "Revit adapter settings to be used while performing the action.")]
        [Input("setLocationOnUpdate", "If false, only parameters and properties of the FamilyInstance will be updated, if true, its location will be updated too.")]
        [Output("success", "True if the Revit FamilyInstance has been updated successfully based on the input BHoM builders work Opening.")]
        public static bool Update(this FamilyInstance element, BH.oM.Architecture.BuildersWork.Opening bHoMObject, RevitSettings settings, bool setLocationOnUpdate)
        {
            if (element == null || bHoMObject == null)
                return false;

            bool success = ((Element)element).Update(bHoMObject, settings, setLocationOnUpdate);
            success |= element.SetDimensions(bHoMObject, settings);
            element.ErrorOnHostChange(bHoMObject);

            return success;
        }

        /***************************************************/

        [Description("Updates the existing Revit FamilyInstance based on the given BHoM ModelInstance.")]
        [Input("element", "Revit FamilyInstance to be updated.")]
        [Input("bHoMObject", "BHoM ModelInstance, based on which the Revit element will be updated.")]
        [Input("settings", "Revit adapter settings to be used while performing the action.")]
        [Input("setLocationOnUpdate", "If false, only parameters and properties of the FamilyInstance will be updated, if true, its location will be updated too.")]
        [Output("success", "True if the Revit FamilyInstance has been updated successfully based on the input BHoM ModelInstance.")]
        public static bool Update(this FamilyInstance element, ModelInstance bHoMObject, RevitSettings settings, bool setLocationOnUpdate)
        {
            if (element.ViewSpecific)
            {
                BH.Engine.Base.Compute.RecordError($"Updating drafting elements using ModelInstances is not allowed. Revit ElementId: {element.Id} BHoM_Guid: {bHoMObject.BHoM_Guid}");
                return false;
            }

            element.ErrorOnCategoryChange(bHoMObject);
            element.ErrorOnHostChange(bHoMObject);

            return ((Element)element).Update(bHoMObject, settings, setLocationOnUpdate);
        }

        /***************************************************/

        [Description("Updates the existing Revit FamilyInstance based on the given BHoM DraftingInstance.")]
        [Input("element", "Revit FamilyInstance to be updated.")]
        [Input("bHoMObject", "BHoM DraftingInstance, based on which the Revit element will be updated.")]
        [Input("settings", "Revit adapter settings to be used while performing the action.")]
        [Input("setLocationOnUpdate", "If false, only parameters and properties of the FamilyInstance will be updated, if true, its location will be updated too.")]
        [Output("success", "True if the Revit FamilyInstance has been updated successfully based on the input BHoM DraftingInstance.")]
        public static bool Update(this FamilyInstance element, DraftingInstance bHoMObject, RevitSettings settings, bool setLocationOnUpdate)
        {
            if (!element.ViewSpecific || element.OwnerViewId == null)
            {
                BH.Engine.Base.Compute.RecordError($"Updating model elements using DraftingInstances is not allowed. Revit ElementId: {element.Id} BHoM_Guid: {bHoMObject.BHoM_Guid}");
                return false;
            }

            element.ErrorOnCategoryChange(bHoMObject);

            if (!string.IsNullOrWhiteSpace(bHoMObject.ViewName) && (element.Document.GetElement(element.OwnerViewId) as View)?.Name != bHoMObject.ViewName)
                BH.Engine.Base.Compute.RecordWarning($"Updating the owner view of an existing Revit element is not allowed. Revit ElementId: {element.Id} BHoM_Guid: {bHoMObject.BHoM_Guid}");

            return ((Element)element).Update(bHoMObject, settings, setLocationOnUpdate);
        }

        /***************************************************/

        [Description("Updates the existing Revit Material based on the given BHoM Material.")]
        [Input("element", "Revit Material to be updated.")]
        [Input("bHoMObject", "BHoM Material, based on which the Revit element will be updated.")]
        [Input("settings", "Revit adapter settings to be used while performing the action.")]
        [Input("setLocationOnUpdate", "Revit Material does not have location property, therefore this parameter is irrelevant.")]
        [Output("success", "True if the Revit Material has been updated successfully based on the input BHoM Material.")]
        public static bool Update(this Material element, BH.oM.Physical.Materials.Material bHoMObject, RevitSettings settings, bool setLocationOnUpdate)
        {
            foreach (BH.oM.Physical.Materials.IMaterialProperties property in bHoMObject.Properties)
            {
                element.ICopyCharacteristics(property);
            }

            element.CopyParameters(bHoMObject, settings);

            if (!string.IsNullOrWhiteSpace(bHoMObject.Name) && element.Name != bHoMObject.Name)
            {
                try
                {
                    element.Name = bHoMObject.Name;
                }
                catch
                {

                }
            }

            return true;
        }

        /***************************************************/

        [Description("Updates the existing Revit Assembly instance based on the given BHoM Assembly.")]
        [Input("element", "Revit Assembly instance to be updated.")]
        [Input("bHoMObject", "BHoM Assembly, based on which the Revit element will be updated.")]
        [Input("settings", "Revit adapter settings to be used while performing the action.")]
        [Input("setLocationOnUpdate", "Revit Assembly instance does not have location property, therefore this parameter is irrelevant.")]
        [Output("success", "True if the Revit Assembly instance has been updated successfully based on the input BHoM Assembly.")]
        public static bool Update(this AssemblyInstance element, Assembly bHoMObject, RevitSettings settings, bool setLocationOnUpdate)
        {
            List<IBHoMObject> assemblyMembers = bHoMObject.AssemblyMembers();
            List<ElementId> memberElementIds = assemblyMembers.Select(x => x.ElementId()).Where(x => x != null).ToList();
            if (memberElementIds.Count == 0)
            {
                BH.Engine.Base.Compute.RecordError($"Update of the assembly failed because it does not have any valid member elements. BHoM_Guid: {bHoMObject.BHoM_Guid}");
                return false;
            }
            else if (memberElementIds.Count != assemblyMembers.Count)
                BH.Engine.Base.Compute.RecordWarning($"The assembly is missing some member elements. BHoM_Guid: {bHoMObject.BHoM_Guid}");

            element.SetMemberIds(memberElementIds);

            element.CopyParameters(bHoMObject, settings);

            if (string.IsNullOrWhiteSpace(bHoMObject.Name))
                BH.Engine.Base.Compute.RecordWarning($"Assembly type name could not be updated because an invalid name has been provided. ElementId: {element.Id}");
            else if (bHoMObject.Name != element.AssemblyTypeName)
            {
                if (new FilteredElementCollector(element.Document).OfClass(typeof(AssemblyInstance)).Cast<AssemblyInstance>().Any(x => x.Name == bHoMObject.Name))
                    BH.Engine.Base.Compute.RecordWarning($"Assembly type name could not be updated to '{bHoMObject.Name}' because other assembly type already uses the provided name. ElementId: {element.Id}");
                else
                    element.AssemblyTypeName = bHoMObject.Name;
            }

            return true;
        }

        /***************************************************/

        [Description("Updates the existing Revit View based on the given BHoM View.")]
        [Input("element", "Revit View instance to be updated.")]
        [Input("bHoMObject", "BHoM View, based on which the Revit View will be updated.")]
        [Input("settings", "Revit adapter settings to be used while performing the action.")]
        [Input("setLocationOnUpdate", "Revit View instance does not have location property, therefore this parameter is irrelevant.")]
        [Output("success", "True if the Revit View instance has been updated successfully based on the input BHoM View.")]
        public static bool Update(this View element, BH.oM.Adapters.Revit.Elements.View bHoMObject, RevitSettings settings, bool setLocationOnUpdate)
        {
            /* ADD FILTERS WITH OVERRIDES TO REVIT VIEW */

            // Via Streams...
            bHoMObject.FiltersWithOverrides
                // 1. Turn Filters and Overrides into Keys and Values of a Dictionary
                .ToDictionary(x => x.Filter, x => x.Overrides)
                // 2. Turn the Dictionary into a List of KeyValue Pairs (to allow use of ForEach()
                .ToList<KeyValuePair<ViewFilter, BH.oM.Adapters.Revit.Elements.OverrideGraphicSettings>>()
                // 3. Assign Filters to View and Set their Overrides
                .ForEach(kvp => { ParameterFilterElement pfe = kvp.Key.ToRevitParameterFilterElement(element.Document, settings);
                    // 3.1 Add ViewFilter to the View
                    element.AddFilter(pfe.Id);
                    // 3.2 Get the Revit FillPatternElement Objects for CutPattern and SurfacePattern
                    FillPatternElement revitCutPattern = new FilteredElementCollector(element.Document)
                        .OfClass(typeof(FillPatternElement))
                        .Cast<FillPatternElement>()
                        .FirstOrDefault(pattern => pattern.Name.Contains(kvp.Value.CutPattern.ToString()));
                    FillPatternElement revitSurfacePattern = new FilteredElementCollector(element.Document)
                        .OfClass(typeof(FillPatternElement))
                        .Cast<FillPatternElement>()
                        .FirstOrDefault(pattern => pattern.Name.Contains(kvp.Value.SurfacePattern.ToString()));
                    // 3.3 Create the OverrideGraphics by Properties
                    OverrideGraphicSettings overrideGraphicsSettings = new OverrideGraphicSettings();
                    Color revitLineColor = new Color(kvp.Value.LineColor.R, kvp.Value.LineColor.G, kvp.Value.LineColor.B);
                    Color revitCutColor = new Color(kvp.Value.CutColor.R, kvp.Value.CutColor.G, kvp.Value.CutColor.B);
                    Color revitSurfaceColor = new Color(kvp.Value.SurfaceColor.R, kvp.Value.SurfaceColor.G, kvp.Value.SurfaceColor.B);
                    overrideGraphicsSettings.SetCutLineColor(revitLineColor);
                    overrideGraphicsSettings.SetProjectionLineColor(revitLineColor);
                    overrideGraphicsSettings.SetCutBackgroundPatternId(revitCutPattern.Id);
                    overrideGraphicsSettings.SetCutBackgroundPatternColor(revitCutColor);
                    overrideGraphicsSettings.SetCutForegroundPatternId(revitCutPattern.Id);
                    overrideGraphicsSettings.SetCutForegroundPatternColor(revitCutColor);
                    overrideGraphicsSettings.SetSurfaceBackgroundPatternId(revitSurfacePattern.Id);
                    overrideGraphicsSettings.SetSurfaceBackgroundPatternColor(revitSurfaceColor);
                    overrideGraphicsSettings.SetSurfaceForegroundPatternId(revitSurfacePattern.Id);
                    overrideGraphicsSettings.SetSurfaceForegroundPatternColor(revitSurfaceColor);
                    // 3.4 Assign Overrides to the ViewFilter
                    element.SetFilterOverrides(pfe.Id, overrideGraphicsSettings); });

            element.CopyParameters(bHoMObject, settings);
            return true;
        }


        [Description("Updates the existing Revit View based on the given BHoM View.")]
        [Input("element", "Revit View instance to be updated.")]
        [Input("bHoMObject", "BHoM View, based on which the Revit View will be updated.")]
        [Input("settings", "Revit adapter settings to be used while performing the action.")]
        [Input("setLocationOnUpdate", "Revit View instance does not have location property, therefore this parameter is irrelevant.")]
        [Output("success", "True if the Revit View instance has been updated successfully based on the input BHoM View.")]
        public static bool Update(this View3D element, BH.oM.Adapters.Revit.Elements.View bHoMObject, RevitSettings settings, bool setLocationOnUpdate)
        {
            /* ADD FILTERS WITH OVERRIDES TO REVIT VIEW */

            // Via Streams...
            bHoMObject.FiltersWithOverrides
                // 1. Turn Filters and Overrides into Keys and Values of a Dictionary
                .ToDictionary(x => x.Filter, x => x.Overrides)
                // 2. Turn the Dictionary into a List of KeyValue Pairs (to allow use of ForEach()
                .ToList<KeyValuePair<ViewFilter, BH.oM.Adapters.Revit.Elements.OverrideGraphicSettings>>()
                // 3. Assign Filters to View and Set their Overrides
                .ForEach(kvp => {
                    ParameterFilterElement pfe = kvp.Key.ToRevitParameterFilterElement(element.Document, settings);
                    // 3.1 Add ViewFilter to the View
                    element.AddFilter(pfe.Id);
                    // 3.2 Get the Revit FillPatternElement Objects for CutPattern and SurfacePattern
                    FillPatternElement revitCutPattern = new FilteredElementCollector(element.Document)
                        .OfClass(typeof(FillPatternElement))
                        .Cast<FillPatternElement>()
                        .FirstOrDefault(pattern => pattern.Name.Contains(kvp.Value.CutPattern.ToString()));
                    FillPatternElement revitSurfacePattern = new FilteredElementCollector(element.Document)
                        .OfClass(typeof(FillPatternElement))
                        .Cast<FillPatternElement>()
                        .FirstOrDefault(pattern => pattern.Name.Contains(kvp.Value.SurfacePattern.ToString()));
                    // 3.3 Create the OverrideGraphics by Properties
                    OverrideGraphicSettings overrideGraphicsSettings = new OverrideGraphicSettings();
                    Color revitLineColor = new Color(kvp.Value.LineColor.R, kvp.Value.LineColor.G, kvp.Value.LineColor.B);
                    Color revitCutColor = new Color(kvp.Value.CutColor.R, kvp.Value.CutColor.G, kvp.Value.CutColor.B);
                    Color revitSurfaceColor = new Color(kvp.Value.SurfaceColor.R, kvp.Value.SurfaceColor.G, kvp.Value.SurfaceColor.B);
                    overrideGraphicsSettings.SetCutLineColor(revitLineColor);
                    overrideGraphicsSettings.SetProjectionLineColor(revitLineColor);
                    overrideGraphicsSettings.SetCutBackgroundPatternId(revitCutPattern.Id);
                    overrideGraphicsSettings.SetCutBackgroundPatternColor(revitCutColor);
                    overrideGraphicsSettings.SetCutForegroundPatternId(revitCutPattern.Id);
                    overrideGraphicsSettings.SetCutForegroundPatternColor(revitCutColor);
                    overrideGraphicsSettings.SetSurfaceBackgroundPatternId(revitSurfacePattern.Id);
                    overrideGraphicsSettings.SetSurfaceBackgroundPatternColor(revitSurfaceColor);
                    overrideGraphicsSettings.SetSurfaceForegroundPatternId(revitSurfacePattern.Id);
                    overrideGraphicsSettings.SetSurfaceForegroundPatternColor(revitSurfaceColor);
                    // 3.4 Assign Overrides to the ViewFilter
                    element.SetFilterOverrides(pfe.Id, overrideGraphicsSettings);
                });

            element.CopyParameters(bHoMObject, settings);
            return true;
        }

        /***************************************************/

        [Description("Updates the existing Revit ParameterFilterElement based on the given BHoM ViewFilter.")]
        [Input("element", "Revit ParameterFilterElement to be updated.")]
        [Input("bHoMObject", "BHoM ViewFilter, based on which the Revit element will be updated.")]
        [Input("settings", "Revit adapter settings to be used while performing the action.")]
        [Input("setLocationOnUpdate", "Revit ParameterFilterElement instance does not have location property, therefore this parameter is irrelevant.")]
        [Output("success", "True if the Revit ParameterFilterElement instance has been updated successfully based on the input BHoM ViewFilter.")]
        public static bool Update(this ParameterFilterElement element, BH.oM.Adapters.Revit.Elements.ViewFilter bHoMObject, RevitSettings settings, bool setLocationOnUpdate)
        {
            // 1. Collect the ElementIds of the filter's categories - via Streams
            List<ElementId> builtInCategoryIds= bHoMObject.Categories.Select(catName => { BuiltInCategory builtInCat;
                                                                                          Enum.TryParse<BuiltInCategory>(catName, out builtInCat);
                                                                                          return builtInCat; })
                                                                      .Select(builtInCat=> new ElementId(builtInCat))
                                                                      .ToList<ElementId>();
            // 2. Assign Categories' Ids to the ParameterFilterElement
            element.SetCategories(builtInCategoryIds);
            // 3. Assign Name to the ParameterFilterElement 
            element.Name = bHoMObject.Name;
            // 4. Assign Filter Rules to the ParameterFilterElement - via Streams
            element.SetElementFilter(new LogicalAndFilter(bHoMObject.Rules
                .Select(filterRule =>  Convert.filterRuleToRevit(element.Document, filterRule))
                .Select(revitFilterRule => new ElementParameterFilter(revitFilterRule))
                .Cast<ElementFilter>()
                .ToList()));

            // 5. Copy parameters to ParameterElementFilter
            element.CopyParameters(bHoMObject, settings);
            return true;
        }


        /***************************************************/
        /****             Disallowed Types              ****/
        /***************************************************/

        [Description("Throws an error on attempt to update the existing Revit Element based on a BHoM Bar.")]
        [Input("element", "Revit Element to be updated.")]
        [Input("bar", "BHoM Bar, based on which the Revit element will be updated.")]
        [Input("settings", "Revit adapter settings to be used while performing the action.")]
        [Input("setLocationOnUpdate", "If false, only parameters and properties of the Element will be updated, if true, its location will be updated too.")]
        [Output("success", "Always false because updating the Revit Element based on BHoM Bar is not allowed.")]
        public static bool Update(this Element element, oM.Structure.Elements.Bar bar, RevitSettings settings, bool setLocationOnUpdate)
        {
            bar.ConvertBeforePushError(typeof(oM.Physical.Elements.IFramingElement));
            return false;
        }

        /***************************************************/

        [Description("Throws an error on attempt to update the existing Revit Element based on a BHoM Panel.")]
        [Input("element", "Revit Element to be updated.")]
        [Input("panel", "BHoM Panel, based on which the Revit element will be updated.")]
        [Input("settings", "Revit adapter settings to be used while performing the action.")]
        [Input("setLocationOnUpdate", "If false, only parameters and properties of the Element will be updated, if true, its location will be updated too.")]
        [Output("success", "Always false because updating the Revit Element based on BHoM Panel is not allowed.")]
        public static bool Update(this Element element, oM.Structure.Elements.Panel panel, RevitSettings settings, bool setLocationOnUpdate)
        {
            panel.ConvertBeforePushError(typeof(oM.Physical.Elements.ISurface));
            return false;
        }

        /***************************************************/

        [Description("Throws an error on attempt to update the existing Revit Element based on a BHoM Panel.")]
        [Input("element", "Revit Element to be updated.")]
        [Input("panel", "BHoM Panel, based on which the Revit element will be updated.")]
        [Input("settings", "Revit adapter settings to be used while performing the action.")]
        [Input("setLocationOnUpdate", "If false, only parameters and properties of the Element will be updated, if true, its location will be updated too.")]
        [Output("success", "Always false because updating the Revit Element based on BHoM Panel is not allowed.")]
        public static bool Update(this Element element, oM.Environment.Elements.Panel panel, RevitSettings settings, bool setLocationOnUpdate)
        {
            panel.ConvertBeforePushError(typeof(oM.Physical.Elements.ISurface));
            return false;
        }


        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        private static void ErrorOnHostChange(this FamilyInstance element, IBHoMObject bHoMObject)
        {
            bool ok = true;
            Output<int, string> hostInfo = bHoMObject.HostInformation();
            int hostId = hostInfo.Item1;
            if (hostId == -1)
                ok = element.Host == null || element.Host is ReferencePlane;
            else
            {
                if (element.Host == null)
                    ok = false;
                else if (element.Host is RevitLinkInstance)
                    ok = hostId == element.HostFace?.LinkedElementId.IntegerValue && hostInfo.Item2 == (element.Document.GetElement(element.Host.GetTypeId()) as RevitLinkType)?.Name;
                else
                    ok = hostId == element.Host.Id.IntegerValue;
            }

            if (!ok)
                BH.Engine.Base.Compute.RecordWarning($"Updating the host of an existing hosted element is not allowed. Revit ElementId: {element.Id} BHoM_Guid: {bHoMObject.BHoM_Guid}");
        }

        /***************************************************/

        private static void ErrorOnCategoryChange(this FamilyInstance element, IInstance bHoMObject)
        {
            if (!string.IsNullOrWhiteSpace(bHoMObject.Properties?.CategoryName) && element.Category?.Name != bHoMObject.Properties.CategoryName)
                BH.Engine.Base.Compute.RecordWarning($"Updating the category of an existing Revit element is not allowed. Revit ElementId: {element.Id} BHoM_Guid: {bHoMObject.BHoM_Guid}");
        }

        /***************************************************/


      
    }
}





