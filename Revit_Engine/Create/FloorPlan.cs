using BH.oM.Adapters.Revit.Elements;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static FloorPlan FloorPlan(string name, string levelName)
        {
            FloorPlan aFloorPlan = new FloorPlan()
            {
                Name = name,
                LevelName = levelName
            };

            aFloorPlan.CustomData.Add("View Name", name);

            aFloorPlan.CustomData.Add(Convert.CategoryName, "Views");
            aFloorPlan.CustomData.Add(Convert.FamilyName, "Floor Plan");

            return aFloorPlan;
        }

        /***************************************************/
    }
}
