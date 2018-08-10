using BH.oM.Common.Materials;
using BH.oM.Structural.Elements;
using BH.oM.Architecture.Elements;
using BH.oM.Structural.Properties;
using System;
using System.Collections.Generic;

namespace BH.UI.Cobra.Adapter
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
            //{typeof(FramingElement), new List<Type> { typeof(ISectionProperty), typeof(Level) } },
            //{typeof(ISectionProperty), new List<Type> { typeof(Material), typeof(IProfile) } },
            //{typeof(PanelPlanar), new List<Type> { typeof(IProperty2D), typeof(Level) } },
            //{typeof(IProperty2D), new List<Type> { typeof(Material) } }
        };
        
        /***************************************************/
    }
}