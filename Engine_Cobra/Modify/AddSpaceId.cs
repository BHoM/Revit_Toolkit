using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

using BH.oM.Environment.Elements;

namespace BH.UI.Cobra.Engine
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static BuildingElement AddSpaceId(this BuildingElement buildingElement, EnergyAnalysisSurface energyAnalysisSurface)
        {
            if (buildingElement == null)
                return null;

            if (energyAnalysisSurface == null)
                return buildingElement;

            EnergyAnalysisSpace aEnergyAnalysisSpace = energyAnalysisSurface.GetAnalyticalSpace();
            if (aEnergyAnalysisSpace == null)
                return buildingElement;

            SpatialElement aSpatialElement = Query.Element(aEnergyAnalysisSpace.Document, aEnergyAnalysisSpace.CADObjectUniqueId) as SpatialElement;
            if (aSpatialElement == null)
                return buildingElement;

            BuildingElement aBuildingElement = buildingElement.GetShallowClone() as BuildingElement;
            aBuildingElement.CustomData.Add(BH.Engine.Adapters.Revit.Convert.SpaceId, aSpatialElement.Id.IntegerValue);

            return aBuildingElement;
        }

        /***************************************************/
    }
}