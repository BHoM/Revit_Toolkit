using Autodesk.Revit.DB;
using BH.oM.Environment.Properties;
using BH.oM.Revit;
using BH.UI.Cobra.Engine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Cobra.Adapter
{
    public partial class CobraAdapter
    {
        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private bool Create(BuildingElementProperties buildingElementProperties, PushSettings pushSettings = null)
        {
            if (buildingElementProperties == null)
            {
                NullObjectCreateError(typeof(BuildingElementProperties));
                return false;
            }

            pushSettings.DefaultIfNull();

            if (pushSettings.Replace)
                Delete(buildingElementProperties);

            Type aType = Query.RevitType(buildingElementProperties.BuildingElementType);
            List<Element> aElementList = new FilteredElementCollector(Document).OfClass(aType).ToList();
            if (aElementList == null && aElementList.Count < 1)
                return false;

            Element aElement = aElementList.Find(x => x.Name == buildingElementProperties.Name);
            if (aElement == null)
            {
                aElement = buildingElementProperties.ToRevit(Document, pushSettings);
                return true;
            }

            return false;
        }

        /***************************************************/
    }
}