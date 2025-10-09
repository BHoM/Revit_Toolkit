using Autodesk.Revit.DB;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        public static long Value(this ElementId id)
        {
#if REVIT2022 || REVIT2023 || REVIT2024 || REVIT2025
            return id.IntegerValue;
#else
            return id.Value;
#endif
        }
    }
}
