using Autodesk.Revit.DB;
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns true if an element contains the given parameter among its visible parameters.")]
        [Input("element", "Revit element to check whether it has a given parameter.")]
        [Input("parameter", "Parameter to search for.")]
        [Output("hasParameter", "True if the input element contains the given parameter among its visible parameters, otherwise false.")]
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

        [Description("Returns true if an element contains a parameter with the give name among its visible parameters.")]
        [Input("element", "Revit element to check whether it has a parameter with the given name.")]
        [Input("parameterName", "Parameter name to search for.")]
        [Output("hasParameter", "True if the input element contains a parameter with the given name among its visible parameters, otherwise false.")]
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
