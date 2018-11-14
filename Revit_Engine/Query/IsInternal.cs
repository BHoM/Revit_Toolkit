using System.ComponentModel;

using BH.oM.Base;
using BH.oM.Reflection.Attributes;
using BH.oM.Environment.Elements;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Cheks whatever Building Element is internal element. Works only for Building Elements pulled from analytical model and adjacency have been assigned.")]
        [Input("buildingElement", "BuildingElement pulled from Revit analytical model")]
        [Output("IsInternal")]
        public static bool IsInternal(this BuildingElement buildingElement)
        {
            if (buildingElement == null)
                return false;

            if (buildingElement.CustomData == null)
                return false;

            if (!buildingElement.CustomData.ContainsKey(Convert.SpaceId))
                return false;

            if (!buildingElement.CustomData.ContainsKey(Convert.AdjacentSpaceId))
                return false;

            int aSpaceId = buildingElement.SpaceId();
            int aAdjacentSpaceId = buildingElement.AdjacentSpaceId();

            return aSpaceId != -1 && aAdjacentSpaceId != -1;
        }

        /***************************************************/
    }
}