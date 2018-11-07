using System.ComponentModel;
using System.Collections.Generic;

using BH.oM.Base;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Creates Pull Settings class which contols pull behaviour of Adapter")]
        [Input("discipline", "Default disciplne for pull method")]
        [Input("refObjects", "Additional reference objects created during Pull process")]
        [Input("copyCustomData", "Saves Parameters of Revit Element into CustomData of BHoM Object")]
        [Input("convertUnits", "Converts units of parameters to SI")]
        [Output("PullSettings")]
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

        [Description("Creates Pull Settings class which contols pull behaviour of Adapter")]
        [Input("discipline", "Default disciplne for pull method")]
        [Output("PullSettings")]
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
