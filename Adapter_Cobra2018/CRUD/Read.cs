using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Base;
using BH.oM.Environmental.Elements;
using BH.Engine.Revit;
using BH.oM.Structural.Elements;
using BH.Engine.Environment;

using Autodesk.Revit.DB;
using BH.oM.DataManipulation.Queries;

namespace BH.UI.Revit.Adapter
{
    /// <summary>
    /// BHoM RevitAdapter
    /// </summary>
    public partial class RevitInternalAdapter
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
        protected void Read(BuiltInCategory builtInCategory, Discipline discipline, Dictionary<ElementId, List<BHoMObject>> objects)
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
        protected void Read(IEnumerable<BuiltInCategory> builtInCategories, Discipline discipline, Dictionary<ElementId, List<BHoMObject>> objects)
        {
            if (m_Document == null || builtInCategories == null || builtInCategories.Count() < 1)
                return;

            ICollection<ElementId> aElementIdList = new FilteredElementCollector(m_Document).WherePasses(new LogicalOrFilter(builtInCategories.ToList().ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter))).ToElementIds();

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
        protected void Read(ElementId elementId, Discipline discipline, Dictionary<ElementId, List<BHoMObject>> objects)
        {
            if (m_Document == null || elementId == null || elementId == ElementId.InvalidElementId)
                return;

            Element aElement = m_Document.GetElement(elementId);

            if (aElement == null)
                return;

            List<Type> aTypeList = Engine.Revit.Query.BHoMTypes(aElement);
            if (aTypeList == null)
                return;

            List<BHoMObject> aResult = null;
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
                aResult = new List<BHoMObject>();
                aResult.Add(Engine.Revit.Convert.ToBHoM(aElement as SpatialElement, objects, discipline, true, true));
            }
            else
            {
                object aObject = Engine.Revit.Convert.ToBHoM(aElement as dynamic, discipline, true, true);

                aResult = new List<BHoMObject>();
                if (aObject is BHoMObject)
                    aResult.Add(aObject as BHoMObject);
                else if (aObject is List<BHoMObject>)
                    aResult.AddRange(aObject as List<BHoMObject>);                
            }

            if(aResult != null && aResult.Count > 0)
            {
                if (objects == null)
                    objects = new Dictionary<ElementId, List<BHoMObject>>();

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
        protected void Read(IEnumerable<ElementId> elementIds, Discipline discipline, Dictionary<ElementId, List<BHoMObject>> objects)
        {
            if (m_Document == null || elementIds == null)
                return;

            List<BHoMObject> aResult = new List<BHoMObject>();
            foreach (ElementId aElementId in elementIds)
                Read(aElementId, discipline, objects);
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
        protected void Read(IEnumerable<Type> types, Dictionary<ElementId, List<BHoMObject>> objects, IEnumerable<string> uniqueIds = null)
        {
            if (types == null || m_Document == null)
                return;

            //Get Revit class types
            List<Tuple<Type, Discipline>> aTupleList = new List<Tuple<Type, Discipline>>();
            foreach (Type aType in types)
            {
                if (Engine.Revit.Query.IsAssignableFromByFullName(aType, typeof(Element)))
                {
                    if(aTupleList.Find(x => x.Item1 == aType) == null)
                        aTupleList.Add(new Tuple<Type, Discipline>(aType, Discipline.Environmental));
                }
                else if (Engine.Revit.Query.IsAssignableFromByFullName(aType, typeof(BHoMObject)))
                {
                    IEnumerable<Type> aTypes = Engine.Revit.Query.RevitTypes(aType);
                    if (aTypes != null)
                    {
                        foreach (Type aType_Temp in aTypes)
                            if (aTupleList.Find(x => x.Item1 == aType_Temp) == null)
                                aTupleList.Add(new Tuple<Type, Discipline>(aType_Temp, aType.Discipline()));
                    }

                }
            }

            if (aTupleList == null || aTupleList.Count < 1)
                return;

            foreach (Tuple<Type, Discipline> aTuple in aTupleList)
            {
                if(aTuple.Item1 == typeof(Document))
                {
                    objects.Add(ElementId.InvalidElementId, new List<BHoMObject>(new BHoMObject[] { m_Document.ToBHoM(aTuple.Item2, true) }));
                    continue;
                }

                List<ElementId> aElementIdList = new List<ElementId>();
                foreach (Element aElement in new FilteredElementCollector(m_Document).OfClass(aTuple.Item1))
                {
                    if (aElement == null)
                        continue;

                    if (uniqueIds == null || uniqueIds.Contains(aElement.UniqueId))
                        aElementIdList.Add(aElement.Id);
                }
                if (aElementIdList == null || aElementIdList.Count < 1)
                    continue;

                Read(aElementIdList, aTuple.Item2, objects);
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
            if (type == null)
                return null;

            Dictionary<ElementId, List<BHoMObject>> aDictionary = new Dictionary<ElementId, List<BHoMObject>>();

            if (ids == null)
               Read(new Type[] { type }, aDictionary);
            else
               Read(new Type[] { type }, aDictionary, ids.Cast<string>().ToList());

            List<BHoMObject> aObjects = new List<BHoMObject>();
            foreach (KeyValuePair<ElementId, List<BHoMObject>> aKeyValuePair in aDictionary)
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
