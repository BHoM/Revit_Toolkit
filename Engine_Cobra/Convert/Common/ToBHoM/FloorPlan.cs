using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static FloorPlan ToBHoM(this ViewPlan viewPlan, PullSettings pullSettings = null)
        {
            FloorPlan aFloorPlan = BH.Engine.Adapters.Revit.Create.FloorPlan(viewPlan.Name, viewPlan.GenLevel.Name);

            aFloorPlan.Name = viewPlan.Name;
            aFloorPlan = Modify.SetIdentifiers(aFloorPlan, viewPlan) as FloorPlan;
            aFloorPlan = Modify.SetCustomData(aFloorPlan, viewPlan, true) as FloorPlan;

            return aFloorPlan;
        }

        /***************************************************/
    }
}
