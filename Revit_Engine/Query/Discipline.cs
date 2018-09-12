using System;

using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Settings;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
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

        public static Discipline Discipline(this RevitSettings revitSettings, Type type)
        {
            Discipline? aDiscipline = Query.Discipline(type);
            if (aDiscipline == null || !aDiscipline.HasValue)
                aDiscipline = DefaultDiscipline(revitSettings);
            return aDiscipline.Value;
        }

        /***************************************************/
    }
}