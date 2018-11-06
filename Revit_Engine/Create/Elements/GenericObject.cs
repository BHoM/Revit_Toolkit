using System.ComponentModel;

using BH.oM.Adapters.Revit.Elements;
using BH.oM.Geometry;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Creates GenericObject by given Point, Family Name and Family Type Name. GenericObject represents generic 3D elements which have not been defined in BHoM structure")]
        [Input("point", "Location Point of Object in 3D space")]
        [Input("familyName", "Revit Family Name")]
        [Input("familyTypeName", "Revit Family Type Name")]
        [Output("GenericObject")]
        public static GenericObject GenericObject(Point point, string familyName, string familyTypeName)
        {
            GenericObject aGenericObject = new GenericObject()
            {
                Name = familyTypeName,
                Location = point
            };

            aGenericObject.CustomData.Add(Convert.FamilyName, familyName);
            aGenericObject.CustomData.Add(Convert.FamilyTypeName, familyTypeName);

            return aGenericObject;
        }

        /***************************************************/

        [Description("Creates GenericObject by given Point, Family Name and Family Type Name. GenericObject represents generic 3D elements which have not been defined in BHoM structure")]
        [Input("curve", "Location Curve of Object in 3D space")]
        [Input("familyName", "Revit Family Name")]
        [Input("familyTypeName", "Revit Family Type Name")]
        [Output("GenericObject")]
        public static GenericObject GenericObject(ICurve curve, string familyName, string familyTypeName)
        {
            GenericObject aGenericObject = new GenericObject()
            {
                Name = familyTypeName,
                Location = curve
            };

            aGenericObject.CustomData.Add(Convert.FamilyName, familyName);
            aGenericObject.CustomData.Add(Convert.FamilyTypeName, familyTypeName);

            return aGenericObject;
        }

        /***************************************************/
    }
}

