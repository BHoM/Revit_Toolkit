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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****         Public methods - Requests         ****/
        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            IEnumerable<ElementId> elementIds = ids;

            object idObject;
            if (request.Equalities.TryGetValue("ObjectIds", out idObject) && idObject is IList)
            {
                IList list = idObject as IList;
                if (list != null)
                    elementIds = uIDocument.Document.ElementIdsByIdObjects(list, elementIds);
            }

            if (request.Type != null)
                elementIds = uIDocument.Document.ElementIdsByBHoMType(request.Type, elementIds);

            if (!string.IsNullOrWhiteSpace(request.Tag))
                BH.Engine.Reflection.Compute.RecordError("Filtering based on tag declared in FilterRequest is currently not supported. The tag-related filter has not been applied.");

            return elementIds;
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this ElementIdsRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByInts(request.ElementIds, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this UniqueIdsRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByUniqueIds(request.UniqueIds, ids);
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

        public static IEnumerable<ElementId> ElementIds(this SelectionSetRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsBySelectionSet(request.SelectionSetName, true, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this DBTypeNameRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByDBType("RevitAPI.dll", request.TypeName, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this CategoryRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByCategoryName(request.CategoryName, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FamilyRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByFamilyType(request.FamilyName, request.FamilyTypeName, true, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this ActiveWorksetRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByWorksets(new List<WorksetId> { uIDocument.Document.ActiveWorksetId() }, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this WorksetRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByWorksets(new List<WorksetId> { uIDocument.Document.WorksetId(request.WorksetName) }, ids);
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

        public static IEnumerable<ElementId> ElementIds(this ViewTemplateRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsViewTemplate(ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this ParameterExistsRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByParameterExistence(request.ParameterName, request.ParameterExists, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this ParameterNumberRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByParameter(request.ParameterName, request.NumberComparisonType, request.Value, request.Tolerance, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this ParameterIntegerRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByParameter(request.ParameterName, request.NumberComparisonType, request.Value, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this ParameterBoolRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByParameter(request.ParameterName, request.Value, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this ParameterElementIdRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByParameter(request.ParameterName, request.ElementId, ids);
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this ParameterTextRequest request, UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            return uIDocument.Document.ElementIdsByParameter(request.ParameterName, request.TextComparisonType, request.Value, ids);
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
