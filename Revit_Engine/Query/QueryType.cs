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

        [Description("Returns Query Type of given FilterQuery")]
        [Input("filterQuery", "FilterQuery")]
        [Output("QueryType")]
        public static oM.Adapters.Revit.Enums.QueryType QueryType(this FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return oM.Adapters.Revit.Enums.QueryType.Undefined;

            if (!filterQuery.Equalities.ContainsKey(Convert.FilterQuery.QueryType))
                return oM.Adapters.Revit.Enums.QueryType.Undefined;

            if (filterQuery.Equalities[Convert.FilterQuery.QueryType] is oM.Adapters.Revit.Enums.QueryType || filterQuery.Equalities[Convert.FilterQuery.QueryType] is int)
                return (oM.Adapters.Revit.Enums.QueryType)filterQuery.Equalities[Convert.FilterQuery.QueryType];

            return oM.Adapters.Revit.Enums.QueryType.Undefined;
        }

        /***************************************************/
    }
}

