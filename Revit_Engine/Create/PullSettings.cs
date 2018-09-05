using BH.oM.Base;
using BH.oM.Adapters.Revit;
using System.Collections.Generic;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static PullSettings PullSettings(Discipline discipline = Discipline.Environmental, Dictionary<int, List<IBHoMObject>> refObjects = null, bool copyCustomData = true, bool convertUnits = true)
        {
            PullSettings aPullSettings = new PullSettings()
            {
                Discipline = discipline,
                CopyCustomData = copyCustomData,
                ConvertUnits = convertUnits,
                RefObjects = refObjects
            };

            return aPullSettings;
        }

        /***************************************************/

        public static PullSettings PullSettings(Discipline discipline)
        {
            PullSettings aPullSettings = new PullSettings()
            {
                Discipline = discipline,
                RefObjects = new Dictionary<int, List<IBHoMObject>>(),
            };

            return aPullSettings;
        }

        /***************************************************/

    }
}
