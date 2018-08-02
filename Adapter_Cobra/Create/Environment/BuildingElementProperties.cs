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

        private bool Create(BuildingElementProperties buildingElementProperties, bool copyCustomData = true, bool replace = false)
        {
            if (buildingElementProperties == null)
            {
                NullObjectCreateError(typeof(BuildingElementProperties));
                return false;
            }

            if (replace)
                Delete(buildingElementProperties);

            Type aType = Query.RevitType(buildingElementProperties.BuildingElementType);
            List<Element> aElementList = new FilteredElementCollector(Document).OfClass(aType).ToList();
            if (aElementList == null && aElementList.Count < 1)
                return false;

            Element aElement = aElementList.Find(x => x.Name == buildingElementProperties.Name);
            if (aElement == null)
            {
                aElement = buildingElementProperties.ToRevit(Document, copyCustomData);
                return true;
            }

            return false;
        }

        /***************************************************/
    }
}