using BH.oM.Adapters.Revit.Elements;
using BH.oM.Geometry;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static DraftingObject DraftingObject(string familyName, string typeName, Point location, string viewName)
        {
            DraftingObject aDraftingObject = new DraftingObject()
            {
                Name = typeName,
                ViewName = viewName,
                Location = location
            };

            aDraftingObject.CustomData.Add(Convert.FamilyName, familyName);
            aDraftingObject.CustomData.Add(Convert.TypeName, typeName);

            return aDraftingObject;
        }

        /***************************************************/
    }
}
