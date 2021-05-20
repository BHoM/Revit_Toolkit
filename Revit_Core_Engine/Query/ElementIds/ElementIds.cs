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
using Autodesk.Revit.UI;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Requests;
using BH.oM.Data.Requests;
using System.Collections;
using System.Collections.Generic;


namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****         Public methods - Requests         ****/
        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterRequest request, Document document, IEnumerable<ElementId> ids = null)
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
                elementIds = document.ElementIdsByBHoMType(request.Type, elementIds);

            if (!string.IsNullOrWhiteSpace(request.Tag))
                BH.Engine.Reflection.Compute.RecordError("Filtering based on tag declared in FilterRequest is currently not supported. The tag-related filter has not been applied.");

            return elementIds;
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterByElementIds request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsByInts(request.ElementIds, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterByUniqueIds request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsByUniqueIds(request.UniqueIds, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterByCategory request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsByCategory(request.CategoryName, request.CaseSensitive, ids);
        }

        /***************************************************/
        
        public static IEnumerable<ElementId> ElementIds(this FilterByDBTypeName request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsByDBType("RevitAPI.dll", request.TypeName, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterByActiveWorkset request, Document document, IEnumerable<ElementId> ids = null)
        {
            if (document.IsLinked)
            {
                BH.Engine.Reflection.Compute.RecordError("It is not allowed to combine active workset requests with link requests.");
                return null;
            }

            return document.ElementIdsByWorksets(new List<WorksetId> { document.ActiveWorksetId() }, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterByWorkset request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsByWorksets(new List<WorksetId> { document.WorksetId(request.WorksetName) }, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this SelectionRequest request, Document document, IEnumerable<ElementId> ids = null)
        {
            if (document.IsLinked)
            {
                BH.Engine.Reflection.Compute.RecordError("It is not allowed to combine selection requests with link requests - Revit selection does not work with links.");
                return null;
            }

            return new UIDocument(document).ElementIdsBySelection(ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterBySelectionSet request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsBySelectionSet(request.SelectionSetName, true, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterByParameterExistence request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsByParameterExistence(request.ParameterName, request.ParameterExists, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterByParameterBool request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsByParameter(request.ParameterName, request.Value, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterByParameterElementId request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsByParameter(request.ParameterName, request.ElementId, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterByParameterInteger request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsByParameter(request.ParameterName, request.NumberComparisonType, request.Value, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterByParameterNumber request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsByParameter(request.ParameterName, request.NumberComparisonType, request.Value, request.Tolerance, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterByParameterText request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsByParameter(request.ParameterName, request.TextComparisonType, request.Value, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterByFamilyAndTypeName request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsByFamilyAndType(request.FamilyName, request.FamilyTypeName, request.CaseSensitive, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterByFamilyType request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsByElementType(request.FamilyTypeId, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterFamilyTypeByName request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsOfFamilyTypes(request.FamilyName, request.FamilyTypeName, request.CaseSensitive, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterFamilyByName request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsOfFamilies(request.FamilyName, request.CaseSensitive, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterTypesOfFamily request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsOfFamilyTypes(request.FamilyId, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterActiveView request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsOfActiveView(ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterByViewSpecific request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsByViewSpecific(request.ViewId, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterByVisibleInView request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsByVisibleInView(request.ViewId, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterViewByName request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsOfViews(request.ViewName, request.CaseSensitive, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterViewsByTemplate request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsOfViews(request.TemplateId, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterViewsByType request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsOfViews(request.RevitViewType.ViewType(), ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterViewTemplateByName request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsOfViewTemplates(request.TemplateName, request.CaseSensitive, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterLinkInstance request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsOfLinkInstances(request.LinkName, request.CaseSensitive, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this EnergyAnalysisModelRequest request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsOfEnergyAnalysisModel(ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterMemberElements request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsOfMemberElements(request.ParentId, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterModelElements request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsOfModelElements(ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterByUsage request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsByUsage(request.Used, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterByScopeBox request, Document document, IEnumerable<ElementId> ids = null)
        {
            return document.ElementIdsByScopeBox(request.BoxName, ids);
        }

        /***************************************************/


        public static IEnumerable<ElementId> ElementIds(this LogicalAndRequest request, Document document, IEnumerable<ElementId> ids = null)
        {
            IEnumerable<ElementId> result;
            if (ids == null)
                result = null;
            else
                result = new HashSet<ElementId>(ids);

            foreach (IRequest subRequest in request.Requests.SortByPerformance())
            {
                result = subRequest.IElementIds(document, result);
                if (result == null)
                    return null;
            }

            return result;
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this LogicalOrRequest request, Document document, IEnumerable<ElementId> ids = null)
        {
            HashSet<ElementId> result = new HashSet<ElementId>();
            foreach (IRequest subRequest in request.Requests)
            {
                IEnumerable<ElementId> subResult = subRequest.IElementIds(document, ids);
                if (subResult == null)
                    return null;

                result.UnionWith(subResult);
            }

            return result;
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this LogicalNotRequest request, Document document, IEnumerable<ElementId> ids = null)
        {
            IEnumerable<ElementId> toExclude = request.Request.IElementIds(document);
            if (toExclude == null)
                return null;

            return document.ElementIdsByExclusion(toExclude, ids);
        }


        /***************************************************/
        /****        Interface methods - Requests       ****/
        /***************************************************/

        public static IEnumerable<ElementId> IElementIds(this IRequest request, Document document, IEnumerable<ElementId> ids = null)
        {
            if (document == null || request == null)
                return null;

            return ElementIds(request as dynamic, document, ids);
        }

        /***************************************************/
    }
}

