using Autodesk.Revit.DB;
using BH.oM.Base;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static WorksetId WorksetId(this BHoMObject bHoMObject)
        {
            if (bHoMObject == null)
                return null;

            object aValue = null;
            if (bHoMObject.CustomData.TryGetValue(BH.Engine.Adapters.Revit.Convert.WorksetId, out aValue))
            {
                if (aValue is string)
                {
                    int aInt = -1;
                    if (int.TryParse((string)aValue, out aInt))
                        return new WorksetId(aInt);
                }
                else if (aValue is int)
                {
                    return new WorksetId((int)aValue);
                }
                else
                {
                    return null;
                }
            }

            return null;
        }

        /***************************************************/

        public static WorksetId WorksetId(this Document document, string worksetName)
        {
            if (document == null || string.IsNullOrEmpty(worksetName))
                return null;

            Workset aWorkset = new FilteredWorksetCollector(document).OfKind(WorksetKind.UserWorkset).First(x => x.Name == worksetName);
            if (aWorkset == null)
                return null;

            return aWorkset.Id;
        }

        /***************************************************/
    }
}
