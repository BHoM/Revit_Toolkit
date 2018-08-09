using System;
using System.Collections.Generic;
using System.Linq;

using BH.oM.Environment.Elements;
using BH.oM.Environment.Properties;
using BH.oM.Structural.Elements;
using BH.Engine.Revit;
using BH.oM.Base;

using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit;

namespace BH.UI.Revit.Adapter
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

            if (pushSettings == null)
                pushSettings = PushSettings.Default;

            //Set ElementType
            Create(buildingElement.BuildingElementProperties, pushSettings);

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