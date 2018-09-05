using System;
using System.Collections.Generic;

namespace BH.Engine.Adapters.Revit
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

            return FullNames(typeToCheck).Contains(type.FullName);
        }

        /***************************************************/
    }
}