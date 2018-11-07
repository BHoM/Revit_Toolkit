using BH.oM.Adapters.Revit.Enums;
using BH.oM.DataManipulation.Queries;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Sets Default Discipline for FilterQuery.")]
        [Input("filterQuery", "FilterQuery")]
        [Input("discipline", "Discipline to be set")]
        [Output("FilterQuery")]
        public static FilterQuery SetDefaultDiscipline(this FilterQuery filterQuery, Discipline discipline)
        {
            if (filterQuery == null)
                return null;

            FilterQuery aFilterQuery = Query.Duplicate(filterQuery);

            if (aFilterQuery.Equalities.ContainsKey(Convert.FilterQuery.DefaultDiscipline))
                aFilterQuery.Equalities[Convert.FilterQuery.DefaultDiscipline] = discipline;
            else
                aFilterQuery.Equalities.Add(Convert.FilterQuery.DefaultDiscipline, discipline);

            return aFilterQuery;
        }

        /***************************************************/
    }
}
