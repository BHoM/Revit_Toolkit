using System.ComponentModel;

using BH.oM.DataManipulation.Queries;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Base;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        [Description("Creates FilterQuery which filters all elements by given Revit Selection Set Name.")]
        [Input("slectionSetName", "Revit Slection Set Name")]
        [Output("FilterQuery")]
        public static FilterQuery SelectionSetFilterQuery(string slectionSetName)
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(BHoMObject);
            aFilterQuery.Equalities[Convert.FilterQuery.QueryType] = QueryType.SelectionSet;
            aFilterQuery.Equalities[Convert.FilterQuery.SelectionSetName] = slectionSetName;
            return aFilterQuery;
        }
    }
}
