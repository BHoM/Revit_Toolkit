using BH.oM.Adapters.Revit.Settings;
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

        [Description("Gets Include Selected property of FilterQuery.")]
        [Input("filterQuery", "FilterQuery")]
        [Output("IncludeSelected")]
        public static bool IncludeSelected(this FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return false;

            if (!filterQuery.Equalities.ContainsKey(Convert.FilterQuery.IncludeSelected))
                return false;

            if (filterQuery.Equalities[Convert.FilterQuery.IncludeSelected] is bool)
                return (bool)filterQuery.Equalities[Convert.FilterQuery.IncludeSelected];
            else
                return false;
        }

        /***************************************************/
    }
}