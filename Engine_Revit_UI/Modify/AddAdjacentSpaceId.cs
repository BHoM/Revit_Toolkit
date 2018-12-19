using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

using BH.oM.Environment.Elements;

namespace BH.UI.Revit.Engine
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static BuildingElement AddAdjacentSpaceId(this BuildingElement buildingElement, EnergyAnalysisSurface energyAnalysisSurface)
        {
            if (buildingElement == null)
                return null;

            BuildingElement aBuildingElement = buildingElement.GetShallowClone() as BuildingElement;
            aBuildingElement.CustomData.Add(BH.Engine.Adapters.Revit.Convert.AdjacentSpaceId, -1);

            if (energyAnalysisSurface == null)
                return aBuildingElement;

            EnergyAnalysisSpace aEnergyAnalysisSpace = energyAnalysisSurface.GetAdjacentAnalyticalSpace();
            if (aEnergyAnalysisSpace == null)
                return aBuildingElement;

            SpatialElement aSpatialElement = Query.Element(aEnergyAnalysisSpace.Document, aEnergyAnalysisSpace.CADObjectUniqueId) as SpatialElement;
            if (aSpatialElement == null)
                return aBuildingElement;

            aBuildingElement.CustomData[BH.Engine.Adapters.Revit.Convert.AdjacentSpaceId] = aSpatialElement.Id.IntegerValue;

            return aBuildingElement;
        }

        /***************************************************/
    }
}