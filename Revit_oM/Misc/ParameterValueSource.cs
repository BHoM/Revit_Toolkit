using BH.oM.Verification.Conditions;

namespace BH.oM.Adapters.Revit
{
    //TODO: add ConvertUnits prop to reflect the behaviour of FilterByParameterNumber? could actually be welcome, potentially helpful in other contexts as well
    public class ParameterValueSource : IValueSource
    {
        public virtual string ParameterName { get; set; } = "";

        public virtual bool FromType { get; set; } = false;
    }
}
