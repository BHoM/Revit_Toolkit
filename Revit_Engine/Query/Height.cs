using System.ComponentModel;

using BH.oM.Geometry;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Reflection.Attributes;
using BH.oM.Environment.Elements;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Returns BuildingElement Height")]
        [Input("buildingElement", "BuildingElement")]
        [Output("Height")]
        public static double Height(this BuildingElement buildingElement)
        {
            if (buildingElement == null)
                return double.NaN;

            BoundingBox aBoundingBox = Geometry.Query.Bounds(buildingElement.PanelCurve as dynamic);

            return aBoundingBox.Max.Z - aBoundingBox.Min.Z;
        }

        /***************************************************/
    }
}

