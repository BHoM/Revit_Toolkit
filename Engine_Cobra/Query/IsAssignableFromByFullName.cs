using System;
using System.Collections.Generic;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
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