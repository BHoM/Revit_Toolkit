using Autodesk.Revit.DB;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        static public PlanarFace Top(this Solid solid)
        {
            PlanarFace aResult = null;
            FaceArray aFaceArray = solid.Faces;
            foreach (Face aFace in aFaceArray)
            {
                PlanarFace aPlanarFace = aFace as PlanarFace;
                if (null != aPlanarFace && Query.IsHorizontal(aPlanarFace))
                    if ((null == aResult) || (aResult.Origin.Z < aPlanarFace.Origin.Z))
                        aResult = aPlanarFace;
            }
            return aResult;
        }

        /***************************************************/
    }
}
