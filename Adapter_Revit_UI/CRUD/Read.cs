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
using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Adapters.Revit.Elements;
using BH.UI.Revit.Engine;
using BH.oM.Data.Requests;
using BH.oM.Adapters.Revit.Properties;
using BH.oM.Adapter;
using BH.Engine.Adapters.Revit;

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

            List<int> elementIds = new List<int>();
            List<string> uniqueIds = new List<string>();
            if (ids != null)
            {
                foreach (object obj in ids)
                {
                    if (obj is int)
                        elementIds.Add((int)obj);
                    else if (obj is string)
                    {
                        string stringId = (string)obj;
                        int id;
                        if (int.TryParse(stringId, out id))
                            elementIds.Add(id);
                        else
                            uniqueIds.Add(stringId);
                    }
                }
            }

            if (elementIds.Count == 0 && uniqueIds.Count == 0)
                return Read(new FilterRequest() { Type = type } as IRequest, actionConfig);
            else
            {
                ByElementIdsRequest elementIdsRequest = new ByElementIdsRequest { ElementIds = elementIds };
                ByUniqueIdsRequest uniqueIdsRequest = new ByUniqueIdsRequest { UniqueIds = uniqueIds };
                return Read(BH.Engine.Data.Create.LogicalAndRequest(new FilterRequest() { Type = type }, BH.Engine.Data.Create.LogicalOrRequest(elementIdsRequest, uniqueIdsRequest)), actionConfig);
            }
        }

        /***************************************************/

        protected override IEnumerable<IBHoMObject> Read(IRequest request, ActionConfig actionConfig = null)
        {
            Autodesk.Revit.UI.UIDocument uiDocument = this.UIDocument;
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

            RevitConfig revitConfig = actionConfig as RevitConfig;
            if (revitConfig == null)
            {
                BH.Engine.Reflection.Compute.RecordWarning("Revit Config has not been specified. Default Revit Config is used.");
                revitConfig = new RevitConfig();
            }

            IEnumerable<ElementId> worksetPrefilter = null;
            if (!revitConfig.IncludeClosedWorksets)
                worksetPrefilter = document.ElementIdsByWorksets(document.OpenWorksetIds().Union(document.SystemWorksetIds()).ToList());

            List<ElementId> elementIds = request.IElementIds(uiDocument, worksetPrefilter).ToList();
            if (elementIds == null)
                return null;
            
            bool pullEdges = revitConfig.PullEdges;
            bool includeNonVisible = revitConfig.IncludeNonVisible;

            Discipline? requestDiscipline = request.Discipline(revitConfig.Discipline);
            if (requestDiscipline == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Conflicting disciplines have been detected.");
                return null;
            }

            Discipline discipline = requestDiscipline.Value;
            if (discipline == Discipline.Undefined)
                discipline = Discipline.Physical;

            RevitSettings revitSettings = RevitSettings;
            MapSettings mapSettings = RevitSettings.MapSettings;
            if (mapSettings.TypeMaps == null || mapSettings.TypeMaps.Count == 0)
                mapSettings = BH.Engine.Adapters.Revit.Query.DefaultMapSettings();
            
            List<IBHoMObject> result = new List<IBHoMObject>();
            List <int> elementIDList = new List<int>();
            foreach (ElementId id in elementIds)
            {
                Element element = document.GetElement(id);
                if (element == null)
                    continue;
                
                PullSettings pullSettings = BH.Engine.Adapters.Revit.Create.PullSettings(discipline, mapSettings);

                //TODO: PullEdges to happen here based on ActionConfig?
                IEnumerable<IBHoMObject> iBHoMObjects = Read(element, revitSettings, pullSettings);

                if (iBHoMObjects != null && iBHoMObjects.Count() != 0)
                { 
                    //Pull Element Edges
                    if (pullEdges)
                    {
                        Options options = new Options();
                        options.ComputeReferences = false;
                        options.DetailLevel = ViewDetailLevel.Fine;
                        options.IncludeNonVisibleObjects = includeNonVisible;
                        List<ICurve> edges = element.Curves(options, pullSettings);
                        
                        foreach (IBHoMObject iBHoMObject in iBHoMObjects)
                        {
                            iBHoMObject.CustomData[BH.Engine.Adapters.Revit.Convert.Edges] = edges;
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

            IEnumerable<Type> types = element.BHoMTypes();
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
                            if (elementType != null)
                            {
                                InstanceProperties objectProperties = Engine.Convert.ToBHoM(elementType, pullSettings) as InstanceProperties;
                                if (objectProperties != null)
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
                        iBHoMObject = ((ElementType)element).ToBHoM(pullSettings);

                    if (iBHoMObject == null && element is Autodesk.Revit.DB.Family)
                        iBHoMObject = ((Autodesk.Revit.DB.Family)element).ToBHoM(pullSettings);

                    if (iBHoMObject == null)
                        iBHoMObject = new BHoMObject();

                    if (!(iBHoMObject is DraftingInstance) && element.ViewSpecific)
                        iBHoMObject = iBHoMObject.SetCustomData(BH.Engine.Adapters.Revit.Convert.ViewName, element.Document.GetElement(element.OwnerViewId).Name);

                    iBHoMObject.Name = element.Name;
                    iBHoMObject = iBHoMObject.SetIdentifiers(element);
                    iBHoMObject = iBHoMObject.SetCustomData(element);
                    obj = iBHoMObject;
                }
                catch (Exception exception)
                {
                    if (converted)
                        BH.Engine.Reflection.Compute.RecordError(string.Format("BHoM object could not be properly converted. Element Id: {0}, Element Name: {1}, Exception Message: {2}", element.Id.IntegerValue, element.Name, exception.Message));
                }
            }

            List<IBHoMObject> result = new List<IBHoMObject>();
            if (obj != null)
            {
                if (obj is IBHoMObject)
                    result.Add((IBHoMObject)obj);
                else if (obj is IEnumerable<IBHoMObject>)
                    result.AddRange((IEnumerable<IBHoMObject>)obj);
            }

            //Assign Tags
            string tagsParameterName = null;
            if (revitSettings != null && revitSettings.GeneralSettings != null)
                tagsParameterName = revitSettings.GeneralSettings.TagsParameterName;

            if (!string.IsNullOrEmpty(tagsParameterName))
            {
                for (int i = 0; i < result.Count; i++)
                {
                    IBHoMObject iBHoMObject = result[i].SetTags(element, tagsParameterName);
                    if (iBHoMObject != null)
                        result[i] = iBHoMObject;
                }
            }

            return result;
        }

        /***************************************************/
    }
}