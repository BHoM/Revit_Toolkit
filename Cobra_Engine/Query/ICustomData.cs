using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Base;

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
        /// Returs CustomData value
        /// </summary>
        /// <param name="bHoMObject">BHoMObject</param>
        /// <param name="name">BHoMObject CustomData Name</param>
        /// <returns name="ElementId">Revit ElementId</returns>
        /// <search>
        /// Query, BHoM, ICustomData
        /// </search>
        public static object ICustomData(this IBHoMObject bHoMObject, string name)
        {
            if (bHoMObject == null)
                return null;

            object aObject;
            if (bHoMObject.CustomData.TryGetValue(name, out aObject))
                return aObject;

            return null;
        }

        /***************************************************/
    }
}

