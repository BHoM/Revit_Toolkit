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

        private static Level Create(oM.Architecture.Elements.Level level, Document document, PushSettings pushSettings = null)
        {
            if (level == null)
            {
                NullObjectCreateError(typeof(oM.Architecture.Elements.Level));
                return null;
            }

            pushSettings = pushSettings.DefaultIfNull();

            if (pushSettings.Replace)
                Delete(level, document);

            List<Element> aElementList = new FilteredElementCollector(document).OfClass(typeof(Level)).ToList();
            if (aElementList == null || aElementList.Count < 1)
                return level.ToRevit(document, pushSettings) as Level;

            Element aElement = aElementList.Find(x => x.Name == level.Name);
            if (aElement == null)
                return level.ToRevit(document, pushSettings) as Level;

            return null;
        }

        /***************************************************/
    }
}