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
    }
}