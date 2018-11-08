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

        [Description("Returns true if given FloorPlan should be considered as template.")]
        [Input("viewPlan", "ViewPlan")]
        [Output("IsTemplate")]
        public static bool IsTemplate(this ViewPlan floorPlan)
        {
            if (floorPlan == null)
                return false;

            return floorPlan.IsTemplate;
        }

        /***************************************************/
    }
}
