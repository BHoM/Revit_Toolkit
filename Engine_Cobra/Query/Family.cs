using Autodesk.Revit.DB;

using BH.oM.Base;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Family Family(this IBHoMObject bHoMObject, Document document)
        {
            if (document == null || bHoMObject == null)
                return null;

            string aFamilyName = BH.Engine.Adapters.Revit.Query.FamilyName(bHoMObject);
            if (string.IsNullOrEmpty(aFamilyName) && !string.IsNullOrEmpty(bHoMObject.Name) && bHoMObject.Name.Contains(":"))
                aFamilyName = BH.Engine.Adapters.Revit.Query.FamilyName(bHoMObject.Name);

            if (string.IsNullOrWhiteSpace(aFamilyName))
                return null;

            List<Family> aFamilyList = new FilteredElementCollector(document).OfClass(typeof(Family)).Cast<Family>().ToList();
            return aFamilyList.Find(x => x.Name == aFamilyName);
        }

        /***************************************************/
    }
}
