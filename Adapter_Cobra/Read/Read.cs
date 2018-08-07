using System;
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
        protected void Read(BuiltInCategory builtInCategory, Discipline discipline, Dictionary<ElementId, List<IBHoMObject>> objects)
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
        protected void Read(IEnumerable<BuiltInCategory> builtInCategories, Discipline discipline, Dictionary<ElementId, List<IBHoMObject>> objects)
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
        protected void Read(ElementId elementId, Discipline discipline, Dictionary<ElementId, List<IBHoMObject>> objects)
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

            List<Type> aTypeList = Query.BHoMTypes(aElement);
            if (aTypeList == null || aTypeList.Count < 1)
            {
                Engine.Reflection.Compute.RecordError(string.Format("BHoM object could not be read because equivalent BHoM type does not exist. Element Id: {0}, Element Name: {1}", elementId.IntegerValue, aElement.Name));
                return;
            }  

            List<IBHoMObject> aResult = null;
            if (aElement is Floor)
            {
                aResult = Engine.Revit.Convert.ToBHoM(aElement as Floor, discipline, true, true);
            }
            else if (aElement is RoofBase)
            {
                aResult = Engine.Revit.Convert.ToBHoM(aElement as RoofBase, discipline, true, true);
            }
            else if (aElement is Wall)
            {
                aResult = Engine.Revit.Convert.ToBHoM(aElement as Wall, discipline, true, true);
            }
            else if (aElement is SpatialElement)
            {
                aResult = new List<IBHoMObject>();
                aResult.Add(Engine.Revit.Convert.ToBHoM(aElement as SpatialElement, objects, discipline, true, true));
            }
            else
            {
                try
                {
                    object aObject = Engine.Revit.Convert.ToBHoM(aElement as dynamic, discipline, true, true);

                    aResult = new List<IBHoMObject>();
                    if (aObject is BHoMObject)
                        aResult.Add(aObject as BHoMObject);
                    else if (aObject is List<IBHoMObject>)
                        aResult.AddRange(aObject as List<IBHoMObject>);
                }
                catch (Exception aException_1)
                {
                    try
                    {
                        IBHoMObject aIBHoMObject = new BHoMObject();
                        aIBHoMObject = Modify.SetIdentifiers(aIBHoMObject, aElement);
                        aIBHoMObject = Modify.SetCustomData(aIBHoMObject, aElement, true);
                        aResult.Add(aIBHoMObject as BHoMObject);
                        Engine.Reflection.Compute.RecordError(string.Format("BHoM object could not be properly converted becasue of missing ToBHoM method. Element Id: {0}, Element Name: {1}, Exception Message: {2}", elementId.IntegerValue, aElement.Name, aException_1.Message));
                    }
                    catch(Exception aException_2)
                    {
                        Engine.Reflection.Compute.RecordError(string.Format("BHoM object could not be read because of conversion exception. Element Id: {0}, Element Name: {1}, Exception Message: {2}", elementId.IntegerValue, aElement.Name, aException_1.Message));
                    }                    
                }
               
            }

            if(aResult != null && aResult.Count > 0)
            {
                if (objects == null)
                    objects = new Dictionary<ElementId, List<IBHoMObject>>();

                objects.Add(aElement.Id, aResult);
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
        protected void Read(IEnumerable<ElementId> elementIds, Discipline discipline, Dictionary<ElementId, List<IBHoMObject>> objects)
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

            List<IBHoMObject> aResult = new List<IBHoMObject>();
            foreach (ElementId aElementId in elementIds)
            {
                if(aElementId == null || aElementId == ElementId.InvalidElementId)
                {
                    Engine.Reflection.Compute.RecordError("BHoM object could not be read because Revit elementId is null or Invalid.");
                    continue;
                }

                try
                {
                    Read(aElementId, discipline, objects);
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
        protected void Read(IEnumerable<Type> types, Dictionary<ElementId, List<IBHoMObject>> objects, IEnumerable<string> uniqueIds = null)
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
                    objects.Add(ElementId.InvalidElementId, new List<IBHoMObject>(new IBHoMObject[] { Document.ToBHoM(aTuple.Item3, true) }));
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

            Dictionary<ElementId, List<IBHoMObject>> aDictionary = new Dictionary<ElementId, List<IBHoMObject>>();

            if (ids == null)
               Read(new Type[] { type }, aDictionary);
            else
               Read(new Type[] { type }, aDictionary, ids.Cast<string>().ToList());

            List<IBHoMObject> aObjects = new List<IBHoMObject>();
            foreach (KeyValuePair<ElementId, List<IBHoMObject>> aKeyValuePair in aDictionary)
            {
                if (aKeyValuePair.Value != null && aKeyValuePair.Value.Count > 0)
                    foreach (BHoMObject aBHoMObject in aKeyValuePair.Value)
                        if (aBHoMObject != null && aBHoMObject.GetType() == type)
                            aObjects.Add(aBHoMObject);
            } 

            return aObjects;
        }

        /***************************************************/
    }
}
