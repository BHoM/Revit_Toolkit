using Autodesk.Revit.DB;
using BH.oM.Base;
using BH.oM.Adapters.Revit;
using BH.oM.Structure.Elements;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BH.UI.Cobra.Engine
{
    public static partial class Compute
    {
        /***************************************************/
        /****               Public methods              ****/
        /***************************************************/

        public static void DefaultIfNull(this PullSettings pullSettings)
        {
            if (pullSettings == null)
            {
                BH.Engine.Reflection.Compute.RecordWarning("Pull settings are not set. Default settings are used.");
                pullSettings = PullSettings.Default;
            }
        }

        /***************************************************/

        public static void DefaultIfNull(this PushSettings pushSettings)
        {
            if (pushSettings == null)
            {
                BH.Engine.Reflection.Compute.RecordWarning("Push settings are not set. Default settings are used.");
                pushSettings = PushSettings.Default;
            }
        }


        /***************************************************/
        /****             Internal methods              ****/
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

        internal static void ElementCouldNotBeQueried(this Element element)
        {
            string aMessage = "Revit element could not be queried.";

            if (element != null)
                aMessage = string.Format("{0} Element Id: {1}", aMessage, element.Id.IntegerValue);

            BH.Engine.Reflection.Compute.RecordWarning(aMessage);
        }

        /***************************************************/
    }
}