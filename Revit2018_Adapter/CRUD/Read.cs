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
        /// <param name="GenerateSpaces">Generate BHoM Spaces from Revit Spaces</param>
        /// <param name="Spaces3D">if sets to true then Spaces wil be transylated to BuildingElementPlanes (3D) otherwise Spaces transylated to BuildingElementCurves (2D)</param>
        /// <returns name="Building">BHoM Building</returns>
        /// <search>
        /// RevitAdapter, ReadBuilidng, Read Builidng, Revit 
        /// </search>
        public Building ReadBuilidng(bool GenerateSpaces, bool Spaces3D)
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
            if (GenerateSpaces)
            {
                SpatialElementBoundaryOptions aSpatialElementBoundaryOptions = new SpatialElementBoundaryOptions();
                aSpatialElementBoundaryOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
                aSpatialElementBoundaryOptions.StoreFreeBoundaryFaces = true;

                if (Spaces3D)
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
        /// <param name="ElementId">ElementId of Revit Element to be read</param>
        /// <returns name="BuildingElement">BHoM BuildingElement</returns>
        /// <search>
        /// RevitAdapter, ReadBuildingElement, Read BuildingElement, Revit, ElementId, BHoMObject, BHoM
        /// </search>
        public BuildingElement ReadBuildingElement(ElementId ElementId)
        {
            Element aElement = m_Document.GetElement(ElementId);

            if (aElement is Wall)
                return (aElement as Wall).ToBHoM();

            return null;
        }

        /// <summary>
        /// Generates BHoMObject from Revit Element by given ElementId
        /// </summary>
        /// <param name="ElementId">ElementId of Revit Element to be read</param>
        /// <returns name="BHoMObject">BHoMObject</returns>
        /// <search>
        /// RevitAdapter, Read, Revit, ElementId, BHoMObject, BHoM
        /// </search>
        public BHoMObject Read(ElementId ElementId)
        {
            if (m_Document == null || ElementId == null || ElementId == ElementId.InvalidElementId)
                return null;

            Element aElement = m_Document.GetElement(ElementId);

            if (aElement == null)
                return null; 

            Type aType = Utilis.BHoM.GetType(aElement);
            if (aType == null)
                return null;

            return (aElement as dynamic).FromRevit();
        }

        /// <summary>
        /// Generates BHoMObjects from Revit Elements by given ElementIds
        /// </summary>
        /// <param name="ElementIds">ElementIds of Revit Elements to be read</param>
        /// <param name="RemoveNulls">Removes nulls from result list</param>
        /// <returns name="BHoMObjects">BHoMObject collection</returns>
        /// <search>
        /// RevitAdapter, Read, Revit, ElementId, BHoMObject, BHoM, BHoMObjects
        /// </search>
        public IEnumerable<BHoMObject> Read(IEnumerable<ElementId> ElementIds, bool RemoveNulls = true)
        {
            if (m_Document == null || ElementIds == null)
                return null;

            List<BHoMObject> aResult = new List<BHoMObject>();
            foreach (ElementId aElementId in ElementIds)
            {
                BHoMObject aBHoMObject = Read(aElementId);
                if (aBHoMObject == null && RemoveNulls)
                    continue;

                aResult.Add(aBHoMObject);
            }

            return aResult;
        }

        /// <summary>
        /// Generates BHoMObjects from Revit Elements by given BuiltInCategory
        /// </summary>
        /// <param name="BuiltInCategory">Revit BuiltInCategory</param>
        /// <param name="RemoveNulls">Removes nulls from result list</param>
        /// <returns name="BHoMObjects">BHoMObject collection</returns>
        /// <search>
        /// RevitAdapter, Read, Revit, BuiltInCategory, BHoMObject, BHoM, BHoMObjects
        /// </search>
        public IEnumerable<BHoMObject> Read(BuiltInCategory BuiltInCategory, bool RemoveNulls = true)
        {
            return Read(new BuiltInCategory[] { BuiltInCategory }, RemoveNulls);
        }

        /// <summary>
        /// Generates BHoMObjects from Revit Elements by given BuiltInCategory
        /// </summary>
        /// <param name="BuiltInCategories">Revit BuiltInCategories collection</param>
        /// <param name="RemoveNulls">Removes nulls from result list</param>
        /// <returns name="BHoMObjects">BHoMObject collection</returns>
        /// <search>
        /// RevitAdapter, Read, Revit, BuiltInCategory, BHoMObject, BHoM, BHoMObjects, BuiltInCategories
        /// </search>
        public IEnumerable<BHoMObject> Read(IEnumerable<BuiltInCategory> BuiltInCategories, bool RemoveNulls = true)
        {
            if (m_Document == null || BuiltInCategories == null || BuiltInCategories.Count() < 1)
                return null;

            ICollection<ElementId> aElementIdList = new FilteredElementCollector(m_Document).WherePasses(new LogicalOrFilter(BuiltInCategories.ToList().ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter))).ToElementIds();

            return Read(aElementIdList, RemoveNulls);
        }

        /***************************************************/
        /**** Protected Methods                         ****/
        /***************************************************/

        /// <summary>
        /// Generates BHoMObjects from Revit Elements by given types and BHoM Ids??
        /// </summary>
        /// <param name="type">BHoM type??</param>
        /// <param name="ids">BHoM id collection??</param>
        /// <returns name="BHoMObjects">BHoMObject collection</returns>
        /// <search>
        /// RevitAdapter, Read, Revit, BHoMObject, BHoM, BHoMObjects, BuiltInCategories
        /// </search>
        protected override IEnumerable<BHoMObject> Read(Type type, IList ids)
        //TODO: Change output to IEnumerable<IObject>
        {
            //TODO: Implement Method
            throw new NotImplementedException();
            
        }
    }
}
