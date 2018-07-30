using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using BH.oM.Base;

namespace BH.Engine.Revit
{
    /// <summary>
    /// BHoM Revit Engine Query Methods
    /// </summary>
    public static partial class Query
    {
        /// <summary>
        /// Reads Revit UniqueId from BHoMObject CustomData. Key: Utilis.AdapterId
        /// </summary>
        /// <param name="bHoMObject">BHoMObject</param>
        /// <returns name="UniqueId">Revit UniqueId</returns>
        /// <search>
        /// Query, BHoM, UniqueId, BHoMObject
        /// </search>
        public static string UniqueId(this BHoMObject bHoMObject)
        {
            if (bHoMObject == null)
                return null;

            object aValue = null;
            if (bHoMObject.CustomData.TryGetValue(Adapter.Revit.Id.AdapterId, out aValue))
            {
                if (aValue is string)
                    return (string)aValue;
                else
                    return null;
            }

            return null;
        }
    }

}

