using BH.oM.DataManipulation.Queries;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Returns WorksetName assigned to FilterQuery.")]
        [Input("filterQuery", "FilterQuery")]
        [Output("WorksetName")]
        public static string WorksetName(this FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return null;

            if (!filterQuery.Equalities.ContainsKey(Convert.FilterQuery.WorksetName))
                return null;

            return filterQuery.Equalities[Convert.FilterQuery.WorksetName] as string;
        }

        /***************************************************/
    }
}
