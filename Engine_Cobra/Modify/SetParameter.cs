
using Autodesk.Revit.DB;

namespace BH.UI.Cobra.Engine
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static Parameter SetParameter(this Parameter parameter, object value, Document Document, bool convertUnits = true)
        {
            if (parameter == null || parameter.IsReadOnly)
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
                        if(convertUnits)
                        {
                            try
                            {
                                aDouble = Convert.FromSI(aDouble, parameter.Definition.UnitType);
                            }
                            catch
                            {
                                aDouble = double.NaN;
                            }
                        }

                        if(!double.IsNaN(aDouble))
                        {
                            parameter.Set(aDouble);
                            return parameter;
                        }
                    }
                    break;
                case StorageType.ElementId:
                    ElementId aElementId = null;
                    if (value is int)
                    {
                        aElementId = new ElementId((int)value);
                    }
                    else if (value is string)
                    {
                        int aInt;
                        if (int.TryParse((string)value, out aInt))
                            aElementId = new ElementId(aInt);
                    }
                    else if(value != null)
                    {
                        int aInt;
                        if (int.TryParse(value.ToString(), out aInt))
                            aElementId = new ElementId(aInt);
                    }

                    if(aElementId != null)
                    {
                        bool aExists = false;
                        if (aElementId == ElementId.InvalidElementId)
                            aExists = true;

                        if(!aExists)
                        {
                            if (Document == null)
                            {
                                aExists = true;
                            }
                            else
                            {
                                Element aElement = Document.GetElement(aElementId);
                                aExists = aElement != null;
                            }
                                
                        }

                        if(aExists)
                        {
                            parameter.Set(aElementId);
                            return parameter;
                        }
                    }
                    break;
                case StorageType.Integer:
                    if (value is double || value is int)
                    {
                        parameter.Set((int)value);
                        return parameter;
                    }
                    else if (value is bool)
                    {
                        if ((bool)value)
                            parameter.Set(1);
                        else
                            parameter.Set(0);

                        return parameter;
                    }
                    else if (value is string)
                    {
                        int aInt = 0;
                        if (int.TryParse((string)value, out aInt))
                        {
                            parameter.Set(aInt);
                            return parameter;
                        }  
                    }
                    break;
                case StorageType.String:
                    if (value == null)
                    {
                        string aString = null;
                        parameter.Set(aString);
                        return parameter;
                    }
                    else if (value is string)
                    {
                        parameter.Set((string)value);
                        return parameter;
                    }
                    else
                    {
                        parameter.Set(value.ToString());
                        return parameter;
                    }
                    break;
            }

            return null;
        }

        /***************************************************/
    }
}