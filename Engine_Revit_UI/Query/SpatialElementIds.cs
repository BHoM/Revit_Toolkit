using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
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
            if (aElementId != null && aElementIdList.Find(x => x.IntegerValue == aElementId.IntegerValue) == null)
                aElementIdList.Add(aElementId);

            return aElementIdList;
        }

        /***************************************************/
    }
}

