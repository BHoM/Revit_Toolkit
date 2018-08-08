using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

namespace BH.Engine.Revit
{
    /// <summary>
    /// BHoM Revit Engine Query Methods
    /// </summary>
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        /// <summary>
        /// Gets EnergyAnalysisSpace ElementId
        /// </summary>
        /// <param name="energyAnalysisSpace">Revit EnergyAnalysisSpace</param>
        /// <returns name="SpatialElementId">List of Revit ElementIds</returns>
        /// <search>
        /// Query, EnergyAnalysisSpaceId, mEnergyAnalysisSpace
        /// </search>
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

