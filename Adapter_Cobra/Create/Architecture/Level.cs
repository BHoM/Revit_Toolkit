using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit;
using BH.UI.Cobra.Engine;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Cobra.Adapter
{
    public partial class CobraAdapter
    {
        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private bool Create(oM.Architecture.Elements.Level level, PushSettings pushSettings = null)
        {
            if (level == null)
            {
                NullObjectCreateError(typeof(oM.Architecture.Elements.Level));
                return false;
            }

            pushSettings.DefaultIfNull();

            if (pushSettings.Replace)
                Delete(level);

            List<Element> aElementList = new FilteredElementCollector(Document).OfClass(typeof(Level)).ToList();
            if (aElementList == null || aElementList.Count < 1)
            {
                level.ToRevit(Document, pushSettings);
                return true;
            }

            Element aElement = aElementList.Find(x => x.Name == level.Name);
            if (aElement == null)
            {
                level.ToRevit(Document, pushSettings);
                return true;
            }

            return false;
        }

        /***************************************************/
    }
}