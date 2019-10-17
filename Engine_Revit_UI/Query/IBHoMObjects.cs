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
using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.oM.Geometry;
using BH.oM.Data.Requests;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Adapters.Revit.Properties;
using BH.oM.Adapters.Revit.Elements;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static IEnumerable<IBHoMObject> IBHoMObjects(this Autodesk.Revit.UI.UIDocument uIDocument, IRequest request, RevitSettings RevitSettings = null)
        {
            if (uIDocument == null || request == null)
                return null;

            List<IBHoMObject> aResult = new List<IBHoMObject>();

            Document aDocument = uIDocument.Document;

            Dictionary<ElementId, List<IRequest>> aIRequestDictionary = Query.IRequestDictionary(request, uIDocument);
            if (aIRequestDictionary == null)
                return null;

            Dictionary<Discipline, PullSettings> aDictionary_PullSettings = new Dictionary<Discipline, PullSettings>();

            RevitSettings aRevitSettings = RevitSettings;
            if (aRevitSettings == null)
                aRevitSettings = new RevitSettings();

            MapSettings aMapSettings = RevitSettings.MapSettings;
            if (aMapSettings.TypeMaps == null || aMapSettings.TypeMaps.Count == 0)
                aMapSettings = BH.Engine.Adapters.Revit.Query.DefaultMapSettings();

            List<ElementId> aElementIdList = new List<ElementId>();
            foreach (KeyValuePair<ElementId, List<IRequest>> aKeyValuePair in aIRequestDictionary)
            {
                Element aElement = aDocument.GetElement(aKeyValuePair.Key);
                if (aElement == null || aElementIdList.Contains(aElement.Id))
                    continue;

                IEnumerable<IRequest> aIRequests = Query.IRequests(aIRequestDictionary, aElement.Id);
                if (aIRequests == null)
                    continue;

                Discipline aDiscipline = Query.Discipline(aIRequests, aRevitSettings);

                PullSettings aPullSettings = null;
                if (!aDictionary_PullSettings.TryGetValue(aDiscipline, out aPullSettings))
                {
                    aPullSettings = BH.Engine.Adapters.Revit.Create.PullSettings(aDiscipline, aMapSettings);
                    aDictionary_PullSettings.Add(aDiscipline, aPullSettings);
                }

                IEnumerable<IBHoMObject> aIBHoMObjects = IBHoMObjects(aElement, aRevitSettings, aPullSettings);

                if (aIBHoMObjects != null && aIBHoMObjects.Count() > 0)
                {
                    //Pull Element Edges
                    if (BH.Engine.Adapters.Revit.Query.PullEdges(aIRequests))
                    {
                        Options aOptions = new Options();
                        aOptions.ComputeReferences = false;
                        aOptions.DetailLevel = ViewDetailLevel.Fine;
                        aOptions.IncludeNonVisibleObjects = BH.Engine.Adapters.Revit.Query.IncludeNonVisibleObjects(aIRequests);

                        foreach (IBHoMObject aIBHoMObject in aIBHoMObjects)
                        {
                            ElementId aElementId = aIBHoMObject.ElementId();
                            if (aElementId == null || aElementId == Autodesk.Revit.DB.ElementId.InvalidElementId)
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

        public static IEnumerable<IBHoMObject> IBHoMObjects(Element element, RevitSettings revitSettings, PullSettings pullSettings = null)
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
                            if (aElementType != null)
                            {
                                InstanceProperties aObjectProperties = Convert.ToBHoM(aElementType, pullSettings) as InstanceProperties;
                                if (aObjectProperties != null)
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
                        aIBHoMObject = Convert.ToBHoM((ElementType)element, pullSettings);
                    if (aIBHoMObject == null && element is Autodesk.Revit.DB.Family)
                        aIBHoMObject = Convert.ToBHoM((Autodesk.Revit.DB.Family)element, pullSettings);

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