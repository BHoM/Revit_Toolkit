using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using BH.oM.Base;
using System.Collections.Generic;
using System.Reflection;
using System;

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

        private static void CheckNullProperties(this BHoMObject obj, IEnumerable<string> propertyNames = null)
        {
            //TODO: Move this one to the BHoM_Engine?
            List<string> nullPropertyNames = new List<string>();

            Type type = obj.GetType();
            if (propertyNames == null)
            {
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo pi in properties)
                {
                    if (pi.GetValue(obj) == null) nullPropertyNames.Add(pi.Name);
                }
            }
            else
            {
                foreach (string propertyName in propertyNames)
                {
                    if (type.GetProperty(propertyName).GetValue(obj) == null) nullPropertyNames.Add(propertyName);
                }
            }
            
            string warning = string.Format("The BHoM object if missing following properties: {0}. BHoM_Guid: {1}.", string.Join(", ", nullPropertyNames), obj.BHoM_Guid);

            ElementId revitId = obj.ElementId();
            if (revitId != null) warning += string.Format(" Revit ElementId: {0}.", revitId);
            Engine.Reflection.Compute.RecordWarning(warning);
        }

        /***************************************************/
    }
}