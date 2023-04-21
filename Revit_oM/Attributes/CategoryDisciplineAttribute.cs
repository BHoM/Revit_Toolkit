﻿using BH.oM.Base;
using System;
using System.ComponentModel;

namespace BH.oM.Revit.Attributes
{
    [Description("Attribute that defines disciplines, to which a category belongs as defined in Visibility & Graphics window." +
                 "\nAllowed values of Disciplines property are: 'Architecture', 'Structure', 'Mechanical', 'Electrical', 'Piping' and 'Infrastructure'." +
                 "\nEach category added to " + nameof(Enums.Category) + " needs to have " + nameof(CategoryDisciplineAttribute) + " assigned to be successfully reflected.")]
    public class CategoryDisciplineAttribute : Attribute, IImmutable, IObject
    {
        /***************************************************/
        /****             Public properties             ****/
        /***************************************************/

        public virtual string[] Disciplines { get; }

        /***************************************************/
        /****                Constructors               ****/
        /***************************************************/

        public CategoryDisciplineAttribute(params string[] disciplines)
        {
            Disciplines = disciplines;
        }

        /***************************************************/
    }
}
