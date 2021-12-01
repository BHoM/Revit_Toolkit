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
using BH.Engine.Adapters.Revit;
using BH.Engine.Data;
using BH.oM.Adapter;
using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Requests;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Data.Requests;
using BH.Revit.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Adapter.Core
{
    public partial class RevitListenerAdapter
    {
        /***************************************************/
        /****             Protected methods             ****/
        /***************************************************/

        protected override int Delete(IRequest request, ActionConfig actionConfig = null)
        {
            if (request == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Deletion could not be executed because provided IRequest is null.");
                return 0;
            }

            if (request.AllRequestsOfType(typeof(FilterByLink)).Count != 0)
            {
                BH.Engine.Reflection.Compute.RecordError($"It is not allowed to remove objects from Revit links - please remove the requests of type {nameof(FilterByLink)} from the request.");
                return 0;
            }

            RevitSettings settings = RevitSettings.DefaultIfNull();

            Document document = this.Document;
            RevitRemoveConfig removeConfig = actionConfig as RevitRemoveConfig;

            Discipline? discipline = request.Discipline();
            if (discipline == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Conflicting disciplines have been detected inside the provided request.");
                return 0;
            }
            else if (discipline == Discipline.Undefined)
            {
                BH.Engine.Reflection.Compute.RecordNote($"Discipline has not been specified, default {Discipline.Physical} will be used.");
                discipline = Discipline.Physical;
            }

            IEnumerable<ElementId> worksetPrefilter = null;
            if (!removeConfig.IncludeClosedWorksets)
                worksetPrefilter = document.ElementIdsByWorksets(document.OpenWorksetIds().Union(document.SystemWorksetIds()).ToList());

            IEnumerable<ElementId> elementIds = request.IElementIds(document, discipline.Value, settings, worksetPrefilter).RemoveGridSegmentIds(document);

            List<ElementId> deletedIds = Delete(elementIds, document, removeConfig.RemovePinned);
            if (deletedIds == null)
                return 0;
            else
                return deletedIds.Count;
        }


        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static bool Delete(IBHoMObject bHoMObject, Document document)
        {
            ElementId elementId = BH.Revit.Engine.Core.Query.ElementId(bHoMObject);
            if (elementId == null)
            {
                string uniqueId = bHoMObject.UniqueId();
                if (!string.IsNullOrWhiteSpace(uniqueId))
                {
                    Element element = document.GetElement(uniqueId);
                    if (element != null)
                        elementId = element.Id;
                }
            }

            if (elementId == null || elementId.IntegerValue < 0)
                return false;

            return Delete(elementId, document, false).Count() != 0;
        }

        /***************************************************/

        public static List<ElementId> Delete(IEnumerable<ElementId> elementIds, Document document, bool removePinned)
        {
            if (elementIds == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit elements could not be deleted because element Ids are null.");
                return null;
            }

            List<ElementId> result = new List<ElementId>();
            foreach (ElementId elementId in elementIds)
            {
                result.AddRange(Delete(elementId, document, removePinned));
            }

            return result;
        }

        /***************************************************/

        public static IEnumerable<ElementId> Delete(ElementId elementId, Document document, bool deletePinned)
        {
            Element element = document.GetElement(elementId);
            if (element == null)
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("Element has not been deleted because it does not exist in the current model. ElementId: {0}", elementId));
                return new List<ElementId>();
            }
            else if(!deletePinned && element.Pinned)
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("Element has not been deleted because it is pinned. ElementId: {0}", elementId));
                return new List<ElementId>();
            }
            else
            {
                try
                {
                    return document.Delete(elementId);
                }
                catch (Exception e)
                {
                    BH.Engine.Reflection.Compute.RecordError(String.Format("Element has not been deleted due to a Revit error. Error message: {0} ElementId: {1}", e.Message, elementId));
                    return new List<ElementId>();
                }
            }
        }

        /***************************************************/
    }
}
