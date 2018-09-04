using BH.oM.Environment.Elements;
using BH.oM.Environment.Properties;
using BH.oM.Revit;
using BH.UI.Cobra.Engine;

namespace BH.UI.Cobra.Adapter
{
    public partial class CobraAdapter
    {
        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private bool Create(BuildingElement buildingElement, PushSettings pushSettings = null)
        {
            if (buildingElement == null)
            {
                NullObjectCreateError(typeof(BuildingElement));
                return false;
            }

            if (buildingElement.BuildingElementProperties == null)
            {
                NullObjectCreateError(typeof(BuildingElementProperties));
                return false;
            }

            pushSettings.DefaultIfNull();

            if (pushSettings.Replace)
                Delete(buildingElement.BuildingElementProperties);

            buildingElement.BuildingElementProperties.ToRevit(Document, pushSettings);


            //Set Level
            if (buildingElement.Level != null)
                Create(buildingElement.Level, pushSettings);

            if (pushSettings.Replace)
                Delete(buildingElement);

            buildingElement.ToRevit(Document, pushSettings);

            return true;
        }

        /***************************************************/
    }
}