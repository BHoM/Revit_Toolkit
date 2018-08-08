﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using BH.oM.Base;
using BH.Engine.Revit;
using BH.oM.Adapters.Revit;

using Autodesk.Revit.DB;

namespace BH.UI.Revit.Adapter
{
    /// <summary>
    /// BHoM RevitAdapter
    /// </summary>
    public partial class CobraAdapter
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/


        /***************************************************/
        /**** Protected Methods                         ****/
        /***************************************************/

        /// <summary>
        /// Generates BHoMObjects from Revit Elements by given BuiltInCategory
        /// </summary>
        /// <param name="builtInCategory">Revit BuiltInCategory</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="objects">BHoM Objects</param>
        /// <search>
        /// RevitAdapter, Read, Revit, BuiltInCategory, BHoMObject, BHoM, BHoMObjects
        /// </search>
        protected void Read(BuiltInCategory builtInCategory, Discipline discipline, List<IBHoMObject> objects)
        {
            Read(new BuiltInCategory[] { builtInCategory }, discipline, objects);
        }

        /***************************************************/

        /// <summary>
        /// Generates BHoMObjects from Revit Elements by given BuiltInCategory
        /// </summary>
        /// <param name="builtInCategories">Revit BuiltInCategories collection</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="objects">BHoM Objects</param>
        /// <search>
        /// RevitAdapter, Read, Revit, BuiltInCategory, BHoMObject, BHoM, BHoMObjects, BuiltInCategories
        /// </search>
        protected void Read(IEnumerable<BuiltInCategory> builtInCategories, Discipline discipline, List<IBHoMObject> objects)
        {
            if (Document == null)
            {
                Engine.Reflection.Compute.RecordError("BHoM objects could not be read because Revit Document is null.");
                return;
            }

            if (builtInCategories == null)
            {
                Engine.Reflection.Compute.RecordError("BHoM objects could not be read because revit Built-in categories is null.");
                return;
            }

            if (builtInCategories.Count() < 1)
                return;

            ICollection<ElementId> aElementIdList = new FilteredElementCollector(Document).WherePasses(new LogicalOrFilter(builtInCategories.ToList().ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter))).ToElementIds();

