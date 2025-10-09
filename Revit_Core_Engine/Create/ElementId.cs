using Autodesk.Revit.DB;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        public static ElementId ElementId(int id)
        {
            return new ElementId(id);
        }

        public static ElementId ElementId(long id)
        {
#if REVIT2022 || REVIT2023
            return new ElementId((int)id);
#else
            return new ElementId(id);
#endif
        }
    }
}