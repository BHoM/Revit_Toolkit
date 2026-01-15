using Autodesk.Revit.DB;

namespace BH.Revit.Engine.Core
{
    internal class SketchUpdateFailurePreprocessor : IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            return FailureProcessingResult.Continue;
        }
    }
}
