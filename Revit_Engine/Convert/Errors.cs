using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace BH.Engine.Revit
{

    /// <summary>
    /// BHoM Revit Engine Convert Methods
    /// </summary>
    public static partial class Convert
    {
        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static void NotConvertedError(this Element element)
        {
            Engine.Reflection.Compute.RecordError(string.Format("Revit element could not be converted because conversion method does not exist. Element Id: {0}, Element Name: {1}", element.Id.IntegerValue, element.Name));
        }

        /***************************************************/

        private static void NotConvertedError(this Document document)
        {
            Engine.Reflection.Compute.RecordError(string.Format("Revit document could not be converted because conversion method does not exist. Document title: {0}", document.Title));
        }

        /***************************************************/

        private static void NotConvertedError(this StructuralMaterialType structuralMaterialType)
        {
            Engine.Reflection.Compute.RecordError("Structural meterial type " + structuralMaterialType + " could not be converted because conversion method does not exist.");
        }

        /***************************************************/

        private static void CheckIfNull(this Element element)
        {
            if (element == null)
                Engine.Reflection.Compute.RecordError(string.Format("BHoM object could not be read because Revit element with Id {0} does not exist.", element.Id.IntegerValue));
        }

        /***************************************************/

        private static void CheckIfNull(this Document document)
        {
            if (document == null)
                Engine.Reflection.Compute.RecordError(string.Format("BHoM object could not be read because Revit document with title {0} does not exist.", document.Title));
        }

        /***************************************************/

        private static void CheckIfNull(this StructuralMaterialType structuralMaterialType)
        {
            Engine.Reflection.Compute.RecordError("BHoM object could not be read because Revit structural material type " + structuralMaterialType + " does not exist.");
        }

        /***************************************************/
    }
}