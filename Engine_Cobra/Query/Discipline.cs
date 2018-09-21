using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.DataManipulation.Queries;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        static public Discipline Discipline(this FilterQuery filterQuery, RevitSettings revitSettings)
        {
            Discipline? aDiscipline = null;

            aDiscipline = BH.Engine.Adapters.Revit.Query.Discipline(filterQuery.Type);
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
    }
}
