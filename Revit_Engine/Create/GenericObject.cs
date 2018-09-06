using BH.oM.Adapters.Revit;
using BH.oM.Geometry;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static GenericObject GenericObject(Point point)
        {
            return new GenericObject()
            {
                Location = point
            };
        }

        /***************************************************/

        public static GenericObject GenericObject(ICurve curve)
        {
            return new GenericObject()
            {
                Location = curve
            };
        }

        /***************************************************/

        public static GenericObject GenericObject(Point point, string familyName, string typeName)
        {
            GenericObject aGenericObject = new GenericObject()
            {
                Location = point
            };

            aGenericObject.CustomData.Add(Convert.FamilyName, familyName);
            aGenericObject.CustomData.Add(Convert.TypeName, typeName);

            return aGenericObject;
        }

        /***************************************************/
    }
}

