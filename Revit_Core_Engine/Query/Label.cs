using Autodesk.Revit.DB;
using BH.oM.Base.Attributes;
using BH.Revit.Engine.Core;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        //[Description("Returns the human-readable label of a given Revit unit type.")]
        //[Input("parameterType", "Unit type to get the label for.")]
        //[Output("label", "Human-readable label of the input Revit unit type.")]
#if (REVIT2018 || REVIT2019 || REVIT2020 || REVIT2021)
        public static string Label(this ParameterType parameterType)
        {
            return LabelUtils.GetLabelFor(parameterType);
        }
#endif
#if (!REVIT2018 && !REVIT2019 && !REVIT2020)
        public static string Label(this ForgeTypeId parameterType)
        {
            if (parameterType != null)
                return LabelUtils.GetLabelForSpec(parameterType);
            else
                return null;
        }
#endif

        /***************************************************/
    }
}
