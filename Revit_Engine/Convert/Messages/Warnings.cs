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

            if (nullPropertyNames.Count > 0)
            {
                string warning = string.Format("The BHoM object if missing following properties: {0}. BHoM_Guid: {1}.", string.Join(", ", nullPropertyNames), obj.BHoM_Guid);

                ElementId revitId = obj.ElementId();
                if (revitId != null) warning += string.Format(" Revit ElementId: {0}.", revitId.IntegerValue);
                Reflection.Compute.RecordWarning(warning);
            }
        }

        /***************************************************/

        private static void UnknownMaterialWarning(this FamilyInstance familyInstance)
        {
            Engine.Reflection.Compute.RecordWarning(string.Format("Revit symbol has been converted to a steel profile with an unknown material. Element Id: {0}, Element Name: {1}", familyInstance.Id.IntegerValue, familyInstance.Name));
        }

        /***************************************************/

        private static void MaterialNotFoundWarning(this string materialGrade)
        {
            Engine.Reflection.Compute.RecordWarning(string.Format("A BHoM equivalent to the Revit material has not been found. Material  grade: {0}", materialGrade));
        }

        /***************************************************/

        private static void CompositePanelWarning(this HostObjAttributes hostObjAttributes)
        {
            Engine.Reflection.Compute.RecordWarning(string.Format("Composite panels are currently not supported in the BHoM. Element type Id: {0}", hostObjAttributes.Id.IntegerValue));
        }

        /***************************************************/

        private static void NonlinearBarWarning(this FamilyInstance bar)
        {
            Engine.Reflection.Compute.RecordWarning(string.Format("Nonlinear bars are currently not supported in the BHoM. Element Id: {0}", bar.Id.IntegerValue));
        }

        /***************************************************/
    }
}