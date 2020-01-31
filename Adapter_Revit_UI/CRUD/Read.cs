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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.oM.Geometry;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Adapters.Revit.Elements;
using BH.UI.Revit.Engine;
using BH.oM.Data.Requests;
using BH.oM.Adapters.Revit.Properties;
using BH.oM.Adapter;

namespace BH.UI.Revit.Adapter
{
    public partial class RevitUIAdapter
    {
        /***************************************************/
        /****             Protected Methods             ****/
        /***************************************************/
        
        protected override IEnumerable<IBHoMObject> IRead(Type type, IList ids, ActionConfig actionConfig = null)
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

            List<IRequest> requestList = new List<IRequest>();
            if (ids != null && ids.Count > 0)
            {
                List<string> uniqueIDList = new List<string>();
                List<int> elementIDList = new List<int>();
                foreach (object obj in ids)
                    if (obj != null)
                    {
                        if (obj is int)
                            elementIDList.Add((int)obj);
                        else if (obj is string)
                            uniqueIDList.Add((string)obj);
                    }

                IRequest requestUniqueIDs = null;
                IRequest requestElementIDs = null;

                if (uniqueIDList.Count > 0)
                    requestUniqueIDs = BH.Engine.Adapters.Revit.Create.UniqueIdsRequest(uniqueIDList);

                if (elementIDList.Count > 0)
                    requestElementIDs = BH.Engine.Adapters.Revit.Create.ElementIdsRequest(elementIDList);

                if (requestUniqueIDs != null && requestElementIDs != null)
                    requestList.Add(BH.Engine.Data.Create.LogicalOrRequest(new List<IRequest>() { requestElementIDs, requestUniqueIDs }));
                else
                {
                    if(requestUniqueIDs != null)
                        requestList.Add(requestUniqueIDs);

                    if (requestElementIDs != null)
                        requestList.Add(requestElementIDs);
                }

            }

            if (type != null)
            {
                requestList.Add(new FilterRequest() {Type = type });
            }

            IEnumerable<IBHoMObject> result = new List<IBHoMObject>();

            if (requestList == null || requestList.Count == 0)
                return result;

            if (requestList.Count == 1)
                result = Read(requestList.First());
            else
                result = Read(BH.Engine.Data.Create.LogicalAndRequest(requestList));

            return result;
        }

        /***************************************************/

        protected override IEnumerable<IBHoMObject> Read(IRequest request, ActionConfig actionConfig = null)
        {
            Document document = this.Document;

            if (document == null)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be read because Revit Document is null.");
                return null;
            }

