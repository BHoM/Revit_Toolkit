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
        /// Gets IntegerValue from Revit ElementId
        /// </summary>
        /// <param name="elementId">Revit ElementId</param>
        /// <returns name="Value">Integer Value</returns>
        /// <search>
        /// Query, BHoM, IntegerValue, ElementId
        /// </search>
        public static int IntegerValue(this ElementId elementId)
        {
            if (elementId == null)
                return Autodesk.Revit.DB.ElementId.InvalidElementId.IntegerValue;

            return elementId.IntegerValue;
        }

        /// <summary>
        /// Gets IntegerValue from Revit WorksetId
        /// </summary>
        /// <param name="worksetId">Revit WorksetId</param>
        /// <returns name="Value">Integer Value</returns>
        /// <search>
        /// Query, BHoM, IntegerValue, WorksetId
        /// </search>
        public static int IntegerValue(this WorksetId worksetId)
        {
            if (worksetId == null)
                return Autodesk.Revit.DB.WorksetId.InvalidWorksetId.IntegerValue;

            return worksetId.IntegerValue;
        }
    }
}
