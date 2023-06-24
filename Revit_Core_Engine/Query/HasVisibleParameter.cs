using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static bool HasVisibleParameter(this Element element, BuiltInParameter parameter)
        {
            if (element == null)
                return false;

            foreach(Parameter param in element.Parameters)
            {
                if ((param.Definition as InternalDefinition)?.BuiltInParameter == parameter)
                    return true;
            }

            return false;
        }

        /***************************************************/

        public static bool HasVisibleParameter(this Element element, string parameterName)
        {
            if (element == null)
                return false;

            foreach (Parameter param in element.Parameters)
            {
                if (param.Definition.Name == parameterName)
                    return true;
            }

            return false;
        }

        /***************************************************/
    }
}
