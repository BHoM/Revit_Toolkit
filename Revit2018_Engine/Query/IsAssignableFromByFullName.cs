using System;
using System.Collections.Generic;

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
        /// Check if type is assignable from another type by checking its full name
        /// </summary>
        /// <param name="type">Source type</param>
        /// <param name="typeToCheck">Type to check</param>
        /// <returns name="Result">type is assignable from another type</returns>
        /// <search>
        /// Query, BHoM, Revit, IsAssignableFromByFullName, Is Assignable From By Full Name, is assignable from by full name
        /// </search>
        public static bool IsAssignableFromByFullName(this Type typeToCheck, Type type)
        {
            if (type == null || typeToCheck == null)
                return false;

            return GetFullNames(typeToCheck).Contains(type.FullName);
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static List<string> GetFullNames(Type type)
        {
            List<string> aResult = new List<string>();
            aResult.Add(type.FullName);
            if (type.BaseType != null)
                aResult.AddRange(GetFullNames(type.BaseType));
            return aResult;
        }

        /***************************************************/
    }
}
