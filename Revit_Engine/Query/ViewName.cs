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

        [Description("Returns ViewName assigned to Viewport.")]
        [Input("viewport", "Viewport")]
        [Output("ViewName")]
        public static string ViewName(this Viewport viewport)
        {
            if (viewport == null)
                return null;

            object aValue = null;
            if (viewport.CustomData.TryGetValue("View Name", out aValue))
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
