using Autodesk.Revit.DB;
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    internal class SketchUpdateFailurePreprocessor : IFailuresPreprocessor
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Preprocesses failures that occur during sketch updates, allowing the operation to continue despite warnings or errors.")]
        [Input("failuresAccessor", "Revit failures accessor object containing failure messages from the sketch update operation.")]
        [Output("failureProcessingResult", "The result of the failure processing, indicating whether to continue with the operation.")]
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            return FailureProcessingResult.Continue;
        }

        /***************************************************/
    }
}
