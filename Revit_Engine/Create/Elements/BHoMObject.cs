using System.ComponentModel;

using BH.oM.Base;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Creates BHoMObject by given Revit ElementId (Element.Id). Allows to pull parameters from any element from Revit")]
        [Input("elementId", "Integer value for Revit ElementId")]
        [Output("BHoMObject")]
        public static BHoMObject BHoMObject(int elementId)
        {
            BHoMObject aBHoMObject = new BHoMObject()
            {

            };

            aBHoMObject.CustomData.Add(Convert.ElementId, elementId);

            return aBHoMObject;
        }

        /***************************************************/

        [Description("Creates BHoMObject by given Revit UniqueID (Element.UniqueId). Allows to pull parameters from any element from Revit")]
        [Input("elementId", "Integer value for Revit ElementId")]
        [Output("BHoMObject")]
        public static BHoMObject BHoMObject(string uniqueId)
        {
            BHoMObject aBHoMObject = new BHoMObject()
            {

            };

            aBHoMObject.CustomData.Add(Convert.AdapterId, uniqueId);

            return aBHoMObject;
        }

        /***************************************************/
    }
}

