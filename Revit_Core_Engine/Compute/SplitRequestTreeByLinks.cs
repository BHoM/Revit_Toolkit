﻿/*
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
using BH.Engine.Data;
using BH.oM.Adapters.Revit.Requests;
using BH.oM.Data.Requests;
using BH.oM.Reflection.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Compute
    {
        /***************************************************/
        /****              Public Methods               ****/
        /***************************************************/

        [Description("Decomposes a tree created by a set of nested ILogicalRequests into a dictionary of Revit documents (both host and linked) and the IRequests relevant to them, which in total represents the same request as the original IRequest.")]
        [Input("request", "An IRequest to be split into a dictionary of Revit documents and the IRequests relevant to them.")]
        [Input("document", "Host document to be used as the basis of the splitting routine.")]
        [Output("splitRequests", "A dictionary of Revit documents (both host and linked) and the IRequests relevant to them, which in total represents the same request as the input IRequest.")]
        public static Dictionary<Document, IRequest> SplitRequestTreeByLinks(this IRequest request, Document document)
        {
            List<RevitLinkInstance> linkInstances = new FilteredElementCollector(document).OfClass(typeof(RevitLinkInstance)).Cast<RevitLinkInstance>().ToList();

            Dictionary<Document, IRequest> requestsByLinks = new Dictionary<Document, IRequest>();
            List<IRequest> splitPerDoc = request.SplitRequestTreeByType(typeof(FilterByLink));
            foreach (IRequest splitRequest in splitPerDoc)
            {
                if (!splitRequest.TryOrganizeByLink(document, linkInstances, requestsByLinks))
                    return null;
            }

            return requestsByLinks;
        }


        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        private static bool TryOrganizeByLink(this IRequest request, Document document, List<RevitLinkInstance> linkInstances, Dictionary<Document, IRequest> requestsByLinks)
        {
            if (request == null)
                return false;

            if (request is FilterByLink)
            {
                BH.Engine.Reflection.Compute.RecordError($"It is not allowed to pull from links without any filtering (IRequest of type {nameof(FilterByLink)} needs to be wrapped into a {nameof(LogicalAndRequest)} with at least one more request).");
                return false;
            }
            else if (!request.IsValidToOrganize())
                return false;

            List<IRequest> linkRequests = request.AllRequestsOfType(typeof(FilterByLink));
            if (linkRequests.Count == 0)
            {
                requestsByLinks.AddRequestByLink(request, document);
                return true;
            }
            else if (linkRequests.Count == 1)
            {
                if (request.AllRequestsOfType(typeof(SelectionRequest)).Count != 0)
                {
                    BH.Engine.Reflection.Compute.RecordError("It is not allowed to combine selection requests with link requests - Revit selection does not work with links.");
                    return false;
                }

                if (request.AllRequestsOfType(typeof(FilterByActiveWorkset)).Count != 0)
                {
                    BH.Engine.Reflection.Compute.RecordError("It is not allowed to combine active workset requests with link requests - Revit selection does not work with links.");
                    return false;
                }

                if (request.AllRequestsOfType(typeof(FilterActiveView)).Count != 0)
                {
                    BH.Engine.Reflection.Compute.RecordError("It is not allowed to combine active view requests with link requests - Revit selection does not work with links.");
                    return false;
                }

                FilterByLink linkRequest = (FilterByLink)linkRequests[0];
                string linkName = linkRequest.LinkName.ToLower();
                if (!linkName.EndsWith(".rvt"))
                {
                    BH.Engine.Reflection.Compute.RecordWarning($"Link name {linkName} inside a link request does not end with .rvt - the suffix has been added.");
                    linkName += ".rvt";
                }

                bool fullPath = linkName.Contains("\\");
                List<RevitLinkInstance> validLinks;
                if (fullPath)
                    validLinks = linkInstances.Where(x => x.GetLinkDocument().PathName.ToLower() == linkName).ToList();
                else
                    validLinks = linkInstances.Where(x => (document.GetElement(x.GetTypeId()) as RevitLinkType)?.Name?.ToLower() == linkName).ToList();

                if (validLinks.Count == 0)
                {
                    if (fullPath)
                        BH.Engine.Reflection.Compute.RecordError($"Active Revit document does not contain link under path {linkRequest.LinkName}.");
                    else
                        BH.Engine.Reflection.Compute.RecordError($"Active Revit document does not contain link named {linkRequest.LinkName}.");

                    return false;
                }
                else if (validLinks.Count != 1)
                {
                    BH.Engine.Reflection.Compute.RecordError($"There is more than one link document named {linkName} - please use full link path in request instead of link name to pull.");
                    return false;
                }

                Document linkDoc = validLinks[0].GetLinkDocument();

                request.RemoveSubRequest(linkRequest);
                request = request.SimplifyRequestTree();
                requestsByLinks.AddRequestByLink(request, linkDoc);

                return true;
            }
            else
                return false;
        }

        /***************************************************/

        private static void AddRequestByLink(this Dictionary<Document, IRequest> requestsByLinks, IRequest request, Document document)
        {
            if (requestsByLinks.ContainsKey(document))
            {
                IRequest requestByLink = requestsByLinks[document];
                if (requestByLink is LogicalOrFilter)
                    ((LogicalOrRequest)requestByLink).Requests.Add(request);
                else
                {
                    LogicalOrRequest newHead = new LogicalOrRequest();
                    newHead.Requests.Add(requestByLink);
                    newHead.Requests.Add(request);
                    requestsByLinks[document] = newHead;
                }
            }
            else
                requestsByLinks[document] = request;
        }

        /***************************************************/

        private static bool RemoveSubRequest(this IRequest requestTree, IRequest toRemove)
        {
            if (requestTree is LogicalNotRequest)
            {
                LogicalNotRequest logical = (LogicalNotRequest)requestTree;
                if (logical.Request == toRemove)
                {
                    logical.Request = null;
                    return true;
                }
                else
                    return logical.Request.RemoveSubRequest(toRemove);
            }
            else if (requestTree is ILogicalRequest)
            {
                ILogicalRequest logical = (ILogicalRequest)requestTree;
                List<IRequest> subRequests = logical.IRequests();
                if (subRequests.Contains(toRemove))
                {
                    subRequests.Remove(toRemove);
                    return true;
                }
                else
                {
                    foreach (IRequest subRequest in subRequests)
                    {
                        bool removed = subRequest.RemoveSubRequest(toRemove);
                        if (removed)
                            return true;
                    }
                }
            }

            return false;
        }

        /***************************************************/

        private static bool IsValidToOrganize(this IRequest request)
        {
            if (request is LogicalNotRequest)
            {
                IRequest subRequest = ((LogicalNotRequest)request).Request;
                if (subRequest == null)
                    return false;

                if (subRequest.GetType() == typeof(FilterByLink))
                {
                    BH.Engine.Reflection.Compute.RecordError($"It is not allowed to wrap IRequests of type {nameof(FilterByLink)} into a {nameof(LogicalNotRequest)} request.");
                    return false;
                }
                else if (subRequest.AllRequestsOfType(typeof(LogicalNotRequest)).Count != 0)
                {
                    BH.Engine.Reflection.Compute.RecordError($"A chain of nested {nameof(LogicalNotRequest)}s has been detected, which is not allowed.");
                    return false;
                }
                else if (subRequest.AllRequestsOfType(typeof(FilterByLink)).Count != 0)
                {
                    BH.Engine.Reflection.Compute.RecordError($"A {nameof(FilterByLink)} nested in {nameof(LogicalNotRequest)} has been detected, which is not allowed.");
                    return false;
                }
                else
                    return subRequest.IsValidToOrganize();
            }
            else if (request is ILogicalRequest)
            {
                List<IRequest> subRequests = ((ILogicalRequest)request).IRequests();
                if (subRequests.Any(x => x.GetType() == typeof(FilterByLink))
                    && (request is LogicalOrRequest || (request is LogicalAndRequest && subRequests.Count == 1)))
                {
                    BH.Engine.Reflection.Compute.RecordError($"It is not allowed to pull from links without any filtering (IRequest of type {nameof(FilterByLink)} needs to be wrapped into a {nameof(LogicalAndRequest)}).");
                    return false;
                }

                return subRequests.All(x => x.IsValidToOrganize());
            }

            return true;
        }

        /***************************************************/
    }
}