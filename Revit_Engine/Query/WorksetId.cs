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
    /// BHoM Revit Engine Query Methods
    /// </summary>
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        /// <summary>
        /// Reads Revit WorksetId from BHoMObject CustomData. Key: BH.Adapter.Revit.Id.WorksetId
        /// </summary>
        /// <param name="bHoMObject">BHoMObject</param>
        /// <returns name="UniqueId">Revit UniqueId</returns>
        /// <search>
        /// Query, BHoM, WorksetId, BHoMObject
        /// </search>
        public static WorksetId WorksetId(this BHoMObject bHoMObject)
        {
            if (bHoMObject == null)
                return null;

            object aValue = null;
            if (bHoMObject.CustomData.TryGetValue(Convert.WorksetId, out aValue))
            {
                if (aValue is string)
                {
                    int aInt = -1;
                    if (int.TryParse((string)aValue, out aInt))
                        return new WorksetId(aInt);
                }
                else if (aValue is int)
                {
                    return new WorksetId((int)aValue);
                }
                else
                {
                    return null;
                }
            }

            return null;
        }
    }
}
