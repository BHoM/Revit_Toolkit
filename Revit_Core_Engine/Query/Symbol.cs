using Autodesk.Revit.DB;
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Returns symbol element of a given GeometryInstance, e.g. mullion type for a geometry instance representing a mullion.")]
        [Input("geometryInstance", "GeometryInstance to find symbol element for.")]
        [Output("symbol", "Symbol element of the input GeometryInstance.")]
        public static Element Symbol(this GeometryInstance geometryInstance)
        {
            if (geometryInstance == null)
                return null;

#if (REVIT2020 || REVIT2021 || REVIT2022)
            return geometryInstance.Symbol;
#else
            return geometryInstance.GetDocument().GetElement(geometryInstance.GetSymbolGeometryId().SymbolId);
#endif
        }

        /***************************************************/
    }
}
