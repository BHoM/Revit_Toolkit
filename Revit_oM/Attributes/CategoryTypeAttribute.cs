using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Revit.Attributes
{
    public class CategoryTypeAttribute : Attribute, IImmutable, IObject
    {
        private readonly string m_Type;

        public string Type
        {
            get
            {
                return m_Type;
            }
        }

        public CategoryTypeAttribute(string type)
        {
            m_Type = type;
        }
    }
}
