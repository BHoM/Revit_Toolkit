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
        /// <param name="pullSettings">BHoM PullSettings</param>
        /// <param name="objects">BHoM Objects</param>
        /// <search>
        /// RevitAdapter, Read, Revit, BuiltInCategory, BHoMObject, BHoM, BHoMObjects
        /// </search>
        protected void Read(BuiltInCategory builtInCategory, List<IBHoMObject> objects, PullSettings pullSettings = null)
        {
            Read(new BuiltInCategory[] { builtInCategory }, objects, pullSettings);
        }

        /***************************************************/

        /// <summary>
        /// Generates BHoMObjects from Revit Elements by given BuiltInCategory
        /// </summary>
        /// <param name="builtInCategories">Revit BuiltInCategories collection</param>
        /// <param name="pullSettings">BHoM PullSettings</param>
        /// <param name="objects">BHoM Objects</param>
        /// <search>
        /// RevitAdapter, Read, Revit, BuiltInCategory, BHoMObject, BHoM, BHoMObjects, BuiltInCategories
        /// </search>
        protected void Read(IEnumerable<BuiltInCategory> builtInCategories, List<IBHoMObject> objects, PullSettings pullSettings = null)
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

            if (pullSettings == null)
                pullSettings = PullSettings.Default;

            ICollection<ElementId> aElementIdList = new FilteredElementCollector(Document).WherePasses(new LogicalOrFilter(builtInCategories.ToList().ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter))).ToElementIds();

            Read(aElementIdList, objects, pullSettings);
        }

        /***************************************************/

        /// <summary>
        /// Generates BHoMObjects from Revit Element by given ElementId
        /// </summary>
        /// <param name="elementId">ElementId of Revit Element to be read</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <param name="objects">BHoM Objects</param>
        /// <param name="pullSettings">BHoM PullSettings</param>
        /// <search>
        /// RevitAdapter, Read, Revit, ElementId, BHoMObjects, BHoM
        /// </search>
        protected void Read(ElementId elementId, List<IBHoMObject> objects, PullSettings pullSettings = null)
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

            if (pullSettings == null)
                pullSettings = PullSettings.Default;

            List<IBHoMObject> aResult = null;
            if (aElement is Floor)
            {
                aResult = Engine.Revit.Convert.ToBHoM(aElement as Floor, pullSettings);
            }
            else if (aElement is RoofBase)
            {
                aResult = Engine.Revit.Convert.ToBHoM(aElement as RoofBase, pullSettings);
            }
            else if (aElement is Wall)
            {
                aResult = Engine.Revit.Convert.ToBHoM(aElement as Wall, pullSettings);
            }
            else if (aElement is FamilyInstance)
            {
                aResult = new List<IBHoMObject> { Engine.Revit.Convert.ToBHoM(aElement as FamilyInstance, pullSettings) };
            }
            else if (aElement is SpatialElement)
            {
                aResult = new List<IBHoMObject>();
                aResult.Add(Engine.Revit.Convert.ToBHoM(aElement as SpatialElement, pullSettings));
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
                        aObject = Engine.Revit.Convert.ToBHoM(aElement as dynamic, pullSettings);
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
        /// <param name="pullSettings">BHoM PullSettings</param>
        /// <param name="objects">BHoM Objects</param>
        /// <returns name="BHoMObjects">BHoMObject collection</returns>
        /// <search>
        /// RevitAdapter, Read, Revit, ElementId, BHoMObject, BHoM, BHoMObjects
        /// </search>
        protected void Read(IEnumerable<ElementId> elementIds, List<IBHoMObject> objects, PullSettings pullSettings = null)
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

            if (pullSettings == null)
                pullSettings = PullSettings.Default;

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
                    Read(aElementId, objects, pullSettings);
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

            Dictionary<Discipline, PullSettings> aDictionary_Discipline = new Dictionary<Discipline, PullSettings>();

            //Get Revit class types
            List<Tuple<Type, List<BuiltInCategory>, PullSettings>> aTupleList = new List<Tuple<Type, List<BuiltInCategory>, PullSettings>>();
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
                    {
                        PullSettings aPullSettings = null;
                        if(!aDictionary_Discipline.TryGetValue(Discipline.Environmental, out aPullSettings))
                        {
                            aPullSettings = new PullSettings();
                            aPullSettings.ConvertUnits = true;
                            aPullSettings.CopyCustomData = true;
                            aPullSettings.RefObjects = new Dictionary<int, List<IBHoMObject>>();
                            aPullSettings.Discipline = Discipline.Environmental;

                            aDictionary_Discipline.Add(aPullSettings.Discipline, aPullSettings);
                        }

                        aTupleList.Add(new Tuple<Type, List<BuiltInCategory>, PullSettings>(aType, new List<BuiltInCategory>(), aPullSettings));
                    }
                        
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
                        {
                            PullSettings aPullSettings = null;
                            Discipline aDiscipline = aType.Discipline();
                            if (!aDictionary_Discipline.TryGetValue(aDiscipline, out aPullSettings))
                            {
                                aPullSettings = new PullSettings();
                                aPullSettings.ConvertUnits = true;
                                aPullSettings.CopyCustomData = true;
                                aPullSettings.RefObjects = new Dictionary<int, List<IBHoMObject>>();
                                aPullSettings.Discipline = aDiscipline;

                                aDictionary_Discipline.Add(aPullSettings.Discipline, aPullSettings);
                            }

                            aTupleList.Add(new Tuple<Type, List<BuiltInCategory>, PullSettings>(aType_Temp, aType.BuiltInCategories(), aPullSettings));
                        }
                            

                }
                else
                {
                    Engine.Reflection.Compute.RecordError(string.Format("Provided type is invalid. Type Name: {}", aType.FullName));
                    continue;
                }
            }

            if (aTupleList == null || aTupleList.Count < 1)
                return;

            foreach (Tuple<Type, List<BuiltInCategory>, PullSettings> aTuple in aTupleList)
            {
                if(aTuple.Item1 == typeof(Document))
                {
                    if (Query.AllowElement(RevitSettings, UIDocument, Document.ProjectInformation))
                        objects.Add(Document.ToBHoM(aTuple.Item3));  
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

                    if (uniqueIds != null && uniqueIds.Count() > 0 && !uniqueIds.Contains(aElement.UniqueId))
                        continue;

                    if (Query.AllowElement(RevitSettings, UIDocument, aElement))
                        aElementIdList.Add(aElement.Id);
                        
                }
                if (aElementIdList == null || aElementIdList.Count < 1)
                    continue;

                Read(aElementIdList, objects, aTuple.Item3);
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
