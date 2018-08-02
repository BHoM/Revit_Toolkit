using System.Collections.Generic;

using BH.oM.Environment.Elements;
using BH.oM.Environment.Interface;

using Autodesk.Revit.DB;
using BH.oM.Base;

namespace BH.Engine.Revit
{

    /// <summary>
    /// BHoM Revit Engine Convert Methods
    /// </summary>
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Material ToRevit(this IMaterial material, Document document)
        {
            ElementId aElementId = Material.Create(document, material.Name);
            return document.GetElement(aElementId) as Material;
        }
    }
}
