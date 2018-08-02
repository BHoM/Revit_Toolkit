using Autodesk.Revit.DB;
using BH.oM.Base;
using System.Collections.Generic;

namespace BH.Engine.Revit
{
    /// <summary>
    /// BHoM Revit Engine Modify Methods
    /// </summary>
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Dictionary<ElementId, List<BHoMObject>> AddBHoMObjects(this Dictionary<ElementId, List<BHoMObject>> objects, IEnumerable<BHoMObject> bHoMObjects)
        {
            if (bHoMObjects == null && bHoMObjects == null)
                return null;

            Dictionary<ElementId, List<BHoMObject>> aResult = null;
            if (objects == null)
                aResult = new Dictionary<ElementId, List<BHoMObject>>();
            else
                aResult = new Dictionary<ElementId, List<BHoMObject>>(objects);

            foreach (BHoMObject aBHoMObject in bHoMObjects)
                aResult = aResult.AddBHoMObject(aBHoMObject);

            return aResult;
        }

        /***************************************************/
    }
}

