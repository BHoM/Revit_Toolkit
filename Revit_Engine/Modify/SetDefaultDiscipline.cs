using BH.oM.Adapters.Revit.Enums;
using BH.oM.DataManipulation.Queries;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static FilterQuery SetDefaultDiscipline(this FilterQuery filterQuery, Discipline Discipline)
        {
            if (filterQuery == null)
                return null;

            FilterQuery aFilterQuery = Query.Duplicate(filterQuery);

            if (aFilterQuery.Equalities.ContainsKey(Convert.FilterQuery.DefaultDiscipline))
                aFilterQuery.Equalities[Convert.FilterQuery.DefaultDiscipline] = Discipline;
            else
                aFilterQuery.Equalities.Add(Convert.FilterQuery.DefaultDiscipline, Discipline);

            return aFilterQuery;
        }

        /***************************************************/
    }
}
