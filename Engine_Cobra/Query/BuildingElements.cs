using Autodesk.Revit.DB;
using BH.oM.Environment.Elements;
using System.Collections.Generic;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static List<BuildingElement> BuildingElements(this Building building, ElementId elementId)
        {
            if (elementId == null || elementId == Autodesk.Revit.DB.ElementId.InvalidElementId)
                return null;

            return BuildingElements(building, elementId.IntegerValue);
        }

        /***************************************************/

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