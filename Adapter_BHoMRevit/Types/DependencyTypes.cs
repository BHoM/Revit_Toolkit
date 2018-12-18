using System;
using System.Collections.Generic;

using BH.oM.Adapters.Revit.Elements;

namespace BH.UI.Revit.Adapter
{
    public partial class CobraAdapter
    {
        /***************************************************/
        /**** BHoM Adapter Interface                    ****/
        /***************************************************/

        protected override List<Type> DependencyTypes<T>()
        {
            Type type = typeof(T);

            if (m_DependencyTypes.ContainsKey(type))
                return m_DependencyTypes[type];

            else if (m_DependencyTypes.ContainsKey(type.BaseType))
                return m_DependencyTypes[type.BaseType];

            else
            {
                foreach (Type interType in type.GetInterfaces())
                {
                    if (m_DependencyTypes.ContainsKey(interType))
                        return m_DependencyTypes[interType];
                }
            }


            return new List<Type>();
        }


        /***************************************************/
        /**** Private Fields                            ****/
        /***************************************************/

        private static Dictionary<Type, List<Type>> m_DependencyTypes = new Dictionary<Type, List<Type>>
        {
            {typeof(Viewport), new List<Type> { typeof(Sheet), typeof(ViewPlan) } },
            {typeof(Sheet), new List<Type> { typeof(ViewPlan)} }
            //{typeof(ISectionProperty), new List<Type> { typeof(Material), typeof(IProfile) } },
            //{typeof(PanelPlanar), new List<Type> { typeof(ISurfaceProperty), typeof(Level) } },
            //{typeof(ISurfaceProperty), new List<Type> { typeof(Material) } }
        };

        /***************************************************/
    }
}