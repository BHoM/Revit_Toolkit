using Autodesk.Revit.DB;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        static public bool IsVertical(XYZ xyz)
        {
            return Query.IsZero(xyz.X) && Query.IsZero(xyz.Y);
        }

        /***************************************************/
    }
}