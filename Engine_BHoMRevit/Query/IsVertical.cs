using Autodesk.Revit.DB;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        static public bool IsVertical(XYZ xyz)
        {
            return BH.Engine.Adapters.Revit.Query.IsZero(xyz.X) && BH.Engine.Adapters.Revit.Query.IsZero(xyz.Y);
        }

        /***************************************************/
    }
}