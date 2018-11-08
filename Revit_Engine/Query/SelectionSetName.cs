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

        [Description("Returns Revit SelectionSet Name for given FilterQuery")]
        [Input("filterQuery", "FilterQuery")]
        [Output("SelectionSetName")]
        public static string SelectionSetName(this FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return null;

            if (!filterQuery.Equalities.ContainsKey(Convert.FilterQuery.SelectionSetName))
                return null;

            return filterQuery.Equalities[Convert.FilterQuery.SelectionSetName] as string;
        }

        /***************************************************/
    }
}
