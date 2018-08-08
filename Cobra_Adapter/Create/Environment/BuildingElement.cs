using System;
using System.Collections.Generic;
using System.Linq;

using BH.oM.Environment.Elements;
using BH.oM.Environment.Properties;
using BH.oM.Structural.Elements;
using BH.Engine.Revit;
using BH.oM.Base;

using Autodesk.Revit.DB;


namespace BH.UI.Revit.Adapter
{
    public partial class CobraAdapter
    {
        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private bool Create(BuildingElement buildingElement, bool copyCustomData = true, bool replace = false)
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

            //Set ElementType
            Create(buildingElement.BuildingElementProperties, copyCustomData, false);

            //Set Level
            if (buildingElement.Level != null)
                Create(buildingElement.Level, copyCustomData, false);

            if (replace)
                Delete(buildingElement);

            buildingElement.ToRevit(Document, copyCustomData);

            return true;
        }

        /***************************************************/
    }
}