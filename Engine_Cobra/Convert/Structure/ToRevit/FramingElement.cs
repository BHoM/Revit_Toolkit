using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit;
using BH.oM.Structure.Elements;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static FamilyInstance ToRevit(this FramingElement framingElement, Document document, PushSettings pushSettings = null)
        {
            if (framingElement == null || document == null)
                return null;

            pushSettings = pushSettings.DefaultIfNull();

            switch (framingElement.StructuralUsage)
            {
                case StructuralUsage1D.Column:
                    return framingElement.ToRevitStructuralColumn(document, pushSettings);
                case StructuralUsage1D.Beam:
                case StructuralUsage1D.Brace:
                case StructuralUsage1D.Cable:
                    return framingElement.ToRevitStructuralFraming(document, pushSettings);
                case StructuralUsage1D.Pile:
                    BH.Engine.Reflection.Compute.RecordError(string.Format("Push of pile foundations is not supported in current version of BHoM. BHoM element Guid: {0}", framingElement.BHoM_Guid));
                    return null;
                default:
                    BH.Engine.Reflection.Compute.RecordWarning(string.Format("Structural usage type is not set. An attempt to create a structural framing element is being made. BHoM element Guid: {0}", framingElement.BHoM_Guid));
                    return framingElement.ToRevitStructuralFraming(document, pushSettings);
            }
        }

        /***************************************************/
    }
}