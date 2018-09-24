using BH.oM.Adapters.Revit.Elements;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static ViewPlan ViewPlan(string name, string levelName)
        {
            ViewPlan aViewPlan = new ViewPlan()
            {
                Name = name,
                LevelName = levelName,
                IsTemplate = false
            };

            aViewPlan.CustomData.Add("View Name", name);

            aViewPlan.CustomData.Add(Convert.CategoryName, "Views");
            aViewPlan.CustomData.Add(Convert.FamilyName, "Floor Plan");

            return aViewPlan;
        }

        /***************************************************/

        public static ViewPlan ViewPlan(string name)
        {
            ViewPlan aViewPlan = new ViewPlan()
            {
                Name = name,
                LevelName = null,
                IsTemplate = true
            };

            aViewPlan.CustomData.Add("View Name", name);

            aViewPlan.CustomData.Add(Convert.CategoryName, "Views");
            aViewPlan.CustomData.Add(Convert.FamilyName, "Floor Plan");

            return aViewPlan;
        }

        /***************************************************/
    }
}
