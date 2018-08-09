using BH.oM.Base;
using BH.oM.Revit;
using System.Collections.Generic;

namespace BH.Engine.Revit
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
