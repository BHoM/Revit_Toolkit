using System;

using BH.oM.Adapters.Revit.Enums;
using BH.oM.DataManipulation.Queries;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Gets Discipline for given BHoM Type.")]
        [Input("type", "BHoM Type")]
        [Output("Discipline")]
        public static Discipline? Discipline(this Type type)
        {
            if (type == null)
                return null;

            if(type.Namespace.StartsWith("BH.oM.Structure"))
                return oM.Adapters.Revit.Enums.Discipline.Structural;

            if (type.Namespace.StartsWith("BH.oM.Environment"))
                return oM.Adapters.Revit.Enums.Discipline.Environmental;

            if (type.Namespace.StartsWith("BH.oM.Architecture"))
                return oM.Adapters.Revit.Enums.Discipline.Architecture;

            return null;
        }

        /***************************************************/

        [Description("Gets Discipline for given FilterQuery.")]
        [Input("filterQuery", "FilterQuery")]
        [Output("Discipline")]
        public static Discipline? Discipline(this FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return null;

            List<Discipline> aDisciplineList = new List<Discipline>();

            IEnumerable<FilterQuery> aFilterQueries = Query.FilterQueries(filterQuery);
            if (aFilterQueries != null && aFilterQueries.Count() > 0)
            {
                foreach (FilterQuery aFilterQuery in aFilterQueries)
                {
                    Discipline? aDiscipline = Discipline(filterQuery);
                    if (aDiscipline != null && aDiscipline.HasValue)
                        return aDiscipline;
                }
            }
            else
            {
                Discipline? aDiscipline = Discipline(filterQuery.Type);
                if (aDiscipline != null && aDiscipline.HasValue)
                    return aDiscipline.Value;

                aDiscipline = DefaultDiscipline(filterQuery);
                if (aDiscipline != null && aDiscipline.HasValue)
                    return aDiscipline.Value;
            }

            return null;
        }

        /***************************************************/
    }
}