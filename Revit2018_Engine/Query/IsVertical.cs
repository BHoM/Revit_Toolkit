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
        /// Checks if Revit Vector (XYZ) is Vertical
        /// </summary>
        /// <param name="xyz">Revit Vector</param>
        /// <returns name="IsVertical">Is Vertical</returns>
        /// <search>
        /// Query, IsVertical, Revit, Is Vertical, XYZ, Vector
        /// </search>
        static public bool IsVertical(XYZ xyz)
        {
            return Query.IsZero(xyz.X) && Query.IsZero(xyz.Y);
        }

        /***************************************************/
    }
}
