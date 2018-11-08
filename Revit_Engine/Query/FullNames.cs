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

        [Description("Gets full names for given Type.")]
        [Input("type", "Type")]
        [Output("FullNames")]
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