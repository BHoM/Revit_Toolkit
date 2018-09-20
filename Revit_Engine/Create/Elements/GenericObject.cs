using BH.oM.Adapters.Revit.Elements;
using BH.oM.Geometry;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static GenericObject GenericObject(Point point, string familyName, string typeName)
        {
            GenericObject aGenericObject = new GenericObject()
            {
                Name = typeName,
                Location = point
            };

            aGenericObject.CustomData.Add(Convert.FamilyName, familyName);
            aGenericObject.CustomData.Add(Convert.TypeName, typeName);

            return aGenericObject;
        }

        /***************************************************/

        public static GenericObject GenericObject(ICurve curve, string familyName, string typeName)
        {
            GenericObject aGenericObject = new GenericObject()
            {
                Name = typeName,
                Location = curve
            };

            aGenericObject.CustomData.Add(Convert.FamilyName, familyName);
            aGenericObject.CustomData.Add(Convert.TypeName, typeName);

            return aGenericObject;
        }

        /***************************************************/
    }
}

