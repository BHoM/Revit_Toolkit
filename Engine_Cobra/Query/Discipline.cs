using BH.oM.Revit;
using System;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static Discipline Discipline(this Type type)
        {
            if (type == null)
                return oM.Revit.Discipline.Environmental;

            if(type.Namespace.StartsWith("BH.oM.Structural"))
                return oM.Revit.Discipline.Structural;

            if (type.Namespace.StartsWith("BH.oM.Environment"))
                return oM.Revit.Discipline.Environmental;

            if (type.Namespace.StartsWith("BH.oM.Architecture"))
                return oM.Revit.Discipline.Architecture;

            return oM.Revit.Discipline.Environmental;
        }

        /***************************************************/
    }
}