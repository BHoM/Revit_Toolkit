using System.ComponentModel;

using BH.oM.Adapters.Revit.Elements;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Creates ViewPlan object by given name and Level Name.")]
        [Input("name", "View plan Name")]
        [Input("levelName", "Level Name")]
        [Output("ViewPlan")]
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

        [Description("Creates ViewPlan object by given name.")]
        [Input("name", "View plan Name")]
        [Output("ViewPlan")]
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
