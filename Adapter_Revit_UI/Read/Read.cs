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
using BH.oM.Geometry;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Adapters.Revit.Elements;
using BH.UI.Revit.Engine;
using BH.oM.Data.Requests;
using BH.oM.Adapters.Revit.Properties;

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

        public override IEnumerable<IBHoMObject> Read(FilterRequest filterRequest)
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

            Autodesk.Revit.UI.UIDocument aUIDocument = UIDocument;

            List<IBHoMObject> aResult = new List<IBHoMObject>();

            Dictionary<ElementId, List<FilterRequest>> aFilterRequestDictionary = Query.FilterRequestDictionary(filterRequest, aUIDocument);
            if (aFilterRequestDictionary == null)
                return null;

            Dictionary<Discipline, PullSettings> aDictionary_PullSettings = new Dictionary<Discipline, PullSettings>();

            RevitSettings aRevitSettings = RevitSettings;

            MapSettings aMapSettings = RevitSettings.MapSettings;
            if (aMapSettings.TypeMaps == null || aMapSettings.TypeMaps.Count == 0)
                aMapSettings = BH.Engine.Adapters.Revit.Query.DefaultMapSettings();

            List <ElementId> aElementIdList = new List<ElementId>();
            foreach (KeyValuePair<ElementId, List<FilterRequest>> aKeyValuePair in aFilterRequestDictionary)
            {
                Element aElement = aDocument.GetElement(aKeyValuePair.Key);
                if (aElement == null || aElementIdList.Contains(aElement.Id))
                    continue;

                IEnumerable<FilterRequest> aFilterRequests = Query.FilterRequests(aFilterRequestDictionary, aElement.Id);
                if (aFilterRequests == null)
                    continue;

                Discipline aDiscipline = Query.Discipline(aFilterRequests, aRevitSettings);

                PullSettings aPullSettings = null;
                if (!aDictionary_PullSettings.TryGetValue(aDiscipline, out aPullSettings))
                {
                    aPullSettings = BH.Engine.Adapters.Revit.Create.PullSettings(aDiscipline, aMapSettings);
                    aDictionary_PullSettings.Add(aDiscipline, aPullSettings);
                }

                IEnumerable<IBHoMObject> aIBHoMObjects = Read(aElement, aRevitSettings, aPullSettings);

                if (aIBHoMObjects != null && aIBHoMObjects.Count() > 0)
                { 
                    //Pull Element Edges
                    if (BH.Engine.Adapters.Revit.Query.PullEdges(aFilterRequests))
                    {
                        Options aOptions = new Options();
                        aOptions.ComputeReferences = false;
                        aOptions.DetailLevel = ViewDetailLevel.Fine;
                        aOptions.IncludeNonVisibleObjects = BH.Engine.Adapters.Revit.Query.IncludeNonVisibleObjects(aFilterRequests);

                        foreach(IBHoMObject aIBHoMObject in aIBHoMObjects)
                        {
                            ElementId aElementId = aIBHoMObject.ElementId();
                            if (aElementId == null || aElementId == ElementId.InvalidElementId)
                                continue;

                            Element aElement_Temp = aDocument.GetElement(aElementId);
                            if (aElement_Temp == null)
                                continue;

                            aIBHoMObject.CustomData[BH.Engine.Adapters.Revit.Convert.Edges] = aElement_Temp.Curves(aOptions, aPullSettings);
                        }
                    }

                    aResult.AddRange(aIBHoMObjects);
                    aElementIdList.Add(aElement.Id);
                }
            }

            return aResult;
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static IEnumerable<IBHoMObject> Read(Element element, RevitSettings revitSettings, PullSettings pullSettings = null)
        {
            if (element == null || !element.IsValidObject)
                return new List<IBHoMObject>();

            object aObject = null;
            bool aConverted = true;

            List<IBHoMObject> aResult = new List<IBHoMObject>();

            IEnumerable<Type> aTypes = Query.BHoMTypes(element);
            if (aTypes != null && aTypes.Count() > 0)
            {
                try
                {
                    aObject = Engine.Convert.ToBHoM(element as dynamic, pullSettings);
                }
                catch (Exception aException)
                {
                    BH.Engine.Reflection.Compute.RecordError(string.Format("BHoM object could not be properly converted becasue of missing ToBHoM method. Element Id: {0}, Element Name: {1}, Exception Message: {2}", element.Id.IntegerValue, element.Name, aException.Message));
                    aConverted = false;
                }
            }

            if (aObject == null)
            {
                try
                {
                    IBHoMObject aIBHoMObject = null;

                    if (element.Location is LocationPoint || element.Location is LocationCurve)
                    {
                        IGeometry aIGeometry = null;
                        try
                        {
                            aIGeometry = element.Location.ToBHoM(pullSettings);
                        }
                        catch (Exception aException)
                        {
                            BH.Engine.Reflection.Compute.RecordWarning(string.Format("Location of BHoM object could not be converted. Element Id: {0}, Element Name: {1}, Exception Message: {2}", element.Id.IntegerValue, element.Name, aException.Message));
                        }

                        if (aIGeometry != null)
                        {
                            ElementType aElementType = element.Document.GetElement(element.GetTypeId()) as ElementType;
                            if(aElementType != null)
                            {
                                InstanceProperties aObjectProperties = Engine.Convert.ToBHoM(aElementType, pullSettings) as InstanceProperties;
                                if(aObjectProperties != null)
                                {
                                    if (element.ViewSpecific)
                                        aIBHoMObject = BH.Engine.Adapters.Revit.Create.DraftingInstance(aObjectProperties, element.Document.GetElement(element.OwnerViewId).Name, aIGeometry as dynamic);
                                    else
                                        aIBHoMObject = BH.Engine.Adapters.Revit.Create.ModelInstance(aObjectProperties, aIGeometry as dynamic);
                                }
                            }
                        }
                    }

                    if (aIBHoMObject == null && element is ElementType)
                        aIBHoMObject = Engine.Convert.ToBHoM((ElementType)element, pullSettings);
                    if(aIBHoMObject == null && element is Autodesk.Revit.DB.Family)
                        aIBHoMObject = Engine.Convert.ToBHoM((Autodesk.Revit.DB.Family)element, pullSettings);

                    if (aIBHoMObject == null)
                        aIBHoMObject = new BHoMObject();

                    if (aIBHoMObject != null)
                    {
                        if (!(aIBHoMObject is DraftingInstance) && element.ViewSpecific)
                            aIBHoMObject = aIBHoMObject.SetCustomData(BH.Engine.Adapters.Revit.Convert.ViewName, element.Document.GetElement(element.OwnerViewId).Name);

                        aIBHoMObject.Name = element.Name;
                        aIBHoMObject = Modify.SetIdentifiers(aIBHoMObject, element);
                        aIBHoMObject = Modify.SetCustomData(aIBHoMObject, element, true);
                        aObject = aIBHoMObject;
                    }

                }
                catch (Exception aException)
                {
                    if (aConverted)
                        BH.Engine.Reflection.Compute.RecordError(string.Format("BHoM object could not be properly converted. Element Id: {0}, Element Name: {1}, Exception Message: {2}", element.Id.IntegerValue, element.Name, aException.Message));
                }
            }

            if (aObject != null)
            {
                aResult = new List<IBHoMObject>();
                if (aObject is IBHoMObject)
                    aResult.Add((IBHoMObject)aObject);
                else if (aObject is IEnumerable<IBHoMObject>)
                    aResult.AddRange((IEnumerable<IBHoMObject>)aObject);
            }

            //Assign Tags
            string aTagsParameterName = null;
            if (revitSettings != null && revitSettings.GeneralSettings != null)
                aTagsParameterName = revitSettings.GeneralSettings.TagsParameterName;

            if (aResult != null && !string.IsNullOrEmpty(aTagsParameterName))
            {
                for (int i = 0; i < aResult.Count; i++)
                {
                    IBHoMObject aIBHoMObject = Modify.SetTags(aResult[i], element, aTagsParameterName);
                    if (aIBHoMObject != null)
                        aResult[i] = aIBHoMObject;
                }
            }

            return aResult;
        }

        /***************************************************/
    }
}