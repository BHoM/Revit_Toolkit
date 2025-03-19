using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        public static bool IsEnumParameter(this Parameter parameter)
        {
            if (parameter?.StorageType != StorageType.Integer)
                return false;

            ForgeTypeId dataType = parameter.Definition.ParameterType();
#if REVIT2021
            return dataType == null || (dataType == SpecTypeId.Number && !int.TryParse(parameter.AsValueString(), out _));
#else
            return dataType == null;
#endif
        }
    }
}
