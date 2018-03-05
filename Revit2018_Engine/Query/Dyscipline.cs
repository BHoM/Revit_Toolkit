using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Base;

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
        /// Returs discipline of BHoM type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns name="ElementId">Revit ElementId</returns>
        /// <search>
        /// Query, BHoM, discipline, Discipline, BHoMObject
        /// </search>
        public static Discipline Discipline(this Type type)
        {
            if (type == null)
                return Revit.Discipline.Environmental;

            if(type.Namespace.StartsWith("BH.oM.Structural"))
                return Revit.Discipline.Structural;

            if (type.Namespace.StartsWith("BH.oM.Environmental"))
                return Revit.Discipline.Environmental;

            return Revit.Discipline.Environmental;
        }

        /***************************************************/
    }
}
