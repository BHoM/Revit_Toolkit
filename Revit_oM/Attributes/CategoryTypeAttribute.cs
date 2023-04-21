using BH.oM.Base;
using System;
using System.ComponentModel;

namespace BH.oM.Revit.Attributes
{
    [Description("Attribute that defines Revit category type as defined in Visibility & Graphics window." +
                 "\nAllowed values of Type property are: 'Model', 'Annotation', 'Analytical' and 'Internal'." +
                 "\nEach category added to " + nameof(Enums.Category) + " needs to have " + nameof(CategoryTypeAttribute) + " assigned to be successfully reflected.")]
    public class CategoryTypeAttribute : Attribute, IImmutable, IObject
    {
        /***************************************************/
        /****             Public properties             ****/
        /***************************************************/

        public string Type
        {
            get
            {
                return m_Type;
            }
        }

        /***************************************************/
        /****                Constructors               ****/
        /***************************************************/

        public CategoryTypeAttribute(string type)
        {
            m_Type = type;
        }


        /***************************************************/
        /****              Private fields               ****/
        /***************************************************/

        private readonly string m_Type;

        /***************************************************/
    }
}
