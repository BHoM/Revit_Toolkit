/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using Autodesk.Revit.DB.Plumbing;
using BH.Engine.Adapters.Revit;
using BH.Engine.Base;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Geometry;
using BH.oM.MEP.System.MaterialFragments;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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

        [Description("Updates the existing Revit Pipe based on the given BHoM Pipe.")]
        [Input("element", "Revit Pipe to be updated.")]
        [Input("bHoMObject", "BHoM Pipe, based on which the Revit element will be updated.")]
        [Input("settings", "Revit adapter settings to be used while performing the action.")]
        [Input("setLocationOnUpdate", "If false, only parameters and properties of the FamilyInstance will be updated, if true, its location will be updated too.")]
        [Output("success", "True if the Revit Pipe has been updated successfully based on the input BHoM Pipe.")]
        public static bool Update(this Autodesk.Revit.DB.Plumbing.Pipe element, BH.oM.MEP.System.Pipe bHoMObject, RevitSettings settings, bool setLocationOnUpdate)
        {
            if (bHoMObject.SectionProperty?.PipeMaterial?.Properties?.FirstOrDefault(x => x is PipeMaterial) is PipeMaterial pipeMaterial)
            {
                PipeDesignData designData = pipeMaterial.FindFragment<PipeDesignData>();
                if (designData != null)
                {
                    PipeSegment pipeSegment = element.Document.PipeSegment(designData.Material, designData.ScheduleType);
                    if (pipeSegment == null)
                        pipeSegment = pipeMaterial.ToRevitPipeSegement(element.Document, settings);

                    if (pipeSegment != null)
                    {
                        if (element.SetParameter(BuiltInParameter.RBS_PIPE_SEGMENT_PARAM, pipeSegment.Id))
                            BH.Engine.Base.Compute.RecordNote($"PipeSegment {pipeSegment.Name} has been set to the updated pipe, but its values were not updated. To update PipeSegment itself, pull and update it separately.");
                        else
                            BH.Engine.Base.Compute.RecordWarning($"PipeSegment {pipeSegment.Name} could not be set on the pipe, default value has been used.");
                    }
                }
            }

            return ((Element)element).Update(bHoMObject, settings, setLocationOnUpdate);
        }

        /***************************************************/

        [Description("Updates the existing Revit PipeSegment based on the given BHoM PipeMaterial.")]
        [Input("element", "Revit PipeSegment to be updated.")]
        [Input("bHoMObject", "BHoM PipeMaterial, based on which the Revit element will be updated.")]
        [Input("settings", "Revit adapter settings to be used while performing the action.")]
        [Input("setLocationOnUpdate", "Revit PipeSegment does not have location property, therefore this parameter is irrelevant.")]
        [Output("success", "True if the Revit PipeSegment has been updated successfully based on the input BHoM PipeMaterial.")]
        public static bool Update(this PipeSegment element, PipeMaterial bHoMObject, RevitSettings settings, bool setLocationOnUpdate)
        {
            Document document = element.Document;

            element.CopyParameters(bHoMObject, settings);

            if (!string.IsNullOrWhiteSpace(bHoMObject.Name) && element.Name != bHoMObject.Name)
                element.Name = bHoMObject.Name;

            PipeDesignData designData = bHoMObject.FindFragment<PipeDesignData>();
            if (designData != null)
            {
                ForgeTypeId bHoMUnit = SpecTypeId.Length;
                double tolerance = Tolerance.MicroDistance; // Roughness and pipe diameter are very small, so we need to scale down the tolerance

                // Update Roughness
                if (Math.Abs(element.Roughness - bHoMObject.Roughness.FromSI(bHoMUnit)) > tolerance)
                {
                    element.Roughness = bHoMObject.Roughness.FromSI(bHoMUnit);
                }

                // Update Description
                element.Description = designData.Description;

                List<PipeSize> sizeSet = designData.SizeSet;
                if (sizeSet != null)
                {
                    oM.Base.Output<bool, List<PipeSize>> validated = sizeSet.Validate();
                    if (validated.Item1)
                    {
                        sizeSet = validated.Item2;

                        // Extract conversion data
                        List<MEPSize> mepSizes = new List<MEPSize>();
                        foreach (PipeSize size in sizeSet)
                        {
                            mepSizes.Add(new MEPSize(
                                size.NominalDiameter.FromSI(bHoMUnit),
                                size.InnerDiameter.FromSI(bHoMUnit),
                                size.OuterDiameter.FromSI(bHoMUnit),
                                usedInSizeLists: true, usedInSizing: true));
                        }

                        // Retrieve existing sizes
                        ICollection<MEPSize> existingSizes = element.GetSizes();
                        Dictionary<double, MEPSize> checkedSizes = existingSizes.ToDictionary(x => x.NominalDiameter, x => x);

                        // Update or Add
                        foreach (MEPSize newSize in mepSizes)
                        {
                            MEPSize similarSize = existingSizes.FirstOrDefault(x => Math.Abs(x.NominalDiameter - newSize.NominalDiameter) <= tolerance);
                            if (similarSize == null)
                            {
                                element.AddSize(newSize);
                            }
                            else
                            {
                                checkedSizes.Remove(similarSize.NominalDiameter);
                                if ((Math.Abs(similarSize.InnerDiameter - newSize.InnerDiameter) > tolerance) ||
                                         (Math.Abs(similarSize.OuterDiameter - newSize.OuterDiameter) > tolerance))
                                {
                                    element.RemoveSize(similarSize.NominalDiameter);
                                    element.AddSize(newSize);
                                }
                            }
                        }

                        // Clean oudated sizes
                        foreach (MEPSize size in checkedSizes.Values)
                        {
                            element.RemoveSize(size.NominalDiameter);
                        }
                    }
                    else
                        BH.Engine.Base.Compute.RecordWarning("Invalid size table has been found in the PipeMaterial.");
                }
                else
                    BH.Engine.Base.Compute.RecordNote("No size table has been found in the PipeMaterial. Sizes of PipeSegment could not be updated.");
            }
            else
                BH.Engine.Base.Compute.RecordNote("No PipeDesignData has been found in the PipeMaterial. Sizes, description and roughness of PipeSegment could not be updated.");

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






