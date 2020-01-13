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
using BH.oM.Base;
using BH.oM.Environment.Elements;
using BH.oM.Environment.Fragments;
using BH.UI.Revit.Engine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Revit.Adapter
{
    public partial class RevitUIAdapter
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/
        
        public static bool Delete(IEnumerable<BHoMObject> bHoMObjects, Document document)
        {
            if (document == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit objects could not be deleted because Revit Document is null.");
                return false;
            }

            if (bHoMObjects == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit objects could not be deleted because BHoM objects are null.");
                return false;
            }

            if (bHoMObjects.Count() < 1)
                return false;

            List<ElementId> elementIDList = Query.ElementIds(document, BH.Engine.Adapters.Revit.Query.UniqueIds(bHoMObjects, true), true);

            if (elementIDList == null || elementIDList.Count < 1)
                return false;

            bool result = false;
            using (Transaction transaction = new Transaction(document, "Create"))
            {
                transaction.Start();
                result = Delete(elementIDList, document);
                transaction.Commit();
            }
            return result;
        }

        /***************************************************/
        
        public static bool Delete(BHoMObject bHoMObject, Document document)
        {
            if (document == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit objects could not be deleted because Revit Document is null.");
                return false;
            }

            if (bHoMObject == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit objects could not be deleted because BHoM object is null.");
                return false;
            }

            bool result = false;
            using (Transaction transaction = new Transaction(document, "Create"))
            {
                transaction.Start();
                result = DeleteByUniqueId(bHoMObject, document);
                transaction.Commit();
            }
            return result;
        }

        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        private static bool DeleteByUniqueId(BHoMObject bHoMObject, Document document)
        {
            if(bHoMObject == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit objects could not be deleted because BHoM object is null.");
                return false;
            }

            string uniqueID = BH.Engine.Adapters.Revit.Query.UniqueId(bHoMObject);
            if (uniqueID != null)
            {
                Element element = document.GetElement(uniqueID);
                return Delete(element);
            }

            return false;
        }

        /***************************************************/

        private static bool DeleteByName(Type type, BHoMObject bHoMObject, Document document)
        {
            if (bHoMObject == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit objects could not be deleted because BHoM object is null.");
                return false;
            }

            if (type == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit objects could not be deleted because provided type is null.");
                return false;
            }

            List<Element> elementList = new FilteredElementCollector(document).OfClass(type).ToList();
            if (elementList == null || elementList.Count < 1)
                return false;

            Element element = elementList.Find(x => x.Name == bHoMObject.Name);
            return Delete(element);
        }

        /***************************************************/

        private static bool DeleteByName(Type type, IEnumerable<BHoMObject> bHoMObjects, Document document)
        {
            if (bHoMObjects == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit objects could not be deleted because BHoM objects are null.");
                return false;
            }

            if (type == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit objects could not be deleted because provided type is null.");
                return false;
            }

            if (bHoMObjects.Count() < 1)
                return false;

            List<Element> elementList = new FilteredElementCollector(document).OfClass(type).ToList();
            if (elementList == null || elementList.Count < 1)
                return false;

            List<ElementId> elementIDList = new List<ElementId>();
            foreach(BHoMObject bhomObject in bHoMObjects)
            {
                foreach(Element element in elementList)
                {
                    if(bhomObject.Name == element.Name)
                    {
                        elementIDList.Add(element.Id);
                        break;
                    }
                }
            }

            return Delete(elementIDList, document);
        }

        /***************************************************/

        private static bool Delete(Element element)
        {
            if(element == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit element could not be deleted because is null.");
                return false;
            }

            ICollection<ElementId> elementIDs = element.Document.Delete(element.Id);
            if (elementIDs != null && elementIDs.Count > 0)
                return true;

            return false;
        }

        /***************************************************/

        private static bool Delete(ICollection<ElementId> elementIds, Document document)
        {
            if (elementIds == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Revit elements could not be deleted because element Ids are null.");
                return false;
            }

            if (elementIds.Count() < 1)
                return false;

            List<ElementId> elementIDList = new List<ElementId>();

            foreach (ElementId elementId in elementIds)
                elementIDList.Add(elementId);

            ICollection<ElementId> elementIDs = document.Delete(elementIDList);
            if (elementIDs != null && elementIDs.Count > 0)
                return true;

            return false;
        }

        /***************************************************/
    }
}