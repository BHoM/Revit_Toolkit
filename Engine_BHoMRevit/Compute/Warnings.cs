using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using BH.oM.Base;
using BH.oM.Structure.Elements;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BH.UI.Revit.Engine
{
    public static partial class Compute
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static void NotConvertedWarning(this Element element)
        {
            string aMessage = "Revit element could not be converted because conversion method does not exist.";

            if (element != null)
                aMessage = string.Format("{0} Element Id: {1}, Element Name: {2}", aMessage, element.Id.IntegerValue, element.Name);

            BH.Engine.Reflection.Compute.RecordWarning(aMessage);
        }

        /***************************************************/

        internal static void NotConvertedWarning(this StructuralMaterialType structuralMaterialType)
        {
            BH.Engine.Reflection.Compute.RecordWarning("Structural meterial type " + structuralMaterialType + " could not be converted because conversion method does not exist.");
        }

        /***************************************************/

        internal static void CheckIfNullPull(this Element element)
        {
            if (element == null)
                BH.Engine.Reflection.Compute.RecordWarning("BHoM object could not be read because Revit element does not exist.");
        }

        /***************************************************/

        internal static void CheckIfNullPush(this Element element, IBHoMObject BHoMObject)
        {
            if (element == null)
                BH.Engine.Reflection.Compute.RecordWarning(string.Format("Revit element has not been created due to BHoM/Revit conversion issues. BHoM element Guid: {0}", BHoMObject.BHoM_Guid));
        }

        /***************************************************/

        internal static void NullObjectWarning()
        {
            BH.Engine.Reflection.Compute.RecordWarning("BHoM object could not be created becasue Revit object is null.");
        }

        /***************************************************/

        internal static void LogNullProperties(this BHoMObject obj, IEnumerable<string> propertyNames = null)
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
                BH.Engine.Reflection.Compute.RecordWarning(warning);
            }
        }

        /***************************************************/

        internal static void UnknownMaterialWarning(this FamilyInstance familyInstance)
        {
            BH.Engine.Reflection.Compute.RecordWarning(string.Format("Revit symbol has been converted to a steel profile with an unknown material. Element Id: {0}, Element Name: {1}", familyInstance.Id.IntegerValue, familyInstance.Name));
        }

        /***************************************************/

        internal static void MaterialNotFoundWarning(this string materialGrade)
        {
            BH.Engine.Reflection.Compute.RecordWarning(string.Format("A BHoM equivalent to the Revit material has not been found. Material  grade: {0}", materialGrade));
        }

        /***************************************************/

        internal static void CompositePanelWarning(this HostObjAttributes hostObjAttributes)
        {
            BH.Engine.Reflection.Compute.RecordWarning(string.Format("Composite panels are currently not supported in BHoM. A zero thickness panel is created. Element type Id: {0}", hostObjAttributes.Id.IntegerValue));
        }

        /***************************************************/

        internal static void OpeningInPanelWarning(this PanelPlanar panelPlanar)
        {
            BH.Engine.Reflection.Compute.RecordWarning(string.Format("In current implementation of BHoM the panels are pushed without openings. {0} openings are skipped for the panel with BHoM_Guid: {1}", panelPlanar.Openings.Count, panelPlanar.BHoM_Guid));
        }

        /***************************************************/

        internal static void ConvertProfileFailedWarning(this FamilySymbol familySymbol)
        {
            BH.Engine.Reflection.Compute.RecordWarning(string.Format("Revit family symbol conversion to BHoM profile failed, zero profile is returned. Family symbol Id: {0}", familySymbol.Id.IntegerValue));
        }

        /***************************************************/

        internal static void NonlinearBarWarning(this FamilyInstance bar)
        {
            string aMessage = "Nonlinear bars are currently not supported in BHoM, the object is returned with empty geometry.";

            if (bar != null)
                aMessage = string.Format("{0} Element Id: {1}", aMessage, bar.Id.IntegerValue);

            BH.Engine.Reflection.Compute.RecordWarning(aMessage);
        }

        /***************************************************/

        internal static void BarCurveNotFoundWarning(this FamilyInstance bar)
        {
            string aMessage = "Bar curve could not be retrieved, the object is returned with empty geometry.";

            if (bar != null)
                aMessage = string.Format("{0} Element Id: {1}", aMessage, bar.Id.IntegerValue);

            BH.Engine.Reflection.Compute.RecordWarning(aMessage);
        }

        /***************************************************/

        internal static void UnsupportedOutlineCurveWarning(this HostObject hostObject)
        {
            string aMessage = "The panel outline contains a curve that is currently not supported in BHoM, the object is returned with empty geometry.";

            if (hostObject != null)
                aMessage = string.Format("{0} Element Id: {1}", aMessage, hostObject.Id.IntegerValue);

            BH.Engine.Reflection.Compute.RecordWarning(aMessage);
        }

        /***************************************************/

        internal static void NonClosedOutlineWarning(this HostObject hostObject)
        {
            string aMessage = "The panel outline is not closed, the object is returned with empty geometry.";

            if (hostObject != null)
                aMessage = string.Format("{0} Element Id: {1}", aMessage, hostObject.Id.IntegerValue);

            BH.Engine.Reflection.Compute.RecordWarning(aMessage);
        }

        /***************************************************/

        internal static void ElementCouldNotBeQueriedWarning(this Element element)
        {
            string aMessage = "Revit element could not be queried.";

            if (element != null)
                aMessage = string.Format("{0} Element Id: {1}", aMessage, element.Id.IntegerValue);

            BH.Engine.Reflection.Compute.RecordWarning(aMessage);
        }

        /***************************************************/
    }
}