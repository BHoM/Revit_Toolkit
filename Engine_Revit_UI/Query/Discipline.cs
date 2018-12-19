using System.Linq;
using System.Collections.Generic;

using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.DataManipulation.Queries;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        static public Discipline Discipline(this FilterQuery filterQuery, RevitSettings revitSettings)
        {
            Discipline? aDiscipline = null;

            aDiscipline = BH.Engine.Adapters.Revit.Query.Discipline(filterQuery);
            if (aDiscipline != null && aDiscipline.HasValue)
                return aDiscipline.Value;

            aDiscipline = BH.Engine.Adapters.Revit.Query.DefaultDiscipline(filterQuery);
            if (aDiscipline != null && aDiscipline.HasValue)
                return aDiscipline.Value;

            aDiscipline = BH.Engine.Adapters.Revit.Query.DefaultDiscipline(revitSettings);
            if (aDiscipline != null && aDiscipline.HasValue)
                return aDiscipline.Value;

            return oM.Adapters.Revit.Enums.Discipline.Structural;
        }

        /***************************************************/

        static public Discipline Discipline(this IEnumerable<FilterQuery> filterQueries, RevitSettings revitSettings)
        {
            if (filterQueries == null || filterQueries.Count() == 0)
                return oM.Adapters.Revit.Enums.Discipline.Structural;

            Discipline? aDiscipline = null;

            foreach (FilterQuery aFilterQuery in filterQueries)
            {
                aDiscipline = BH.Engine.Adapters.Revit.Query.Discipline(aFilterQuery);
                if (aDiscipline != null && aDiscipline.HasValue)
                    return aDiscipline.Value;
            }

            foreach (FilterQuery aFilterQuery in filterQueries)
            {
                aDiscipline = BH.Engine.Adapters.Revit.Query.DefaultDiscipline(aFilterQuery);
                if (aDiscipline != null && aDiscipline.HasValue)
                    return aDiscipline.Value;
            }

            aDiscipline = BH.Engine.Adapters.Revit.Query.DefaultDiscipline(revitSettings);
            if (aDiscipline != null && aDiscipline.HasValue)
                return aDiscipline.Value;

            return oM.Adapters.Revit.Enums.Discipline.Structural;
        }

        /***************************************************/
    }
}
