using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Compute
    {
        /***************************************************/
        /****              Public Methods               ****/
        /***************************************************/

        [Description("Tries to extract a value from an BHoMObject based on the instruction embedded in the provided " + nameof(ParameterValueSource) + ".")]
        [Input("bHoMObject", "bHoMObject to extract the value from.")]
        [Input("valueSource", "Object defining how to extract the value from the input object.")]
        [MultiOutput(0, "found", "True if value source exists in the input object (i.e. value could be extracted from the object), otherwise false.")]
        [MultiOutput(1, "value", "Value extracted from the input object based on the provided instruction.")]
        public static Output<bool, object> TryGetValueFromSource(this IBHoMObject bHoMObject, ParameterValueSource valueSource)
        {
            if (bHoMObject == null)
            {
                BH.Engine.Base.Compute.RecordError("Could not extract value from a null element.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(valueSource?.ParameterName))
            {
                BH.Engine.Base.Compute.RecordError("Could not extract value based on an empty value source.");
                return null;
            }

            if (valueSource.FromType == true)
            {
                IBHoMObject type = bHoMObject.IGetRevitElementType();
                if (type != null)
                    bHoMObject = type;
                else
                {
                    BH.Engine.Base.Compute.RecordNote($"BHoM object with Guid {bHoMObject.BHoM_Guid} does not have a property representing Revit type, so Revit type parameter cannot be queried.");
                    return new Output<bool, object> { Item1 = false, Item2 = null };
                }
            }

            RevitParameter param = bHoMObject.GetRevitParameter(valueSource.ParameterName);
            if (param == null && valueSource.FromType == null)
            {
                param = bHoMObject.IGetRevitElementType()?.GetRevitParameter(valueSource.ParameterName);
                if (param != null)
                    BH.Engine.Base.Compute.RecordNote($"Parameter {valueSource.ParameterName} was not found in the instance of object with Guid {bHoMObject.BHoM_Guid}, but was found in the type.");
            }

            if (param == null)
            {
                BH.Engine.Base.Compute.RecordNote($"Element with id {bHoMObject.ElementId()} does not have a parameter named {valueSource.ParameterName}.");
                return new Output<bool, object> { Item1 = false, Item2 = null };
            }
            else
                return new Output<bool, object> { Item1 = true, Item2 = param.Value };
        }

        /***************************************************/
    }
}
