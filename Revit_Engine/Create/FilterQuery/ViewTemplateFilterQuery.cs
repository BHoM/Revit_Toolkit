using System.ComponentModel;

using BH.oM.DataManipulation.Queries;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Base;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        [Description("Creates FilterQuery which filters all View Templates in Revit Document.")]
        [Output("FilterQuery")]
        public static FilterQuery ViewTemplateFilterQuery()
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(BHoMObject);
            aFilterQuery.Equalities[Convert.FilterQuery.QueryType] = QueryType.ViewTemplate;
            return aFilterQuery;
        }

        [Description("Creates FilterQuery which filters View Template by given name.")]
        [Input("viewTemplateName", "Revit View Template Name")]
        [Output("FilterQuery")]
        public static FilterQuery ViewTemplateFilterQuery(string viewTemplateName)
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(BHoMObject);
            aFilterQuery.Equalities[Convert.FilterQuery.QueryType] = QueryType.ViewTemplate;
            aFilterQuery.Equalities[Convert.FilterQuery.ViewTemplateName] = viewTemplateName;
            return aFilterQuery;
        }
    }
}
