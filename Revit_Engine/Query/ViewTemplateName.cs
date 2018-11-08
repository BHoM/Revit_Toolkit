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

        [Description("Returns View Template Name for given FilterQuery.")]
        [Input("filterQuery", "FilterQuery")]
        [Output("ViewTemplateName")]
        public static string ViewTemplateName(this FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return null;

            if (!filterQuery.Equalities.ContainsKey(Convert.FilterQuery.ViewTemplateName))
                return null;

            return filterQuery.Equalities[Convert.FilterQuery.ViewTemplateName] as string;
        }

        /***************************************************/
    }
}
