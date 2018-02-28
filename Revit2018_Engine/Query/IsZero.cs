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
        /// Checks if double value is zero. Method takes into account Machine epsilon error
        /// </summary>
        /// <param name="double">double value</param>
        /// <returns name="IsZero">IsZero</returns>
        /// <search>
        /// Query, IsZero, Revit, Is Zero, double
        /// </search>
        static public bool IsZero(double @double)
        {
            return oM.Geometry.Tolerance.Distance > Math.Abs(@double);
        }

        /***************************************************/
    }
}
