using System;
using System.Collections.Generic;

using BH.oM.Environment.Elements;

using Autodesk.Revit.DB;


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
        /// Get BuilidingElements by given Revit ElementId
        /// </summary>
        /// <param name="building">BHoM Building</param>
        /// <param name="elementId">Revit ElementId</param>
        /// <returns name="BuildingElements">BHoM BuildingElements</returns>
        /// <search>
        /// Query, BuildingElements, ElementId
        /// </search>
        public static List<BuildingElement> BuildingElements(this Building building, ElementId elementId)
        {
            if (elementId == null || elementId == Autodesk.Revit.DB.ElementId.InvalidElementId)
                return null;

            return BuildingElements(building, elementId.IntegerValue);
        }

        /// <summary>
        /// Get BuilidingElements by given integer value of Revit ElementId
        /// </summary>
        /// <param name="building">BHoM Building</param>
        /// <param name="elementId">Integer value of Revit ElementId</param>
        /// <returns name="BuildingElements">BHoM BuildingElements</returns>
        /// <search>
        /// Query, BuildingElements, ElementId
        /// </search>
        public static List<BuildingElement> BuildingElements(this Building building, int elementId)
        {
            if (building == null)
                return null;

            List<BuildingElement> aResult = new List<BuildingElement>();
            foreach (BuildingElement aBuildingElement in building.BuildingElements)
            {
                object aValue;
                if (aBuildingElement.CustomData.TryGetValue(Convert.ElementId, out aValue))
                    if (aValue is int && (int)aValue == elementId)
                        aResult.Add(aBuildingElement);
            }
            return aResult;
        }

        /***************************************************/
    }
}

