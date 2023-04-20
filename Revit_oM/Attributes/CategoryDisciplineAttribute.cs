using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Revit.Attributes
{
    public class CategoryDisciplineAttribute : Attribute, IImmutable, IObject
    {
        private readonly string[] m_Disciplines;

        public string[] Disciplines
        {
            get 
            {
                return m_Disciplines; 
            }
        }

        public CategoryDisciplineAttribute(params string[] disciplines)
        {
            m_Disciplines = disciplines;
        }
    }
}
