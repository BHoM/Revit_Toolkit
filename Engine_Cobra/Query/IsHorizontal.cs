using Autodesk.Revit.DB;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        static public bool IsHorizontal(PlanarFace planarFace)
        {
            return IsVertical(planarFace.FaceNormal);
        }

        /***************************************************/
    }
}