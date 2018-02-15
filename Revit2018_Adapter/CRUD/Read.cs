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

        public Building ReadBuilidng(bool GenerateSpaces, bool Spaces3D)
        {
            Building aBuilding = m_Document.FromRevit();

            //Adding BuilidngElementProperties
            aBuilding.BuildingElementProperties = new FilteredElementCollector(m_Document).OfClass(typeof(WallType)).ToList().ConvertAll(x => ((WallType)x).FromRevit());

            //Adding Storeys
            List<Level> aLevelList = new FilteredElementCollector(m_Document).OfClass(typeof(Level)).Cast<Level>().ToList();
            List<Storey> aStoreyList = new List<Storey>();
            if (aLevelList != null && aLevelList.Count > 0)
                aStoreyList = aLevelList.ConvertAll(x => x.FromRevit());
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
                        Space aSpace = aSpatialElement.FromRevit(aSpatialElementGeometryCalculator, aBuilding.BuildingElementProperties, aStoreyList);
                        if (aSpace != null)
                            aBuilding.Add(aSpace);
                    }
                }
                else
                {
                    //2D geometry
                    foreach (SpatialElement aSpatialElement in aSpatialElementList)
                    {
                        Space aSpace = aSpatialElement.FromRevit(aSpatialElementBoundaryOptions, aBuilding.BuildingElementProperties, aStoreyList);
                        if (aSpace != null)
                            aBuilding.Add(aSpace);
                    }
                }
            }

            return aBuilding;
        }

        public BuildingElement ReadBuildingElement(ElementId ElementId)
        {
            Element aElement = m_Document.GetElement(ElementId);

            if (aElement is Wall)
                return (aElement as Wall).FromRevit();

            return null;
        }

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

        public IEnumerable<BHoMObject> Read(BuiltInCategory BuiltInCategory, bool RemoveNulls = true)
        {
            return Read(new BuiltInCategory[] { BuiltInCategory }, RemoveNulls);
        }

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

        protected override IEnumerable<BHoMObject> Read(Type type, IList ids) //Chnage to IObject
        {
            throw new NotImplementedException();
        }
    }
}
