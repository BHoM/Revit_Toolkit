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
        /// Gets EnergyAnalysisSpace ElementIds
        /// </summary>
        /// <param name="energyAnalysisSurface">Revit EnergyAnalysisSurface</param>
        /// <returns name="SpatialElementIds">List of Revit ElementIds</returns>
        /// <search>
        /// Query, EnergyAnalysisSpaceIds, EnergyAnalysisSurface
        /// </search>
        static public List<ElementId> SpatialElementIds(this EnergyAnalysisSurface energyAnalysisSurface)
        {
            if (energyAnalysisSurface == null)
                return null;

            List<ElementId> aElementIdList = new List<ElementId>();

            ElementId aElementId = null;

            aElementId = SpatialElementId(energyAnalysisSurface.GetAnalyticalSpace());
            if(aElementId != null)
                aElementIdList.Add(aElementId);

            aElementId = SpatialElementId(energyAnalysisSurface.GetAdjacentAnalyticalSpace());
            if (aElementId != null)
                aElementIdList.Add(aElementId);

            return aElementIdList;
        }

        /***************************************************/
    }
}

