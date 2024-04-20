using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Revit.Views
{
    public enum FilterRuleType
    {
        VALUE_STRINGRULE,
        VALUE_NUMERICRULE_DOUBLE,
        VALUE_NUMERICRULE_ELEMENTID,
        VALUE_NUMERICRULE_INTEGER,
        VALUE_GLOBALPARAMASSOCIATION,
        INVERSE,
        CATEGORY,
        PARAMVALUEPRESENCE_NOVALUE,
        PARAMVALUEPRESENCE_VALUE,
        SHAREDPARAM_APPLICABLE
    }
}
