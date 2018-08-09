using Autodesk.Revit.DB;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static int IntegerValue(this ElementId elementId)
        {
            if (elementId == null)
                return Autodesk.Revit.DB.ElementId.InvalidElementId.IntegerValue;

            return elementId.IntegerValue;
        }

        /***************************************************/

        public static int IntegerValue(this WorksetId worksetId)
        {
            if (worksetId == null)
                return Autodesk.Revit.DB.WorksetId.InvalidWorksetId.IntegerValue;

            return worksetId.IntegerValue;
        }

        /***************************************************/
    }
}