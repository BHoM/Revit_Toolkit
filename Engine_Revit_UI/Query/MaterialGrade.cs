using Autodesk.Revit.DB;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        static public string MaterialGrade(this Element element)
        {
            if (element == null)
                return null;

            Parameter aParameter = element.LookupParameter("BHE_Material Grade");
            if (aParameter == null)
                return null;

            return aParameter.AsString();
        }

        /***************************************************/
    }
}