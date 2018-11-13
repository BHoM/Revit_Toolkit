using System;

using BH.oM.Adapters.Revit.Enums;
using BH.oM.DataManipulation.Queries;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using BH.oM.Reflection.Attributes;
using BH.oM.Base;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Filters objects by type.")]
        [Input("objects", "Objects to be filtered by Type")]
        [Input("type", "Type")]
        [Output("Objects")]
        public static IEnumerable<object> FilterByType(this IEnumerable<object> objects, Type type)
        {
            if (type == null)
                return objects;

            if (objects == null || objects.Count() == 0)
                return objects;

            List<object> aResult = new List<object>();
            foreach (object aObject in objects)
                if (aObject.GetType() == type)
                    aResult.Add(aObject);

            return aResult;
        }

        /***************************************************/
    }
}