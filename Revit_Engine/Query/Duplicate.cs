using BH.oM.Base;
using BH.oM.DataManipulation.Queries;
using System.Collections.Generic;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static IBHoMObject Duplicate(this IBHoMObject BHoMObject)
        {
            if (BHoMObject == null)
                return null;

            IBHoMObject aBHoMObject = BHoMObject.GetShallowClone();

            //aBHoMObject = aBHoMObject.SetCustomData(BH.Engine.Adapters.Revit.Convert.ElementId, element.Id.IntegerValue);
            //aBHoMObject = aBHoMObject.SetCustomData(BH.Engine.Adapters.Revit.Convert.AdapterId, element.UniqueId);

            aBHoMObject.CustomData.Remove(Convert.ElementId);
            aBHoMObject.CustomData.Remove(Convert.AdapterId);


            return aBHoMObject;
        }

        /***************************************************/

        public static FilterQuery Duplicate(this FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return null;

            FilterQuery aFilterQuery = new FilterQuery();

            if (filterQuery.Equalities != null)
                aFilterQuery.Equalities = new Dictionary<string, object>(filterQuery.Equalities);
            else
                aFilterQuery.Equalities = new Dictionary<string, object>();

            aFilterQuery.Tag = filterQuery.Tag;

            aFilterQuery.Type = filterQuery.Type;


            return aFilterQuery;
        }

        /***************************************************/
    }
}
