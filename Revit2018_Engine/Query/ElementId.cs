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
        /// Reads Revit ElementId from BHoMObject CustomData. Key: Utilis.ElementId
        /// </summary>
        /// <param name="bHoMObject">BHoMObject</param>
        /// <returns name="ElementId">Revit ElementId</returns>
        /// <search>
        /// Query, BHoM, ElemenId, elementid, BHoMObject
        /// </search>
        public static ElementId ElementId(this IBHoMObject bHoMObject)
        {
            if (bHoMObject == null)
                return null;

            object aValue = null;
            if (bHoMObject.CustomData.TryGetValue(Convert.ElementId, out aValue))
            {
                if (aValue is string)
                {
                    int aInt = -1;
                    if (int.TryParse((string)aValue, out aInt))
                        return new ElementId(aInt);
                }
                else if (aValue is int)
                {
                    return new ElementId((int)aValue);
                }
                else
                {
                    return null;
                }
            }

            return null;
        }

        /***************************************************/
    }
}
