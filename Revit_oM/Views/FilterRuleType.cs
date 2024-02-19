using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Revit.Views
{
    public enum FilterRuleType
    {
        EQUALS,
        GREATER,
        GREATER_OR_EQUAL,
        LESS,
        LESS_OR_EQUAL,
        CONTAINS,
        NOT_CONTAINS,
        BEGINSWITH,
        NOT_BEGINSWITH,
        ENDSWITH,
        NOT_ENDSWITH
    }
}
