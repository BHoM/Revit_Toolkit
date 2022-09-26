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
using Autodesk.Revit.UI;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Requests;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Data.Requests;
using BH.oM.Base.Attributes;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****         Public methods - Requests         ****/
        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given FilterRequest.")]
        [Input("request", "FilterRequest containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this FilterRequest request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            IEnumerable<ElementId> elementIds = ids;

            object idObject;
            if (request.Equalities.TryGetValue("ObjectIds", out idObject) && idObject is IList)
            {
                IList list = idObject as IList;
                if (list != null)
                    elementIds = document.ElementIdsByIdObjects(list, elementIds);
            }

            if (request.Type != null)
                elementIds = document.ElementIdsByBHoMType(request.Type, settings, elementIds);

            if (!string.IsNullOrWhiteSpace(request.Tag))
                BH.Engine.Base.Compute.RecordError("Filtering based on tag declared in FilterRequest is currently not supported. The tag-related filter has not been applied.");

            return elementIds;
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given FilterByElementIds request.")]
        [Input("request", "FilterByElementIds request containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this FilterByElementIds request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsByInts(request.ElementIds, ids);
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given FilterByUniqueIds request.")]
        [Input("request", "FilterByUniqueIds request containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this FilterByUniqueIds request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsByUniqueIds(request.UniqueIds, ids);
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given FilterByCategory request.")]
        [Input("request", "FilterByCategory request containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this FilterByCategory request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsByCategory(request.CategoryName, request.CaseSensitive, ids);
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given FilterByDBTypeName request.")]
        [Input("request", "FilterByDBTypeName request containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this FilterByDBTypeName request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsByDBType("RevitAPI.dll", request.TypeName, ids);
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given FilterByActiveWorkset request.")]
        [Input("request", "FilterByActiveWorkset request containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this FilterByActiveWorkset request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            if (document.IsLinked)
            {
                BH.Engine.Base.Compute.RecordError("It is not allowed to combine active workset requests with link requests.");
                return null;
            }

            return document.ElementIdsByWorksets(new List<WorksetId> { document.ActiveWorksetId() }, ids);
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given FilterByWorkset request.")]
        [Input("request", "FilterByWorkset request containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this FilterByWorkset request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsByWorksets(new List<WorksetId> { document.WorksetId(request.WorksetName) }, ids);
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given SelectionRequest request.")]
        [Input("request", "SelectionRequest request containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this SelectionRequest request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            if (document.IsLinked)
            {
                BH.Engine.Base.Compute.RecordError("It is not allowed to combine selection requests with link requests - Revit selection does not work with links.");
                return null;
            }

            return new UIDocument(document).ElementIdsBySelection(ids);
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given FilterBySelectionSet request.")]
        [Input("request", "FilterBySelectionSet request containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this FilterBySelectionSet request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsBySelectionSet(request.SelectionSetName, true, ids);
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given IParameterRequest.")]
        [Input("request", "IParameterRequest containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this IParameterRequest request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            return document.IElementIdsByParameter(request, discipline, settings, ids);
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given FilterByFamilyAndTypeName request.")]
        [Input("request", "FilterByFamilyAndTypeName request containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this FilterByFamilyAndTypeName request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsByFamilyAndType(request.FamilyName, request.FamilyTypeName, request.CaseSensitive, ids);
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given FilterByFamilyType request.")]
        [Input("request", "FilterByFamilyType request containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this FilterByFamilyType request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsByElementType(request.FamilyTypeId, ids);
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given FilterFamilyTypeByName request.")]
        [Input("request", "FilterFamilyTypeByName request containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this FilterFamilyTypeByName request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsOfFamilyTypes(request.FamilyName, request.FamilyTypeName, request.CaseSensitive, ids);
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given FilterFamilyByName request.")]
        [Input("request", "FilterFamilyByName request containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this FilterFamilyByName request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsOfFamilies(request.FamilyName, request.CaseSensitive, ids);
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given FilterTypesOfFamily request.")]
        [Input("request", "FilterTypesOfFamily request containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this FilterTypesOfFamily request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsOfFamilyTypes(request.FamilyId, ids);
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given FilterActiveView request.")]
        [Input("request", "FilterActiveView request containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this FilterActiveView request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsOfActiveView(ids);
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given FilterByViewSpecific request.")]
        [Input("request", "FilterByViewSpecific request containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this FilterByViewSpecific request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsByViewSpecific(request.ViewId, ids);
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given FilterByVisibleInView request.")]
        [Input("request", "FilterByVisibleInView request containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this FilterByVisibleInView request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsByVisibleInView(request.ViewId, ids);
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given FilterByVisibleInActiveView request.")]
        [Input("request", "FilterByVisibleInActiveView request containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this FilterByVisibleInActiveView request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsByVisibleInActiveView(ids);
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given FilterViewByName request.")]
        [Input("request", "FilterViewByName request containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this FilterViewByName request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsOfViews(request.ViewName, request.CaseSensitive, ids);
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given FilterViewsByTemplate request.")]
        [Input("request", "FilterViewsByTemplate request containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this FilterViewsByTemplate request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsOfViews(request.TemplateId, ids);
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given FilterViewsByType request.")]
        [Input("request", "FilterViewsByType request containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this FilterViewsByType request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsOfViews(request.RevitViewType.ViewType(), ids);
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given FilterViewTemplateByName request.")]
        [Input("request", "FilterViewTemplateByName request containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this FilterViewTemplateByName request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsOfViewTemplates(request.TemplateName, request.CaseSensitive, ids);
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given FilterLinkInstance request.")]
        [Input("request", "FilterLinkInstance request containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this FilterLinkInstance request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsOfLinkInstances(request.LinkName, request.CaseSensitive, ids);
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given EnergyAnalysisModelRequest.")]
        [Input("request", "EnergyAnalysisModelRequest containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this EnergyAnalysisModelRequest request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsOfEnergyAnalysisModel(ids);
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given FilterMemberElements request.")]
        [Input("request", "FilterMemberElements request containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this FilterMemberElements request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsOfMemberElements(request.ParentId, ids);
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given FilterModelElements request.")]
        [Input("request", "FilterModelElements request containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this FilterModelElements request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsOfModelElements(ids);
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given FilterByUsage request.")]
        [Input("request", "FilterByUsage request containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this FilterByUsage request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsByUsage(request.Used, ids);
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given FilterByScopeBox request.")]
        [Input("request", "FilterByScopeBox request containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this FilterByScopeBox request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsByScopeBox(request.BoxName, ids);
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given FilterEverything request\n" +
                     "in practice, returns all ElementIds in the model or equivalent of ids input, if the latter is not null.")]
        [Input("request", "FilterEverything request containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this FilterEverything request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsOfEverything(ids);
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given LogicalAndRequest.")]
        [Input("request", "LogicalAndRequest containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this LogicalAndRequest request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            IEnumerable<ElementId> result;
            if (ids == null)
                result = null;
            else
                result = new HashSet<ElementId>(ids);

            foreach (IRequest subRequest in request.Requests.SortByPerformance())
            {
                result = subRequest.IElementIds(document, discipline, settings, result);
                if (result == null)
                    return null;
            }

            return result;
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given LogicalOrRequest.")]
        [Input("request", "LogicalOrRequest containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this LogicalOrRequest request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            HashSet<ElementId> result = new HashSet<ElementId>();
            foreach (IRequest subRequest in request.Requests)
            {
                IEnumerable<ElementId> subResult = subRequest.IElementIds(document, discipline, settings, ids);
                if (subResult == null)
                    return null;

                result.UnionWith(subResult);
            }

            return result;
        }

        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given LogicalNotRequest.")]
        [Input("request", "LogicalNotRequest containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> ElementIds(this LogicalNotRequest request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            IEnumerable<ElementId> toExclude = request.Request.IElementIds(document);
            if (toExclude == null)
                return null;

            return document.ElementIdsByExclusion(toExclude, ids);
        }


        /***************************************************/
        /****        Interface methods - Requests       ****/
        /***************************************************/

        [Description("Finds the ElementIds of all elements within the Revit document that pass the filtering criteria set in the given IRequest.")]
        [Input("request", "IRequest containing the filtering criteria, against which the elements in the Revit document are checked.")]
        [Input("document", "Revit Document queried for the filtered elements.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while evaluating the elements against the filtering criteria.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered Revit ElementIds.")]
        public static IEnumerable<ElementId> IElementIds(this IRequest request, Document document, Discipline discipline = Discipline.Undefined, RevitSettings settings = null, IEnumerable<ElementId> ids = null)
        {
            if (document == null || request == null)
                return null;

            return ElementIds(request as dynamic, document, discipline, settings, ids);
        }

        /***************************************************/
    }
}


