using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.UI.Revit.Adapter
{
    public partial class CobraAdapter
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        //public override int UpdateProperty(Type type, IEnumerable<object> ids, string property, object newValue)
        //{

        //    if (property == "Tags")
        //    {
        //        List<int> indecies = ids.Select(x => (int)x).ToList();
        //        if (indecies.Count < 1)
        //            return 0;

        //        List<HashSet<string>> tags = (newValue as IEnumerable<HashSet<string>>).ToList();
        //        return UpdateTags(type, indecies, tags);
        //    }

        //    return 0;
        //}

        ///***************************************************/
        ///**** Private Methods                           ****/
        ///***************************************************/

        //private int UpdateTags(Type t, List<int> indecies, List<HashSet<string>> tags)
        //{
        //    Dictionary<int, HashSet<string>> typeTags = this.GetTypeTags(t);// = m_tags[t];

        //    for (int i = 0; i < indecies.Count; i++)
        //    {
        //        typeTags[indecies[i]] = tags[i];
        //    }

        //    return indecies.Count;
        //}

        /***************************************************/
    }
}