            Read(aElementIdList, discipline, objects);
        }

        /***************************************************/

        /// <summary>
        /// Generates BHoMObjects from Revit Element by given ElementId
        /// </summary>
        /// <param name="elementId">ElementId of Revit Element to be read</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="objects">BHoM Objects</param>
        /// <search>
        /// RevitAdapter, Read, Revit, ElementId, BHoMObjects, BHoM
        /// </search>
        protected void Read(ElementId elementId, Discipline discipline, List<IBHoMObject> objects, Dictionary<ElementId, List<IBHoMObject>> refObjects)
        {
            if (Document == null)
            {
                Engine.Reflection.Compute.RecordError("BHoM object could not be read because Revit Document is null.");
                return;
            }

            if (elementId == null)
            {
                Engine.Reflection.Compute.RecordError("BHoM object could not be read because Revit elementId is null.");
                return;
            }

            if (elementId == ElementId.InvalidElementId)
                return;

            Element aElement = Document.GetElement(elementId);

            if (aElement == null)
            {
                Engine.Reflection.Compute.RecordError(string.Format("BHoM object could not be read because Revit element with Id {0} does not exist.", elementId.IntegerValue));
                return;
            }

            if (!Query.AllowElement(RevitSettings, UIDocument, aElement))
                return;

            List<IBHoMObject> aResult = null;
            if (aElement is Floor)
            {
                aResult = Engine.Revit.Convert.ToBHoM(aElement as Floor, refObjects, discipline, true, true);
            }
            else if (aElement is RoofBase)
            {
                aResult = Engine.Revit.Convert.ToBHoM(aElement as RoofBase, discipline, true, true);
            }
            else if (aElement is Wall)
            {
                aResult = Engine.Revit.Convert.ToBHoM(aElement as Wall, refObjects, discipline, true, true);
            }
            else if (aElement is FamilyInstance)
            {
                aResult = new List<IBHoMObject> { Engine.Revit.Convert.ToBHoM(aElement as FamilyInstance, refObjects, discipline, true, true) };
            }
            else if (aElement is SpatialElement)
            {
                aResult = new List<IBHoMObject>();
                aResult.Add(Engine.Revit.Convert.ToBHoM(aElement as SpatialElement, refObjects, discipline, true, true));
            }
            else
            {
                object aObject = null;
                bool aConverted = true;

                List<Type> aTypeList = Query.BHoMTypes(aElement);
                if (aTypeList != null && aTypeList.Count > 0)
                {
                    try
                    {
                        aObject = Engine.Revit.Convert.ToBHoM(aElement as dynamic, discipline, true, true);
                    }
                    catch (Exception aException)
                    {
                        Engine.Reflection.Compute.RecordError(string.Format("BHoM object could not be properly converted becasue of missing ToBHoM method. Element Id: {0}, Element Name: {1}, Exception Message: {2}", elementId.IntegerValue, aElement.Name, aException.Message));
                        aConverted = false;
                    }
                }

                if(aObject == null)
                {
                    try
                    {
                        IBHoMObject aIBHoMObject = new BHoMObject();
                        aIBHoMObject = Modify.SetIdentifiers(aIBHoMObject, aElement);
                        aIBHoMObject = Modify.SetCustomData(aIBHoMObject, aElement, true);
                        aObject = aIBHoMObject;
                    }
                    catch (Exception aException)
                    {
                        if (aConverted)
                            Engine.Reflection.Compute.RecordError(string.Format("BHoM object could not be properly converted. Element Id: {0}, Element Name: {1}, Exception Message: {2}", elementId.IntegerValue, aElement.Name, aException.Message));
                    }
                }

                if(aObject != null)
                {
                    aResult = new List<IBHoMObject>();
                    if (aObject is BHoMObject)
                        aResult.Add(aObject as BHoMObject);
                    else if (aObject is List<IBHoMObject>)
                        aResult.AddRange(aObject as List<IBHoMObject>);
                }
            }

            if(aResult != null)
            {
                objects.AddRange(aResult);
            }
        }

        /***************************************************/

        /// <summary>
        /// Generates BHoMObjects from Revit Elements by given ElementIds
        /// </summary>
        /// <param name="elementIds">ElementIds of Revit Elements to be read</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="objects">BHoM Objects</param>
        /// <returns name="BHoMObjects">BHoMObject collection</returns>
        /// <search>
        /// RevitAdapter, Read, Revit, ElementId, BHoMObject, BHoM, BHoMObjects
        /// </search>
        protected void Read(IEnumerable<ElementId> elementIds, Discipline discipline, List<IBHoMObject> objects)
        {
            if (Document == null)
            {
                Engine.Reflection.Compute.RecordError("BHoM objects could not be read because Revit Document is null.");
                return;
            }

            if (elementIds == null)
            {
                Engine.Reflection.Compute.RecordError("BHoM objects could not be read because revit ElementIds are null.");
                return;
            }

            if (elementIds.Count() < 1)
                return;

            Dictionary<ElementId, List<IBHoMObject>> refObjects = new Dictionary<ElementId, List<IBHoMObject>>();
            foreach (ElementId aElementId in elementIds)
            {
                if(aElementId == null || aElementId == ElementId.InvalidElementId)
                {
                    Engine.Reflection.Compute.RecordError("BHoM object could not be read because Revit elementId is null or Invalid.");
                    continue;
                }

                try
                {
                    Read(aElementId, discipline, objects, refObjects);
                }
                catch (Exception e)
                {
                    Engine.Reflection.Compute.RecordError("Failed to read the element with the Revit ElementId: " + aElementId.IntegerValue.ToString() + ". \n Following error message was thrown: " + e.Message);
                }
            }
        }

        /***************************************************/

        /// <summary>
        /// Generates BHoMObjects from given class types. Class types have to inherits from Autodesk.Revit.DB.Element or BH.oM.Base.BHoMObject
        /// </summary>
        /// <param name="types">Revit or BHoM class types</param>
        /// <param name="uniqueIds">Revit element UniqueIds to be read. all object from given class type is read if uniqueIds collection equals to null</param>
        /// <param name="objects">BHoM Objects</param>
        /// <search>
        /// RevitAdapter, Read, Revit, BHoMObject, BHoM, BHoMObjects, 
        /// </search>
        protected void Read(IEnumerable<Type> types, List<IBHoMObject> objects, IEnumerable<string> uniqueIds = null)
        {
            if (Document == null)
            {
                Engine.Reflection.Compute.RecordError("BHoM objects could not be read because Revit Document is null.");
                return;
            }

            if (types == null)
            {
                Engine.Reflection.Compute.RecordError("BHoM objects could not be read because provided types are null.");
                return;
            }

            if (types.Count() < 1)
                return;

            //Get Revit class types
            List<Tuple<Type, List<BuiltInCategory>, Discipline>> aTupleList = new List<Tuple<Type, List<BuiltInCategory>, Discipline>>();
            foreach (Type aType in types)
            {
                if(aType == null)
                {
                    Engine.Reflection.Compute.RecordError("Provided type could not be read because is null.");
                    continue;
                }

                if (Query.IsAssignableFromByFullName(aType, typeof(Element)))
                {
                    if (aTupleList.Find(x => x.Item1 == aType) == null)
                        aTupleList.Add(new Tuple<Type, List<BuiltInCategory>, Discipline>(aType, new List<BuiltInCategory>(), Discipline.Environmental));
                }
                else if (Query.IsAssignableFromByFullName(aType, typeof(BHoMObject)))
                {
                    IEnumerable<Type> aTypes = Query.RevitTypes(aType);
                    if (aTypes == null || aTypes.Count() < 1)
                    {
                        Engine.Reflection.Compute.RecordError(string.Format("BHoM object could not be read because equivalent BHoM types do not exist. Type Name: {0}", aType.FullName));
                        continue;
                    }

                    foreach (Type aType_Temp in aTypes)
                        if (aTupleList.Find(x => x.Item1 == aType_Temp) == null)
                            aTupleList.Add(new Tuple<Type, List<BuiltInCategory>, Discipline>(aType_Temp, aType.BuiltInCategories(), aType.Discipline()));

                }
                else
                {
                    Engine.Reflection.Compute.RecordError(string.Format("Provided type is invalid. Type Name: {}", aType.FullName));
                    continue;
                }
            }

            if (aTupleList == null || aTupleList.Count < 1)
                return;

            foreach (Tuple<Type, List<BuiltInCategory>, Discipline> aTuple in aTupleList)
            {
                if(aTuple.Item1 == typeof(Document))
                {
                    objects.Add(Document.ToBHoM(aTuple.Item3, true));
                    continue;
                }

                FilteredElementCollector aFilteredElementCollector = null;
                if (aTuple.Item2 == null || aTuple.Item2.Count < 1)
                    aFilteredElementCollector = new FilteredElementCollector(Document).OfClass(aTuple.Item1);
                else
                    aFilteredElementCollector = new FilteredElementCollector(Document).OfClass(aTuple.Item1).WherePasses(new LogicalOrFilter(aTuple.Item2.ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter)));

                List<ElementId> aElementIdList = new List<ElementId>();
                foreach (Element aElement in aFilteredElementCollector)
                {
                    if (aElement == null)
                        continue;

                    if (uniqueIds == null || uniqueIds.Contains(aElement.UniqueId))
                    {
                        aElementIdList.Add(aElement.Id);
                        continue;
                    }

                    if(RevitSettings != null && RevitSettings.SelectionSettings != null)
                    {
                        IEnumerable<string> aUniqueIds = RevitSettings.SelectionSettings.UniqueIds;
                        if (aUniqueIds != null && aUniqueIds.Count() > 0 && aUniqueIds.Contains(aElement.UniqueId))
                        {
                            aElementIdList.Add(aElement.Id);
                            continue;
                        }

                        IEnumerable<int> aElementIds = RevitSettings.SelectionSettings.ElementIds;
                        if(aElementIds != null && aElementIds.Count() > 0 && aElementIds.Contains(aElement.Id.IntegerValue))
                        {
                            aElementIdList.Add(aElement.Id);
                            continue;
                        }
                    }
                        
                }
                if (aElementIdList == null || aElementIdList.Count < 1)
                    continue;

                Read(aElementIdList, aTuple.Item3, objects);
            }
        }

        /***************************************************/

        /// <summary>
        /// Generates BHoMObjects from Revit Elements by given types and BHoM Ids??
        /// </summary>
        /// <param name="type">Object type</param>
        /// <param name="ids">BHoM id collection</param>
        /// <returns name="BHoMObjects">BHoMObject collection</returns>
        /// <search>
        /// RevitAdapter, Read, Revit, BHoMObject, BHoM, BHoMObjects, BuiltInCategories
        /// </search>
        protected override IEnumerable<IBHoMObject> Read(Type type, IList ids)
        {
            if (Document == null)
            {
                Engine.Reflection.Compute.RecordError("BHoM objects could not be read because Revit Document is null.");
                return null;
            }

            if (type == null)
            {
                Engine.Reflection.Compute.RecordError("BHoM objects could not be read because provided type is null.");
                return null;
            }

            List<IBHoMObject> aObjects = new List<IBHoMObject>();
            if (ids == null)
               Read(new Type[] { type }, aObjects);
            else
               Read(new Type[] { type }, aObjects, ids.Cast<string>().ToList());

            return aObjects;
        }

        /***************************************************/
    }
}
