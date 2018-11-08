using System.ComponentModel;

using BH.oM.DataManipulation.Queries;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Returns Type Name for given FilterQuery")]
        [Input("filterQuery", "FilterQuery")]
        [Output("TypeName")]
        public static string TypeName(this FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return null;

            if (!filterQuery.Equalities.ContainsKey(Convert.FilterQuery.TypeName))
                return null;

            return filterQuery.Equalities[Convert.FilterQuery.TypeName] as string;
        }

        /***************************************************/
    }
}
