using System.Collections.Generic;

using BH.oM.Adapters.Revit;
using BH.oM.Base;

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
    }
}
