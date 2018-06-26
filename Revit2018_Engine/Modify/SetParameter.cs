using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using BH.oM.Base;

namespace BH.Engine.Revit
{
    /// <summary>
    /// BHoM Revit Engine Modify Methods
    /// </summary>
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        /// <summary>
        /// Mutable method, Copy Value to Revit Parameter. If parameter has units then value needs to be in Revit internal units: use UnitUtils.ConvertToInternalUnits (Autodesk.Revit.DB) to convert units
        /// </summary>
        /// <param name="parameter">Revit Parameter</param>
        /// <param name="value">Value for parameter to be set. StorageType: 1. Double - Value type of double, int, bool, string 2. ElementId - Value type of int, string, 3. Integer - Value type of double, int, bool, string 4. String - value type of string, object</param>
        /// <param name="convertUnits">Change units from SI</param>
        /// <returns name="Parameter">Revit Parameter</returns>
        /// <search>
        /// Modify, SetParameter, Revit, Set Parameter, Parameter
        /// </search>
        public static Parameter SetParameter(this Parameter parameter, object value, bool convertUnits = true)
        {
            if (parameter == null)
                return null;

            switch (parameter.StorageType)
            {
                case StorageType.Double:
                    double aDouble = double.NaN;
                    if (value is double || value is int)
                    {
                        aDouble = (double)value;
                    }
                    else if (value is bool)
                    {
                        if ((bool)value)
                            aDouble = 1.0;
                        else
                            aDouble = 0.0;
                    }
                    else if (value is string)
                    {
                        if (!double.TryParse((string)value, out aDouble))
                            aDouble = double.NaN;
                    }

                    if (!double.IsNaN(aDouble))
                    {
                        try
                        {
                            aDouble = Convert.FromSI(aDouble, parameter.Definition.UnitType);
                        }
                        catch
                        {

                        }
                    }
                        
                    parameter.Set(aDouble);
                    break;
                case StorageType.ElementId:
                    if (value is int)
                    {
                        parameter.Set(new ElementId((int)value));
                    }
                    else if (value is string)
                    {
                        int aInt;
                        if (int.TryParse((string)value, out aInt))
                        {
                            parameter.Set(new ElementId(aInt));
                        }

                    }
                    break;
                case StorageType.Integer:
                    if (value is double || value is int)
                    {
                        parameter.Set((int)value);
                    }
                    else if (value is bool)
                    {
                        if ((bool)value)
                            parameter.Set(1);
                        else
                            parameter.Set(0);
                    }
                    else if (value is string)
                    {
                        int aInt = 0;
                        if (int.TryParse((string)value, out aInt))
                            parameter.Set(aInt);
                    }
                    break;

                case StorageType.String:
                    if (value == null)
                    {
                        string aString = null;
                        parameter.Set(aString);
                    }
                    else if (value is string)
                    {
                        parameter.Set((string)value);
                    }
                    else
                    {
                        parameter.Set(value.ToString());
                    }
                    break;

            }

            return parameter;
        }

        /***************************************************/
    }
}
