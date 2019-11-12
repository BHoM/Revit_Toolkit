/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.UI.Revit.Engine;
using BH.oM.Data.Requests;
using BH.oM.Revit.Requests;

namespace BH.UI.Revit.Adapter
{
    public partial class RevitUIAdapter
    {
        /***************************************************/
        /**** Protected Methods                         ****/
        /***************************************************/
        
        protected override IEnumerable<IBHoMObject> Read(Type type, IList ids)
        {
            if (Document == null)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be read because Revit Document is null.");
                return null;
            }

            if (type == null)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be read because provided type is null.");
                return null;
            }

            List<FilterRequest> aFilterRequestList = new List<FilterRequest>();
            if (ids != null && ids.Count > 0)
            {
                List<string> aUniqueIdList = new List<string>();
                List<int> aElementIdList = new List<int>();
                foreach (object aObject in ids)
                    if(aObject != null)
                    {
                        if (aObject is int)
                            aElementIdList.Add((int)aObject);
                        else if (aObject is string)
                            aUniqueIdList.Add((string)aObject);
                    }

                FilterRequest aFilterRequest_UniqueIds = null;
                FilterRequest aFilterRequest_ElementIds = null;

                if (aUniqueIdList.Count > 0)
                    aFilterRequest_UniqueIds = BH.Engine.Adapters.Revit.Create.SelectionFilterRequest(aUniqueIdList);

                if (aElementIdList.Count > 0)
                    aFilterRequest_ElementIds = BH.Engine.Adapters.Revit.Create.SelectionFilterRequest(aElementIdList);

                if (aFilterRequest_UniqueIds != null && aFilterRequest_ElementIds != null)
                {
                    aFilterRequestList.Add(BH.Engine.Adapters.Revit.Create.LogicalOrFilterRequest(new List<FilterRequest>() { aFilterRequest_ElementIds, aFilterRequest_UniqueIds }));
                }
                else
                {
                    if(aFilterRequest_UniqueIds != null)
                        aFilterRequestList.Add(aFilterRequest_UniqueIds);

                    if (aFilterRequest_ElementIds != null)
                        aFilterRequestList.Add(aFilterRequest_ElementIds);
                }

            }

            if(type != null)
            {
                aFilterRequestList.Add(new FilterRequest() {Type = type });
            }

            IEnumerable<IBHoMObject> aResult = new List<IBHoMObject>();

            if (aFilterRequestList == null || aFilterRequestList.Count == 0)
                return aResult;

            if (aFilterRequestList.Count == 1)
                aResult = Read(aFilterRequestList.First());
            else
                aResult = Read(BH.Engine.Adapters.Revit.Create.LogicalAndFilterRequest(aFilterRequestList));

            return aResult;
        }

        /***************************************************/
        protected override IEnumerable<IBHoMObject> Read(FilterRequest filterRequest)
        {
            Document aDocument = Document;

            if (aDocument == null)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be read because Revit Document is null.");
                return null;
            }

            if (filterRequest == null)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be read because provided FilterRequest is null.");
                return null;
            }

            //TODO: this is temporary solution. Any further calls in this method to FilterRequest shall be changed to IRequest
            FilterRequest aFilterRequest = filterRequest as FilterRequest;
            if (aFilterRequest == null)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be read because provided IRequest is not FilterRequest.");
                return null;
            }

            Autodesk.Revit.UI.UIDocument aUIDocument = UIDocument;

            List<IBHoMObject> aResult = new List<IBHoMObject>();

            Dictionary<ElementId, List<FilterRequest>> aFilterRequestDictionary = Query.FilterRequestDictionary(aFilterRequest, aUIDocument);
            if (aFilterRequestDictionary == null)
                return null;

        //    if (aDocument == null)
        //    {
        //        BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be read because Revit Document is null.");
        //        return null;
        //    }

        //    if (filterRequest == null)
        //    {
        //        BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be read because provided FilterRequest is null.");
        //        return null;
        //    }

        //    Autodesk.Revit.UI.UIDocument aUIDocument = null;

        //    if (aUIDocument == null)
        //        aUIDocument = UIDocument;


        //    return Query.IBHoMObjects(aUIDocument, filterRequest, RevitSettings);
        //}

        /***************************************************/

        public virtual IEnumerable<IBHoMObject> Read(IRequest request)
        {
            Document aDocument = Document;

            if (aDocument == null)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be read because Revit Document is null.");
                return null;
            }

            if (request == null)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be read because provided IRequest is null.");
                return null;
            }

            Autodesk.Revit.UI.UIDocument aUIDocument = null;

            if (request is LinkRequest)
            {
                LinkRequest aLinkRequest = request as LinkRequest;
                Document aDocument_Link = Query.LinkDocument(aDocument, aLinkRequest.Id);
                if (aDocument_Link != null)
                    aUIDocument = new Autodesk.Revit.UI.UIDocument(aDocument_Link);
            }

            if (aUIDocument == null)
                aUIDocument = UIDocument;


            return Query.IBHoMObjects(aUIDocument, request, RevitSettings);
        }
        /***************************************************/
    }
}