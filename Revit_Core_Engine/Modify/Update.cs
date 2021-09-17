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
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Reflection.Attributes;
using System;
using System.ComponentModel;

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
                BH.Engine.Reflection.Compute.RecordWarning("The element could not be updated because Revit element does not exist.");
                return false;
            }

            if (bHoMObject == null)
            {
                BH.Engine.Reflection.Compute.RecordWarning("The element could not be updated because BHoM object does not exist.");
                return false;
            }

            if (element.Pinned)
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("Element could not be updated because it is pinned. ElementId: {0}", element.Id));
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
                BH.Engine.Reflection.Compute.RecordError($"Updating drafting elements using ModelInstances is not allowed. Revit ElementId: {element.Id} BHoM_Guid: {bHoMObject.BHoM_Guid}");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(bHoMObject.Properties?.CategoryName) && element.Category?.Name != bHoMObject.Properties.CategoryName)
                BH.Engine.Reflection.Compute.RecordWarning($"Updating the category of an existing element is not allowed. Revit ElementId: {element.Id} BHoM_Guid: {bHoMObject.BHoM_Guid}");

            if (((element.Host == null || element.Host is ReferencePlane) && bHoMObject.HostId != -1) || (element.Host != null && !(element.Host is ReferencePlane) && element.Host.Id.IntegerValue != bHoMObject.HostId))
                BH.Engine.Reflection.Compute.RecordWarning($"Updating the host of an existing hosted element is not allowed. Revit ElementId: {element.Id} BHoM_Guid: {bHoMObject.BHoM_Guid}");

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
                BH.Engine.Reflection.Compute.RecordError($"Updating model elements using DraftingInstances is not allowed. Revit ElementId: {element.Id} BHoM_Guid: {bHoMObject.BHoM_Guid}");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(bHoMObject.Properties?.CategoryName) && element.Category?.Name != bHoMObject.Properties.CategoryName)
                BH.Engine.Reflection.Compute.RecordWarning($"Updating the category of an existing Revit element is not allowed. Revit ElementId: {element.Id} BHoM_Guid: {bHoMObject.BHoM_Guid}");

            if (!string.IsNullOrWhiteSpace(bHoMObject.ViewName) && (element.Document.GetElement(element.OwnerViewId) as View)?.Name != bHoMObject.ViewName)
                BH.Engine.Reflection.Compute.RecordWarning($"Updating the owner view of an existing Revit element is not allowed. Revit ElementId: {element.Id} BHoM_Guid: {bHoMObject.BHoM_Guid}");

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
        /****             Disallowed Types              ****/
        /***************************************************/

        [Description("Throws an error on attempt to update the existing Revit Element based on a BHoM Bar.")]
        [Input("element", "Revit Element to be updated.")]
        [Input("bHoMObject", "BHoM Bar, based on which the Revit element will be updated.")]
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
        [Input("bHoMObject", "BHoM Panel, based on which the Revit element will be updated.")]
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
        [Input("bHoMObject", "BHoM Panel, based on which the Revit element will be updated.")]
        [Input("settings", "Revit adapter settings to be used while performing the action.")]
        [Input("setLocationOnUpdate", "If false, only parameters and properties of the Element will be updated, if true, its location will be updated too.")]
        [Output("success", "Always false because updating the Revit Element based on BHoM Panel is not allowed.")]
        public static bool Update(this Element element, oM.Environment.Elements.Panel panel, RevitSettings settings, bool setLocationOnUpdate)
        {
            panel.ConvertBeforePushError(typeof(oM.Physical.Elements.ISurface));
            return false;
        }

        /***************************************************/
    }
}


