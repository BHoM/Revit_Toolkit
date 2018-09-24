using Autodesk.Revit.DB;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static WorksetId ActiveWorksetId(this Document document)
        {
            if (document == null)
                return null;

            WorksetTable worksetTable = document.GetWorksetTable();
            return worksetTable.GetActiveWorksetId();
        }

        /***************************************************/
    }
}