            if (request == null)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be read because provided IRequest is null.");
                return null;
            }

            Autodesk.Revit.UI.UIDocument uiDocument = UIDocument;
            List<IBHoMObject> result = new List<IBHoMObject>();

            Dictionary<ElementId, List<IRequest>> requestDictionary = Query.RequestDictionary(request, uiDocument);
            if (requestDictionary == null)
                return null;

            Dictionary<Discipline, PullSettings> dictionaryPullSettings = new Dictionary<Discipline, PullSettings>();

            RevitSettings revitSettings = RevitSettings;

            MapSettings mapSettings = RevitSettings.MapSettings;
            if (mapSettings.TypeMaps == null || mapSettings.TypeMaps.Count == 0)
                mapSettings = BH.Engine.Adapters.Revit.Query.DefaultMapSettings();

            List <int> elementIDList = new List<int>();
            foreach (KeyValuePair<ElementId, List<IRequest>> kvp in requestDictionary)
            {
                if (elementIDList.Contains(kvp.Key.IntegerValue))
                    continue;
                
                Element element = document.GetElement(kvp.Key);
                if (element == null)
                    continue;
                
                List<IRequest> requests;
                if (!requestDictionary.TryGetValue(element.Id, out requests))
                    continue;

                //TODO: move this to actionConfig?
                Discipline discipline = BH.Engine.Adapters.Revit.Query.Discipline(requests, revitSettings);

                PullSettings pullSettings = null;
                if (!dictionaryPullSettings.TryGetValue(discipline, out pullSettings))
                {
                    pullSettings = BH.Engine.Adapters.Revit.Create.PullSettings(discipline, mapSettings);
                    dictionaryPullSettings.Add(discipline, pullSettings);
                }

                IEnumerable<IBHoMObject> iBHoMObjects = Read(element, revitSettings, pullSettings);

                if (iBHoMObjects != null && iBHoMObjects.Count() > 0)
                { 
                    //Pull Element Edges
                    if (BH.Engine.Adapters.Revit.Query.PullEdges(requests))
                    {
                        Options options = new Options();
                        options.ComputeReferences = false;
                        options.DetailLevel = ViewDetailLevel.Fine;
                        options.IncludeNonVisibleObjects = BH.Engine.Adapters.Revit.Query.IncludeNonVisibleObjects(requests);
                        
                        foreach(IBHoMObject iBHoMObject in iBHoMObjects)
                        {
                            ElementId elementId = iBHoMObject.ElementId();
                            if (elementId == null || elementId == ElementId.InvalidElementId)
                                continue;

                            Element elementTemp = document.GetElement(elementId);
                            if (elementTemp == null)
                                continue;

                            iBHoMObject.CustomData[BH.Engine.Adapters.Revit.Convert.Edges] = elementTemp.Curves(options, pullSettings);
                        }
                    }

                    result.AddRange(iBHoMObjects);
                    elementIDList.Add(element.Id.IntegerValue);

                    //TODO: atm it has been glued to the existing code, should be done in a more structured way
                    if (element is MultiSegmentGrid)
                    {
                        foreach (ElementId elementId in (element as MultiSegmentGrid).GetGridIds())
                        {
                            if (elementIDList.Contains(elementId.IntegerValue))
                                result.RemoveAll(x => x.CustomData.ContainsKey(BH.Engine.Adapters.Revit.Convert.ElementId) && (int)x.CustomData[BH.Engine.Adapters.Revit.Convert.ElementId] != elementId.IntegerValue);
                            else
                                elementIDList.Add(elementId.IntegerValue);
                        }
                    }
                }
            }

            return result;
        }


        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        private static IEnumerable<IBHoMObject> Read(Element element, RevitSettings revitSettings, PullSettings pullSettings = null)
        {
            if (element == null || !element.IsValidObject)
                return new List<IBHoMObject>();

            object obj = null;
            bool converted = true;

            List<IBHoMObject> result = new List<IBHoMObject>();

            IEnumerable<Type> types = Query.BHoMTypes(element);
            if (types != null && types.Count() > 0)
            {
                try
                {
                    obj = Engine.Convert.ToBHoM(element as dynamic, pullSettings);
                }
                catch (Exception exception)
                {
                    BH.Engine.Reflection.Compute.RecordError(string.Format("BHoM object could not be properly converted becasue of missing ToBHoM method. Element Id: {0}, Element Name: {1}, Exception Message: {2}", element.Id.IntegerValue, element.Name, exception.Message));
                    converted = false;
                }
            }

            if (obj == null)
            {
                try
                {
                    IBHoMObject iBHoMObject = null;

                    if (element.Location is LocationPoint || element.Location is LocationCurve)
                    {
                        IGeometry iGeometry = null;
                        try
                        {
                            iGeometry = element.Location.ToBHoM(pullSettings);
                        }
                        catch (Exception exception)
                        {
                            BH.Engine.Reflection.Compute.RecordWarning(string.Format("Location of BHoM object could not be converted. Element Id: {0}, Element Name: {1}, Exception Message: {2}", element.Id.IntegerValue, element.Name, exception.Message));
                        }

                        if (iGeometry != null)
                        {
                            ElementType elementType = element.Document.GetElement(element.GetTypeId()) as ElementType;
                            if(elementType != null)
                            {
                                InstanceProperties objectProperties = Engine.Convert.ToBHoM(elementType, pullSettings) as InstanceProperties;
                                if(objectProperties != null)
                                {
                                    if (element.ViewSpecific)
                                        iBHoMObject = BH.Engine.Adapters.Revit.Create.DraftingInstance(objectProperties, element.Document.GetElement(element.OwnerViewId).Name, iGeometry as dynamic);
                                    else
                                        iBHoMObject = BH.Engine.Adapters.Revit.Create.ModelInstance(objectProperties, iGeometry as dynamic);
                                }
                            }
                        }
                    }

                    if (iBHoMObject == null && element is ElementType)
                        iBHoMObject = Engine.Convert.ToBHoM((ElementType)element, pullSettings);
                    if(iBHoMObject == null && element is Autodesk.Revit.DB.Family)
                        iBHoMObject = Engine.Convert.ToBHoM((Autodesk.Revit.DB.Family)element, pullSettings);

                    if (iBHoMObject == null)
                        iBHoMObject = new BHoMObject();

                    if (iBHoMObject != null)
                    {
                        if (!(iBHoMObject is DraftingInstance) && element.ViewSpecific)
                            iBHoMObject = iBHoMObject.SetCustomData(BH.Engine.Adapters.Revit.Convert.ViewName, element.Document.GetElement(element.OwnerViewId).Name);

                        iBHoMObject.Name = element.Name;
                        iBHoMObject = Modify.SetIdentifiers(iBHoMObject, element);
                        iBHoMObject = Modify.SetCustomData(iBHoMObject, element);
                        obj = iBHoMObject;
                    }

                }
                catch (Exception exception)
                {
                    if (converted)
                        BH.Engine.Reflection.Compute.RecordError(string.Format("BHoM object could not be properly converted. Element Id: {0}, Element Name: {1}, Exception Message: {2}", element.Id.IntegerValue, element.Name, exception.Message));
                }
            }

            if (obj != null)
            {
                result = new List<IBHoMObject>();
                if (obj is IBHoMObject)
                    result.Add((IBHoMObject)obj);
                else if (obj is IEnumerable<IBHoMObject>)
                    result.AddRange((IEnumerable<IBHoMObject>)obj);
            }

            //Assign Tags
            string tagsParameterName = null;
            if (revitSettings != null && revitSettings.GeneralSettings != null)
                tagsParameterName = revitSettings.GeneralSettings.TagsParameterName;

            if (result != null && !string.IsNullOrEmpty(tagsParameterName))
            {
                for (int i = 0; i < result.Count; i++)
                {
                    IBHoMObject iBHoMObject = Modify.SetTags(result[i], element, tagsParameterName);
                    if (iBHoMObject != null)
                        result[i] = iBHoMObject;
                }
            }

            return result;
        }

        /***************************************************/
    }
}