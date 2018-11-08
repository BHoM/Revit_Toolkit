using System.ComponentModel;

using BH.oM.Base;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Gets value of CustomData for given name.")]
        [Input("bHoMObject", "BHoMObject")]
        [Input("name", "CustomData Name")]
        [Output("Value")]
        public static object CustomDataValue(this IBHoMObject bHoMObject, string name)
        {
            if (bHoMObject == null)
                return null;

            object aObject;
            if (bHoMObject.CustomData.TryGetValue(name, out aObject))
                return aObject;

            return null;
        }

        /***************************************************/
    }
}