using BH.oM.Reflection.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Checks if one type is assignable from another type by comparing its full name.")]
        [Input("filterQuery", "FilterQuery")]
        [Output("IncludeSelected")]
        public static bool IsAssignableFromByFullName(this Type typeToCheck, Type type)
        {
            if (type == null || typeToCheck == null)
                return false;

            return FullNames(typeToCheck).Contains(type.FullName);
        }

        /***************************************************/
    }
}