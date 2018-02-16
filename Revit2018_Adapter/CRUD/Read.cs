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

        /// <summary>
        /// Generates BHoM Building from Revit Document
        /// </summary>
        /// <param name="generateSpaces">Generate BHoM Spaces from Revit Spaces</param>
        /// <param name="spaces3D">if sets to true then Spaces wil be transylated to BuildingElementPlanes (3D) otherwise Spaces transylated to BuildingElementCurves (2D)</param>
        /// <returns name="Building">BHoM Building</returns>
        /// <search>
        /// RevitAdapter, ReadBuilidng, Read Builidng, Revit 
        /// </search>
        public Building ReadBuilidng(bool generateSpaces, bool spaces3D)
        {
            Building aBuilding = m_Document.ToBHoM();

            //Adding BuilidngElementProperties
            aBuilding.BuildingElementProperties = new FilteredElementCollector(m_Document).OfClass(typeof(WallType)).ToList().ConvertAll(x => ((WallType)x).ToBHoM());

            //Adding Storeys
            List<Level> aLevelList = new FilteredElementCollector(m_Document).OfClass(typeof(Level)).Cast<Level>().ToList();
            List<Storey> aStoreyList = new List<Storey>();
            if (aLevelList != null && aLevelList.Count > 0)
                aStoreyList = aLevelList.ConvertAll(x => x.ToBHoM());
            aBuilding.Add(aStoreyList);


            //Adding Spaces
            List<SpatialElement> aSpatialElementList = new FilteredElementCollector(m_Document).OfCategory(BuiltInCategory.OST_MEPSpaces).Cast<SpatialElement>().ToList();
            aSpatialElementList.RemoveAll(x => x.Area < 0.001);


            //Adding Spaces
            if (generateSpaces)
            {
                SpatialElementBoundaryOptions aSpatialElementBoundaryOptions = new SpatialElementBoundaryOptions();
                aSpatialElementBoundaryOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
                aSpatialElementBoundaryOptions.StoreFreeBoundaryFaces = true;

                if (spaces3D)
                {
                    //3D geometry
                    SpatialElementGeometryCalculator aSpatialElementGeometryCalculator = new SpatialElementGeometryCalculator(m_Document, aSpatialElementBoundaryOptions);
                    foreach (SpatialElement aSpatialElement in aSpatialElementList)
                    {
                        Space aSpace = aSpatialElement.ToBHoM(aSpatialElementGeometryCalculator, aBuilding.BuildingElementProperties, aStoreyList);
                        if (aSpace != null)
                            aBuilding.Add(aSpace);
                    }
                }
                else
                {
                    //2D geometry
                    foreach (SpatialElement aSpatialElement in aSpatialElementList)
                    {
                        Space aSpace = aSpatialElement.ToBHoM(aSpatialElementBoundaryOptions, aBuilding.BuildingElementProperties, aStoreyList);
                        if (aSpace != null)
                            aBuilding.Add(aSpace);
                    }
                }
            }

            return aBuilding;
        }

        /// <summary>
        /// Generates BHoM BuildingElement from Revit Element by given ElementId
        /// </summary>
        /// <param name="elementId">ElementId of Revit Element to be read</param>
        /// <returns name="BuildingElement">BHoM BuildingElement</returns>
        /// <search>
        /// RevitAdapter, ReadBuildingElement, Read BuildingElement, Revit, ElementId, BHoMObject, BHoM
        /// </search>
        public IEnumerable<BuildingElement> ReadBuildingElements(ElementId elementId)
        {
            Element aElement = m_Document.GetElement(elementId);

            Type aType = Utilis.BHoM.GetType(aElement);
            if (aType != typeof(BuildingElement))
                return null;

            IEnumerable<BHoMObject> aResult = Read(elementId);
            if (aResult == null)
                return null;

            return aResult.Cast<BuildingElement>();
        }

        /// <summary>
        /// Generates BHoMObject from Revit Element by given ElementId
        /// </summary>
        /// <param name="elementId">ElementId of Revit Element to be read</param>
        /// <returns name="BHoMObject">BHoMObject</returns>
        /// <search>
        /// RevitAdapter, Read, Revit, ElementId, BHoMObject, BHoM
        /// </search>
        public BHoMObject ReadSingle(ElementId elementId)
        {
            if (m_Document == null || elementId == null || elementId == ElementId.InvalidElementId)
                return null;

            Element aElement = m_Document.GetElement(elementId);

            if (aElement == null)
                return null; 

            Type aType = Utilis.BHoM.GetType(aElement);
            if (aType == null)
                return null;

            if (aElement is Floor)
            {
                List<BHoMObject> aResult = Engine.Revit.Convert.ToBHoM(aElement as dynamic, true);
                if (aResult != null && aResult.Count > 0)
                    return aResult.First();
            }
            else
            {
                
                return Engine.Revit.Convert.ToBHoM(aElement as dynamic, true);
            }

            return null;
        }

        /// <summary>
        /// Generates BHoMObjects from Revit Element by given ElementId
        /// </summary>
        /// <param name="elementId">ElementId of Revit Element to be read</param>
        /// <returns name="BHoMObjects">BHoMObjects collection</returns>
        /// <search>
        /// RevitAdapter, Read, Revit, ElementId, BHoMObjects, BHoM
        /// </search>
        public IEnumerable<BHoMObject> Read(ElementId elementId)
        {
            if (m_Document == null || elementId == null || elementId == ElementId.InvalidElementId)
                return null;

            Element aElement = m_Document.GetElement(elementId);

            if (aElement == null)
                return null;

            Type aType = Utilis.BHoM.GetType(aElement);
            if (aType == null)
                return null;

            if(aElement is Floor)
            {
                return Engine.Revit.Convert.ToBHoM(aElement as Floor, true);
            }
            else
            {
                List<BHoMObject> aResult = new List<BHoMObject>();
                aResult.Add(Engine.Revit.Convert.ToBHoM(aElement as dynamic, true));
                return aResult;
            }
        }

        /// <summary>
        /// Generates BHoMObjects from Revit Elements by given ElementIds
        /// </summary>
        /// <param name="elementIds">ElementIds of Revit Elements to be read</param>
        /// <returns name="BHoMObjects">BHoMObject collection</returns>
        /// <search>
        /// RevitAdapter, Read, Revit, ElementId, BHoMObject, BHoM, BHoMObjects
        /// </search>
        public IEnumerable<BHoMObject> Read(IEnumerable<ElementId> elementIds)
        {
            if (m_Document == null || elementIds == null)
                return null;

            List<BHoMObject> aResult = new List<BHoMObject>();
            foreach (ElementId aElementId in elementIds)
            {
                IEnumerable<BHoMObject> aBHoMObjects = Read(aElementId);
                if (aBHoMObjects == null || aBHoMObjects.Count() < 1)
                    continue;

                foreach(BHoMObject aBHoMObject in aBHoMObjects)
                    aResult.Add(aBHoMObject);
            }

            return aResult;
        }

        /// <summary>
        /// Generates BHoMObjects from Revit Elements by given BuiltInCategory
        /// </summary>
        /// <param name="builtInCategory">Revit BuiltInCategory</param>
        /// <returns name="BHoMObjects">BHoMObject collection</returns>
        /// <search>
        /// RevitAdapter, Read, Revit, BuiltInCategory, BHoMObject, BHoM, BHoMObjects
        /// </search>
        public IEnumerable<BHoMObject> Read(BuiltInCategory builtInCategory)
        {
            return Read(new BuiltInCategory[] { builtInCategory });
        }

        /// <summary>
        /// Generates BHoMObjects from Revit Elements by given BuiltInCategory
        /// </summary>
        /// <param name="builtInCategories">Revit BuiltInCategories collection</param>
        /// <returns name="BHoMObjects">BHoMObject collection</returns>
        /// <search>
        /// RevitAdapter, Read, Revit, BuiltInCategory, BHoMObject, BHoM, BHoMObjects, BuiltInCategories
        /// </search>
        public IEnumerable<BHoMObject> Read(IEnumerable<BuiltInCategory> builtInCategories)
        {
            if (m_Document == null || builtInCategories == null || builtInCategories.Count() < 1)
                return null;

            ICollection<ElementId> aElementIdList = new FilteredElementCollector(m_Document).WherePasses(new LogicalOrFilter(builtInCategories.ToList().ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter))).ToElementIds();

            return Read(aElementIdList);
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
        public IEnumerable<BHoMObject> Read(IEnumerable<Type> types, IEnumerable<string> uniqueIds = null)
        {
            if (types == null || m_Document == null)
                return null;

            //Get Revit class types
            List<Type> aTypeList = new List<Type>();
            foreach(Type aType in types)
            {
                if (Utilis.Type.IsAssignableFromByFullName(typeof(Element), aType))
                {
                    if (!aTypeList.Contains(aType))
                        aTypeList.Add(aType);
                }
                else if (Utilis.Type.IsAssignableFromByFullName(typeof(BHoMObject), aType))
                {
                    IEnumerable<Type> aTypes = Utilis.Revit.GetTypes(aType);
                    if (aTypes != null)
                    {
                        foreach (Type aType_Temp in aTypes)
                            if (!aTypeList.Contains(aType_Temp))
                                aTypeList.Add(aType_Temp);
                    }
                       
                }
            }

            if (aTypeList == null || aTypeList.Count < 1)
                return null;

            List<ElementId> aElementIdList = new List<ElementId>();
            foreach (Element aElement in new FilteredElementCollector(m_Document).WherePasses(new LogicalOrFilter(aTypeList.ConvertAll(x => new ElementClassFilter(x) as ElementFilter))))
            {
                if (aElement == null)
                    continue;

                if (uniqueIds == null || uniqueIds.Contains(aElement.UniqueId))
                    aElementIdList.Add(aElement.Id);
            }

            if (aElementIdList == null || aElementIdList.Count < 1)
                return null;

            return Read(aElementIdList);
        }

        /***************************************************/
        /**** Protected Methods                         ****/
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
        protected override IEnumerable<BHoMObject> Read(Type type, IList ids)
        {
            return Read(new Type[] { type }, ids.Cast<string>().ToList());
        }
    }
}
