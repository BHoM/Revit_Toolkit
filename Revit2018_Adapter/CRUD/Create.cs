using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Environmental.Elements;
using BH.oM.Environmental.Properties;
using BH.oM.Structural.Elements;
using BH.Engine.Revit;
using BH.Engine.Environment;

using Autodesk.Revit.DB;

namespace BH.Adapter.Revit
{
    public partial class RevitAdapter
    {
        public Building ReadBuilidng(Document Document, bool GenerateSpaces, bool Spaces3D)
        {
            Building aBuilding = Document.FromRevit();

            List<BuildingElementProperties> aBuildingElementPropertiesList = new FilteredElementCollector(Document).OfClass(typeof(WallType)).ToList().ConvertAll(x => ((WallType)x).FromRevit());

            List<Level> aLevelList = new FilteredElementCollector(Document).OfClass(typeof(Level)).Cast<Level>().ToList();
            List<Storey> aStoreyList = new List<Storey>();
            if (aLevelList != null && aLevelList.Count > 0)
                aStoreyList = aLevelList.ConvertAll(x => x.FromRevit());

            List<SpatialElement> aSpatialElementList = new FilteredElementCollector(Document).OfCategory(BuiltInCategory.OST_MEPSpaces).Cast<SpatialElement>().ToList();
            aSpatialElementList.RemoveAll(x => x.Area < 0.001);

            aBuilding.Add(aStoreyList);

            if (GenerateSpaces)
            {
                SpatialElementBoundaryOptions aSpatialElementBoundaryOptions = new SpatialElementBoundaryOptions();
                aSpatialElementBoundaryOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
                aSpatialElementBoundaryOptions.StoreFreeBoundaryFaces = true;

                if (Spaces3D)
                {
                    SpatialElementGeometryCalculator aSpatialElementGeometryCalculator = new SpatialElementGeometryCalculator(Document, aSpatialElementBoundaryOptions);
                    foreach (SpatialElement aSpatialElement in aSpatialElementList)
                    {
                        Space aSpace = aSpatialElement.FromRevit(aSpatialElementGeometryCalculator, aBuildingElementPropertiesList, aStoreyList);
                        if (aSpace != null)
                            aBuilding.Add(aSpace);
                    }
                }
                else
                {
                    foreach (SpatialElement aSpatialElement in aSpatialElementList)
                    {
                        Space aSpace = aSpatialElement.FromRevit(aSpatialElementBoundaryOptions, aBuildingElementPropertiesList, aStoreyList);
                        if (aSpace != null)
                            aBuilding.Add(aSpace);
                    }
                }
            }

            return aBuilding;
        }
    }
}
