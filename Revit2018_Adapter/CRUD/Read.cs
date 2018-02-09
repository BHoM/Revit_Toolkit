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
    public partial class RevitAdapter
    {

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

        protected override IEnumerable<BHoMObject> Read(Type type, IList ids) //Chnage to IObject
        {
            throw new NotImplementedException();
        }
    }
}
