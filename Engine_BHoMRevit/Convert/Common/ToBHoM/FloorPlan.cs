using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static oM.Adapters.Revit.Elements.ViewPlan ToBHoMViewPlan(this Autodesk.Revit.DB.ViewPlan viewPlan, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            oM.Adapters.Revit.Elements.ViewPlan aViewPlan = pullSettings.FindRefObject<oM.Adapters.Revit.Elements.ViewPlan>(viewPlan.Id.IntegerValue);
            if (aViewPlan != null)
                return aViewPlan;

            if(!viewPlan.IsTemplate && viewPlan.GenLevel != null)
                aViewPlan = BH.Engine.Adapters.Revit.Create.ViewPlan(viewPlan.Name, viewPlan.GenLevel.Name);
            else
                aViewPlan = BH.Engine.Adapters.Revit.Create.ViewPlan(viewPlan.Name);

            aViewPlan.Name = viewPlan.Name;
            aViewPlan = Modify.SetIdentifiers(aViewPlan, viewPlan) as oM.Adapters.Revit.Elements.ViewPlan;
            if (pullSettings.CopyCustomData)
                aViewPlan = Modify.SetCustomData(aViewPlan, viewPlan, true) as oM.Adapters.Revit.Elements.ViewPlan;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aViewPlan);

            return aViewPlan;
        }

        /***************************************************/
    }
}
