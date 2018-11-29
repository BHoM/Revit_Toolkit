using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit.Settings;
using System.Linq;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        internal static Level ToRevitLevel(this oM.Architecture.Elements.Level level, Document document, PushSettings pushSettings = null)
        {
            Level aLevel = pushSettings.FindRefObject<Level>(document, level.BHoM_Guid);
            if (aLevel != null)
                return aLevel;

            pushSettings.DefaultIfNull();

            ElementId aElementId = level.ElementId();

            if (aElementId != null && aElementId != ElementId.InvalidElementId)
                aLevel = document.GetElement(aElementId) as Level;

            if(aLevel == null)
                aLevel = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().ToList().Find(x => x.Name == level.Name);

            if(aLevel == null)
            {
                aLevel = Level.Create(document, level.Elevation);
                aLevel.Name = level.Name;
            }

            aLevel.CheckIfNullPush(level);
            if (aLevel == null)
                return null;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aLevel, level, new BuiltInParameter[] { BuiltInParameter.DATUM_TEXT, BuiltInParameter.LEVEL_ELEV }, pushSettings.ConvertUnits);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(level, aLevel);

            return aLevel;
        }

        /***************************************************/
    }
}