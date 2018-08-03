using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace BH.Engine.Revit
{
    public static partial class Compute
    {
        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        internal static void NotConvertedError(this Element element)
        {
            Engine.Reflection.Compute.RecordError(string.Format("Revit element could not be converted because conversion method does not exist. Element Id: {0}, Element Name: {1}", element.Id.IntegerValue, element.Name));
        }

        /***************************************************/

        internal static void NotConvertedError(this Document document)
        {
            Engine.Reflection.Compute.RecordError(string.Format("Revit document could not be converted because conversion method does not exist. Document title: {0}", document.Title));
        }

        /***************************************************/

        internal static void NotConvertedError(this StructuralMaterialType structuralMaterialType)
        {
            Engine.Reflection.Compute.RecordError("Structural meterial type " + structuralMaterialType + " could not be converted because conversion method does not exist.");
        }

        /***************************************************/

        internal static void CheckIfNull(this Element element)
        {
            if (element == null)
                Engine.Reflection.Compute.RecordError(string.Format("BHoM object could not be read because Revit element with Id {0} does not exist.", element.Id.IntegerValue));
        }

        /***************************************************/

        internal static void CheckIfNull(this Document document)
        {
            if (document == null)
                Engine.Reflection.Compute.RecordError(string.Format("BHoM object could not be read because Revit document with title {0} does not exist.", document.Title));
        }

        /***************************************************/

        internal static void NonlinearBarError(this FamilyInstance bar)
        {
            Engine.Reflection.Compute.RecordError(string.Format("Nonlinear bars are currently not supported in BHoM, the object is returned with empty geometry. Element Id: {0}", bar.Id.IntegerValue));
        }

        /***************************************************/

        internal static void UnsupportedOutlineCurveError(this HostObject hostObject)
        {
            Reflection.Compute.RecordError(string.Format("The panel outline contains a curve that is currently not supported in BHoM, the object is returned with empty geometry. Element Id: {0}", hostObject.Id.IntegerValue));
        }

        /***************************************************/
    }
}