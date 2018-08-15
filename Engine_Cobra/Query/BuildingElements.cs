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

            return BH.Engine.Revit.Query.BuildingElements(building, elementId.IntegerValue);
        }

        /***************************************************/
    }
}