using BH.oM.Base;
using BH.oM.DataManipulation.Queries;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Duplicates given BHoMObject and removes its identity data (ElementId, AdapterId).")]
        [Input("bHoMObject", "BHoMObject")]
        [Output("BHoMObject")]
        public static IBHoMObject Duplicate(this IBHoMObject bHoMObject)
        {
            if (bHoMObject == null)
                return null;

            IBHoMObject aBHoMObject = bHoMObject.GetShallowClone();

            aBHoMObject.CustomData.Remove(Convert.ElementId);
            aBHoMObject.CustomData.Remove(Convert.AdapterId);


            return aBHoMObject;
        }

        /***************************************************/

        [Description("Duplicates FilterQuery.")]
        [Input("filterQuery", "FilterQuery")]
        [Output("FilterQuery")]
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
