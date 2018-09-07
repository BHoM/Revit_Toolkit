using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;
using BH.UI.Cobra.Engine;

namespace BH.UI.Cobra.Adapter
{
    public partial class CobraAdapter
    {
        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private Level Create(oM.Architecture.Elements.Level level, PushSettings pushSettings = null)
        {
            if (level == null)
            {
                NullObjectCreateError(typeof(oM.Architecture.Elements.Level));
                return null;
            }

            pushSettings = pushSettings.DefaultIfNull();

            if (pushSettings.Replace)
                Delete(level);

            List<Element> aElementList = new FilteredElementCollector(Document).OfClass(typeof(Level)).ToList();
            if (aElementList == null || aElementList.Count < 1)
                return level.ToRevit(Document, pushSettings) as Level;

            Element aElement = aElementList.Find(x => x.Name == level.Name);
            if (aElement == null)
                return level.ToRevit(Document, pushSettings) as Level;

            return null;
        }

        /***************************************************/
    }
}