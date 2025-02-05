using BH.oM.Data.Requests;
using BH.oM.Verification.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Adapters.Revit.Requests
{
    //TODO: move to data engine??
    //TODO: add Create methods that would reflect the previous constructors and add versioning if possible
    public class ConditionRequest : IRequest
    {
        public virtual ICondition Condition { get; set; } = null;
    }
}
