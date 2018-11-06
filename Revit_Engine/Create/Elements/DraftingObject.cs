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

        [Description("Creates DraftingObject by given Family Name, Type Name, Location and View Name. Drafting Object defines all view specific 2D elements")]
        [Input("familyName", "Revit Family Name")]
        [Input("familyTypeName", "Revit Family Type Name")]
        [Input("location", "Location of DraftingObject on View")]
        [Input("viewName", "View assigned to DraftingObject")]
        [Output("DraftingObject")]
        public static DraftingObject DraftingObject(string familyName, string familyTypeName, Point location, string viewName)
        {
            DraftingObject aDraftingObject = new DraftingObject()
            {
                Name = familyTypeName,
                ViewName = viewName,
                Location = location
            };

            aDraftingObject.CustomData.Add(Convert.FamilyName, familyName);
            aDraftingObject.CustomData.Add(Convert.FamilyTypeName, familyTypeName);

            return aDraftingObject;
        }

        /***************************************************/
    }
}
