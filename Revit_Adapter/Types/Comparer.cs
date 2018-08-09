using System;
using System.Collections.Generic;

namespace BH.Adapter.Revit
{

    public partial class RevitAdapter
    {
        /***************************************************/
        /**** BHoM Adapter Interface                    ****/
        /***************************************************/

        protected override IEqualityComparer<T> Comparer<T>()
        {
            Type type = typeof(T);

            if (m_Comparers.ContainsKey(type))
            {
                return m_Comparers[type] as IEqualityComparer<T>;
            }
            else
            {
                return EqualityComparer<T>.Default;
            }

        }


        /***************************************************/
        /**** Private Fields                            ****/
        /***************************************************/

        private static Dictionary<Type, object> m_Comparers = new Dictionary<Type, object>
        {
            //{typeof(ISectionProperty), new BHoMObjectNameOrToStringComparer() },
            //{typeof(Material), new BHoMObjectNameComparer() },
            //{typeof(LinkConstraint), new BHoMObjectNameComparer() },
        };
        
        /***************************************************/
    }
}
