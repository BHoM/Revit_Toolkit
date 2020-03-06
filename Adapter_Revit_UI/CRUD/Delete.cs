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
using BH.oM.Adapter;
using BH.oM.Adapters.Revit;
using BH.oM.Base;
using BH.oM.Data.Requests;
using BH.UI.Revit.Engine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Revit.Adapter
{
    public partial class RevitUIAdapter
    {
        /***************************************************/
        /****             Protected methods             ****/
        /***************************************************/

        protected override int Delete(IRequest request, ActionConfig actionConfig = null)
        {
            if (request == null)
            {
                BH.Engine.Reflection.Compute.RecordError("BHoM objects could not be read because provided IRequest is null.");
                return 0;
            }

            UIDocument uiDocument = this.UIDocument;
            Document document = this.Document;
            RevitRemoveConfig removeConfig = actionConfig as RevitRemoveConfig;

            IEnumerable<ElementId> worksetPrefilter = null;
            if (!removeConfig.IncludeClosedWorksets)
                worksetPrefilter = document.ElementIdsByWorksets(document.OpenWorksetIds().Union(document.SystemWorksetIds()).ToList());

            IEnumerable<ElementId> elementIds = request.IElementIds(uiDocument, worksetPrefilter).RemoveGridSegmentIds(document);

            List<ElementId> deletedIds = Delete(elementIds, document, removeConfig.RemovePinned);
            if (deletedIds == null)
                return 0;
            else
                return deletedIds.Count;
        }


        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        private static bool Delete(IBHoMObject bHoMObject, Document document)
        {
            ElementId elementId = Engine.Query.ElementId(bHoMObject);
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

        private static List<ElementId> Delete(IEnumerable<ElementId> elementIds, Document document, bool removePinned)
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

        private static IEnumerable<ElementId> Delete(ElementId elementId, Document document, bool deletePinned)
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