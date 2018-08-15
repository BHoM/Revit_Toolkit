using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static List<BuiltInCategory> SelectionBuiltInCategories(this UIDocument uIDocument)
        {
            if (uIDocument == null)
                return null;

            Selection aSelection = uIDocument.Selection;
            if (aSelection == null)
                return null;

            Document aDocument = uIDocument.Document;
            if (aDocument == null)
                return null;

            List<BuiltInCategory> aResult = new List<BuiltInCategory>();
            foreach(ElementId aElementId in aSelection.GetElementIds())
            {
                Element aElement = aDocument.GetElement(aElementId);
                if(aElement != null && aElement.Category != null)
                {
                    BuiltInCategory aBuiltInCategory = (BuiltInCategory)aElement.Category.Id.IntegerValue;
                    if (!aResult.Contains(aBuiltInCategory))
                        aResult.Add(aBuiltInCategory);
                }
            }
            return aResult;
        }

        /***************************************************/
    }
}