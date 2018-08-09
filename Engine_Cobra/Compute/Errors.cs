using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace BH.UI.Cobra.Engine
{
    public static partial class Compute
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static void NotConvertedError(this Element element)
        {
            string aMessage = "Revit element could not be converted because conversion method does not exist.";

            if (element != null)
                aMessage = string.Format("{0} Element Id: {1}, Element Name: {2}", aMessage, element.Id.IntegerValue, element.Name);

            BH.Engine.Reflection.Compute.RecordError(aMessage);
        }

        /***************************************************/

        internal static void NotConvertedError(this Document document)
        {
            string aMessage = "Revit document could not be converted because conversion method does not exist.";

            if (document != null)
                aMessage = string.Format("{0} Document title: {1}", aMessage, document.Title);

            BH.Engine.Reflection.Compute.RecordError(aMessage);
        }

        /***************************************************/

        internal static void NotConvertedError(this StructuralMaterialType structuralMaterialType)
        {
            BH.Engine.Reflection.Compute.RecordError("Structural meterial type " + structuralMaterialType + " could not be converted because conversion method does not exist.");
        }

        /***************************************************/

        internal static void CheckIfNull(this Element element)
        {
            if (element == null)
                BH.Engine.Reflection.Compute.RecordError("BHoM object could not be read because Revit element does not exist.");
        }

        /***************************************************/

        internal static void NullDocumentError()
        {
            BH.Engine.Reflection.Compute.RecordError("BHoM object could not be read because Revit document does not exist.");
        }

        /***************************************************/

        internal static void CheckIfNull(this Document document)
        {
            if (document == null)
                NullDocumentError();
        }

        /***************************************************/
    }
}