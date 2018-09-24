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

        internal static oM.Adapters.Revit.Elements.ViewPlan ToBHoMViewPlan(this Autodesk.Revit.DB.ViewPlan viewPlan, PullSettings pullSettings = null)
        {
            oM.Adapters.Revit.Elements.ViewPlan aViewPlan = null;

            if(!viewPlan.IsTemplate && viewPlan.GenLevel != null)
                aViewPlan = BH.Engine.Adapters.Revit.Create.ViewPlan(viewPlan.Name, viewPlan.GenLevel.Name);
            else
                aViewPlan = BH.Engine.Adapters.Revit.Create.ViewPlan(viewPlan.Name);

            aViewPlan.Name = viewPlan.Name;
            aViewPlan = Modify.SetIdentifiers(aViewPlan, viewPlan) as oM.Adapters.Revit.Elements.ViewPlan;
            aViewPlan = Modify.SetCustomData(aViewPlan, viewPlan, true) as oM.Adapters.Revit.Elements.ViewPlan;

            return aViewPlan;
        }

        /***************************************************/
    }
}
