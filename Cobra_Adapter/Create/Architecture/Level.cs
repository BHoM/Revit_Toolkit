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

        private bool Create(oM.Architecture.Elements.Level level, bool copyCustomData = true, bool replace = false)
        {
            if (level == null)
            {
                NullObjectCreateError(typeof(oM.Architecture.Elements.Level));
                return false;
            }

            if (replace)
                Delete(level);

            List<Element> aElementList = new FilteredElementCollector(Document).OfClass(typeof(Level)).ToList();
            if (aElementList == null || aElementList.Count < 1)
            {
                level.ToRevit(Document, copyCustomData);
                return true;
            }

            Element aElement = aElementList.Find(x => x.Name == level.Name);
            if (aElement == null)
            {
                level.ToRevit(Document, copyCustomData);
                return true;
            }

            return false;
        }

        /***************************************************/
    }
}