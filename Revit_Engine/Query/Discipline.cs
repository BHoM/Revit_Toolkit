using BH.oM.Revit;
using System;

namespace BH.Engine.Revit
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
                return oM.Revit.Discipline.Structural;

            if (type.Namespace.StartsWith("BH.oM.Environment"))
                return oM.Revit.Discipline.Environmental;

            if (type.Namespace.StartsWith("BH.oM.Architecture"))
                return oM.Revit.Discipline.Architecture;

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