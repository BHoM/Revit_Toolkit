using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        static public ElementId SpatialElementId(this EnergyAnalysisSpace energyAnalysisSpace)
        {
            if (energyAnalysisSpace == null)
                return null;

            if (energyAnalysisSpace != null)
            {
                Element aElement = energyAnalysisSpace.Document.GetElement(energyAnalysisSpace.CADObjectUniqueId);
                if (aElement != null)
                    return aElement.Id;
            }

            return null;
        }

        /***************************************************/
    }
}

