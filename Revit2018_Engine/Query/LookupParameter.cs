using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Base;

using Autodesk.Revit.DB;

namespace BH.Engine.Revit
{
    /// <summary>
    /// BHoM Revit Engine Query Methods
    /// </summary>
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Parameter LookupParameter(this Element element, IEnumerable<string> parameterNames)
        {
            foreach (string s in parameterNames)
            {
                Parameter p= element.LookupParameter(s);
                if (p != null && p.HasValue) return p;
            }
            return null;
        }

        /***************************************************/

        public static double LookupParameterDouble(this Element element, string parameterName, bool convertUnits = true)
        {
            double value = double.NaN;
            Parameter p = element.LookupParameter(parameterName);
            if (p != null && p.HasValue)
            {
                value = p.AsDouble();
                if (convertUnits)
                    value = Convert.ToSI(value, p.Definition.UnitType);
            }
            return value;
        }

        /***************************************************/

        public static double LookupParameterDouble(this Element element, IEnumerable<string> parameterNames, bool convertUnits = true)
        {
            double value = double.NaN;
            Parameter p = element.LookupParameter(parameterNames);
            if (p != null)
            {
                value = p.AsDouble();
                if (convertUnits)
                    value = Convert.ToSI(value, p.Definition.UnitType);
            }
            return value;
        }

        /***************************************************/
    }
}