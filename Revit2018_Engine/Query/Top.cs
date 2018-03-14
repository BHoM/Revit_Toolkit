using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace BH.Engine.Revit
{
    /// <summary>
    /// BHoM Revit Engine Query Methods
    /// </summary>
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        /// <summary>
        /// Gets Top Planar Face of Revit Solid
        /// </summary>
        /// <param name="solid">Revit Solid</param>
        /// <returns name="PlanarFace">Revit PlanarFace</returns>
        /// <search>
        /// Query, Planar Face, Solid, Top, planarface
        /// </search>
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
