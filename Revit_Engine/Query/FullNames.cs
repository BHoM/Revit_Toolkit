using System;
using System.Collections.Generic;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        private static List<string> FullNames(Type type)
        {
            List<string> aResult = new List<string>();
            aResult.Add(type.FullName);
            if (type.BaseType != null)
                aResult.AddRange(FullNames(type.BaseType));
            return aResult;
        }

        /***************************************************/
    }
}