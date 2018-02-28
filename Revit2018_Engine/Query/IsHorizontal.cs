using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace BH.Engine.Revit
{
    /// <summary>
    /// BHoM Revit Engine Modify Methods
    /// </summary>
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        /// <summary>
        /// Checks if Revit PlanarFace is horizontal
        /// </summary>
        /// <param name="planarFace">Revit PlanarFace</param>
        /// <returns name="IsHorizontal">Is Horizontal</returns>
        /// <search>
        /// Query, IsHorizontal, Revit, Is Horizontal, PlanarFace, Planar Face
        /// </search>
        static public bool IsHorizontal(PlanarFace planarFace)
        {
            return IsVertical(planarFace.FaceNormal);
        }

        /***************************************************/
    }
}
