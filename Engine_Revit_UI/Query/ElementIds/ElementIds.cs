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
using Autodesk.Revit.UI;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit;
using BH.oM.Data.Requests;
using System.Collections.Generic;


namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****         Public methods - Requests         ****/
        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this ByActiveWorksetRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByWorksets(new List<WorksetId> { uIDocument.Document.ActiveWorksetId() }, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this ByCategoryRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByCategoryNames(request.CategoryName, ids);
        }

        /***************************************************/
        
        public static IEnumerable<ElementId> ElementIds(this ByDBTypeNameRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByDBType("RevitAPI.dll", request.TypeName, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this ByElementIdsRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByInts(request.ElementIds, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this ByFamilyRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByFamily(request.FamilyName, request.FamilyTypeName, true, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this ByParameterBoolRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByParameter(request.ParameterName, request.Value, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this ByParameterElementIdRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByParameter(request.ParameterName, request.ElementId, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this ByParameterExistsRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByParameterExistence(request.ParameterName, request.ParameterExists, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this ByParameterIntegerRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByParameter(request.ParameterName, request.NumberComparisonType, request.Value, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this ByParameterNumberRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByParameter(request.ParameterName, request.NumberComparisonType, request.Value, request.Tolerance, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this ByParameterTextRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByParameter(request.ParameterName, request.TextComparisonType, request.Value, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this BySelectionSetRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsBySelectionSet(request.SelectionSetName, true, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this ByUniqueIdsRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByUniqueIds(request.UniqueIds, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this ByWorksetRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByWorksets(new List<WorksetId> { uIDocument.Document.WorksetId(request.WorksetName) }, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this ElementsOnlyRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsOfElementsOnly(ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FamilyByNameRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsOfFamilies(request.FamilyName, true, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FamilyTypesOfFamilyRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsOfFamilyTypes(request.FamilyName, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByBHoMType(request.Type, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this LogicalAndRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            IEnumerable<ElementId> result;
            if (ids == null)
                result = null;
            else
                result = new HashSet<ElementId>(ids);

            foreach (IRequest subRequest in request.Requests.SortByPerformance())
            {
                result = subRequest.IElementIds(uIDocument, result);
            }

            return result;
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this LogicalOrRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            HashSet<ElementId> result = new HashSet<ElementId>();
            foreach (IRequest subRequest in request.Requests)
            {
                result.UnionWith(subRequest.IElementIds(uIDocument, ids));
            }

            return result;
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this SelectionRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            if (uIDocument.Selection == null)
                return null;

            HashSet<ElementId> result = new HashSet<ElementId>(uIDocument.Selection.GetElementIds());
            if (ids != null)
                result.IntersectWith(ids);

            return result;
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this ViewByTemplateRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByTemplate(request.TemplateName, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this ViewByTypeRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByViewType(request.RevitViewType.ViewType(), ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this ViewTemplateByNameRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsOfViewTemplates(request.TemplateName, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this VisibleInViewRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByVisibleInView(request.ViewName, true, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this EnergyAnalysisModelRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsEnergyAnalysisModel(ids);
        }

        /***************************************************/
        /****        Interface methods - Requests       ****/
        /***************************************************/

        public static IEnumerable<ElementId> IElementIds(this IRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            if (uIDocument == null || request == null)
                return null;

            return ElementIds(request as dynamic, uIDocument, ids);
        }

        /***************************************************/
    }
}
