using BH.oM.Adapters.Revit.Elements;
using BH.oM.Base;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static bool IsTemplate(this ViewPlan floorPlan)
        {
            if (floorPlan == null)
                return false;

            return floorPlan.IsTemplate;
        }

        /***************************************************/
    }
}
