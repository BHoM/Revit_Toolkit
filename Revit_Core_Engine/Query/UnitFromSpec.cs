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

        [Description("Returns document-specific Revit spec representing a given unit type.")]
        [Input("spec", "Revit spec queried for unit representing it.")]
        [Input("doc", "Revit document that contains the information about units used per each unit type (e.g. sqm for area).")]
        [Output("unit", "Revit unit representing the input spec.")]
        public static ForgeTypeId UnitFromSpec(this ForgeTypeId spec, Document doc)
        {
#if (REVIT2021)
            if (spec != null)
#else
            if (spec != null && UnitUtils.IsMeasurableSpec(spec))
#endif
                return doc.GetUnits().GetFormatOptions(spec).GetUnitTypeId();
            else
                return null;
        }
    }

    /***************************************************/
}
