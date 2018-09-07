using Autodesk.Revit.DB;
using BH.oM.Environment.Interface;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Internal Methods                          ****/
        /***************************************************/

        internal static Material ToRevit(this IMaterial material, Document document, PushSettings pushSettings = null)
        {
            ElementId aElementId = Material.Create(document, material.Name);
            return document.GetElement(aElementId) as Material;
        }

        /***************************************************/
    }
}