using Autodesk.Revit.DB;

using BH.oM.Environment.Elements;
using BH.oM.Environment.Properties;
using BH.oM.Adapters.Revit.Settings;
using BH.UI.Revit.Engine;

namespace BH.UI.Revit.Adapter
{
    public partial class RevitUIAdapter
    {
        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static Element Create(BuildingElement buildingElement, Document document, PushSettings pushSettings = null)
        {
            if (buildingElement == null)
            {
                NullObjectCreateError(typeof(BuildingElement));
                return null;
            }

            if (buildingElement.BuildingElementProperties == null)
            {
                NullObjectCreateError(typeof(BuildingElementProperties));
                return null;
            }

            pushSettings = pushSettings.DefaultIfNull();

            //if (pushSettings.Replace)
            //    Delete(buildingElement.BuildingElementProperties, document);

            buildingElement.BuildingElementProperties.ToRevit(document, pushSettings);


            //Set Level
            /*if (buildingElement.Level != null)
                Create(buildingElement.Level, pushSettings);*/

            if (pushSettings.Replace)
                Delete(buildingElement, document);

            return buildingElement.ToRevit(document, pushSettings);
        }

        /***************************************************/
    }
}