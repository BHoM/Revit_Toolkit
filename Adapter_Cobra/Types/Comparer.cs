using BH.oM.Common.Materials;
using BH.oM.Architecture.Elements;
using BH.oM.Structural.Properties;
using System;
using System.Collections.Generic;
using BH.Engine.Base.Objects;

namespace BH.UI.Cobra.Adapter
{
    public partial class CobraAdapter
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
            //{typeof(IProfile), new BHoMObjectNameOrToStringComparer() },
            //{typeof(IProperty2D), new BHoMObjectNameOrToStringComparer() },
            //{typeof(Material), new BHoMObjectNameComparer() },
            //{typeof(Level), new BHoMObjectNameComparer() },
        };
        
        /***************************************************/
    }
}
