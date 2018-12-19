using Autodesk.Revit.DB;
using BH.oM.Environment.Interface;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Internal Methods                          ****/
        /***************************************************/

        internal static Material ToRevitMaterial(this IMaterial material, Document document, PushSettings pushSettings = null)
        {
            Material aMaterial = pushSettings.FindRefObject<Material>(document, material.BHoM_Guid);
            if (aMaterial != null)
                return aMaterial;

            pushSettings.DefaultIfNull();

            aMaterial = document.GetElement(Material.Create(document, material.Name)) as Material;

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(material, aMaterial);

            return aMaterial;
        }

        /***************************************************/
    }
}