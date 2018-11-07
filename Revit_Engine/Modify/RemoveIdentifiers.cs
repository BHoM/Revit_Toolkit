using System.ComponentModel;

using BH.oM.Base;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Removes Revit Identifiers from BHoM object.")]
        [Input("bHoMObject", "BHoMObject")]
        [Output("IBHoMObject")]
        public static IBHoMObject RemoveIdentifiers(this IBHoMObject bHoMObject)
        {
            if (bHoMObject == null)
                return null;

            IBHoMObject aBHoMObject = bHoMObject.GetShallowClone();

            aBHoMObject.CustomData.Remove(Convert.AdapterId);
            aBHoMObject.CustomData.Remove(Convert.ElementId);

            return aBHoMObject;
        }

        /***************************************************/
    }
}