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

namespace BH.Adapter.Revit
{
    /// <summary>
    /// BHoM RevitAdapter
    /// </summary>
    public partial class RevitAdapter
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
        /// <returns name="BHoMObjects">BHoMObject collection</returns>
        /// <search>
        /// RevitAdapter, Read, Revit, BuiltInCategory, BHoMObject, BHoM, BHoMObjects
        /// </search>
        protected IEnumerable<BHoMObject> Read(BuiltInCategory builtInCategory, Discipline discipline)
        {
            return Read(new BuiltInCategory[] { builtInCategory }, discipline);
        }

        /// <summary>
        /// Generates BHoMObjects from Revit Elements by given BuiltInCategory
        /// </summary>
        /// <param name="builtInCategories">Revit BuiltInCategories collection</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <returns name="BHoMObjects">BHoMObject collection</returns>
        /// <search>
        /// RevitAdapter, Read, Revit, BuiltInCategory, BHoMObject, BHoM, BHoMObjects, BuiltInCategories
        /// </search>
        protected IEnumerable<BHoMObject> Read(IEnumerable<BuiltInCategory> builtInCategories, Discipline discipline)
        {
            if (m_Document == null || builtInCategories == null || builtInCategories.Count() < 1)
                return null;

            ICollection<ElementId> aElementIdList = new FilteredElementCollector(m_Document).WherePasses(new LogicalOrFilter(builtInCategories.ToList().ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter))).ToElementIds();

            return Read(aElementIdList, discipline);
        }

        /***************************************************/

        /// <summary>
        /// Generates BHoMObjects from Revit Element by given ElementId
        /// </summary>
        /// <param name="elementId">ElementId of Revit Element to be read</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <returns name="BHoMObjects">BHoMObjects collection</returns>
        /// <search>
        /// RevitAdapter, Read, Revit, ElementId, BHoMObjects, BHoM
        /// </search>
        protected IEnumerable<BHoMObject> Read(ElementId elementId, Discipline discipline)
        {
            if (m_Document == null || elementId == null || elementId == ElementId.InvalidElementId)
                return null;

            Element aElement = m_Document.GetElement(elementId);

            if (aElement == null)
                return null;

            List<Type> aTypeList = Engine.Revit.Query.BHoMTypes(aElement);
            if (aTypeList == null)
                return null;

            if (aElement is Floor)
            {
                return Engine.Revit.Convert.ToBHoM(aElement as Floor, discipline, true);
            }
            else
            {
                List<BHoMObject> aResult = new List<BHoMObject>();
                aResult.Add(Engine.Revit.Convert.ToBHoM(aElement as dynamic, discipline, true));
                return aResult;
            }
        }

        /// <summary>
        /// Generates BHoMObjects from Revit Elements by given ElementIds
        /// </summary>
        /// <param name="elementIds">ElementIds of Revit Elements to be read</param>
        /// <param name="discipline">BHoM Discipline</param>
        /// <returns name="BHoMObjects">BHoMObject collection</returns>
        /// <search>
        /// RevitAdapter, Read, Revit, ElementId, BHoMObject, BHoM, BHoMObjects
        /// </search>
        protected IEnumerable<BHoMObject> Read(IEnumerable<ElementId> elementIds, Discipline discipline)
        {
            if (m_Document == null || elementIds == null)
                return null;

            List<BHoMObject> aResult = new List<BHoMObject>();
            foreach (ElementId aElementId in elementIds)
            {
                IEnumerable<BHoMObject> aBHoMObjects = Read(aElementId, discipline);
                if (aBHoMObjects == null || aBHoMObjects.Count() < 1)
                    continue;

                foreach (BHoMObject aBHoMObject in aBHoMObjects)
                    aResult.Add(aBHoMObject);
            }

            return aResult;
        }

        /// <summary>
        /// Generates BHoMObjects from given class types. Class types have to inherits from Autodesk.Revit.DB.Element or BH.oM.Base.BHoMObject
        /// </summary>
        /// <param name="types">Revit or BHoM class types</param>
        /// <param name="uniqueIds">Revit element UniqueIds to be read. all object from given class type is read if uniqueIds collection equals to null</param>
        /// <returns name="BHoMObjects">BHoMObject collection</returns>
        /// <search>
        /// RevitAdapter, Read, Revit, BHoMObject, BHoM, BHoMObjects, 
        /// </search>
        protected IEnumerable<BHoMObject> Read(IEnumerable<Type> types, IEnumerable<string> uniqueIds = null)
        {
            if (types == null || m_Document == null)
                return null;

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
                return null;

            List<BHoMObject> aBHoMObjectList = new List<BHoMObject>();
            foreach (Tuple<Type, Discipline> aTuple in aTupleList)
            {
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

                aBHoMObjectList.AddRange(Read(aElementIdList, aTuple.Item2));
            }

            return aBHoMObjectList;
        }

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
            if(ids == null)
                return Read(new Type[] { type }, null);
            else
                return Read(new Type[] { type }, ids.Cast<string>().ToList());
        }
    }
}
