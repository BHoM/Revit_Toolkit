using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

using BH.oM.DataManipulation.Queries;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static oM.Adapters.Revit.Enums.QueryType QueryType(this FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return oM.Adapters.Revit.Enums.QueryType.Undefined;

            if (!filterQuery.Equalities.ContainsKey(Convert.FilterQuery.QueryType))
                return oM.Adapters.Revit.Enums.QueryType.Undefined;

            if (filterQuery.Equalities[Convert.FilterQuery.QueryType] is oM.Adapters.Revit.Enums.QueryType)
                return (oM.Adapters.Revit.Enums.QueryType)filterQuery.Equalities[Convert.FilterQuery.QueryType];

            return oM.Adapters.Revit.Enums.QueryType.Undefined;
        }

        /***************************************************/
    }
}

