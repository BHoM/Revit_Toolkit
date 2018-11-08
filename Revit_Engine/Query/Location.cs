using System.ComponentModel;

using BH.oM.Geometry;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Returns Location (Point or Curve) of given GenericObject.")]
        [Input("genericObject", "GenericObject")]
        [Output("Location")]
        public static IGeometry Location(this GenericObject genericObject)
        {
            if (genericObject == null)
                return null;

            return genericObject.Location;
        }

        /***************************************************/
    }
}

