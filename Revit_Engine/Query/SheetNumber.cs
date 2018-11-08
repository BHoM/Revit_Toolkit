using System.ComponentModel;

using BH.oM.Adapters.Revit.Elements;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Returns SheetNumber assigned to given Viewport")]
        [Input("viewport", "Viewport")]
        [Output("SheetNumber")]
        public static string SheetNumber(this Viewport viewport)
        {
            if (viewport == null)
                return null;

            object aValue = null;
            if (viewport.CustomData.TryGetValue("Sheet Number", out aValue))
            {
                if (aValue == null)
                    return null;

                return aValue.ToString();
            }

            return null;
        }

        /***************************************************/
    }
}
