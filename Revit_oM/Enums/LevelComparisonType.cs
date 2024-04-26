using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Revit.Enums
{
    /***************************************************/

    [Description("Enumerator defining the way in which two levels are compared.")]
    public enum LevelComparisonType
    {
        [Description("Check if input and reference level are at the same elevation.")]
        Equal,
        [Description("Check if input and reference level are at different elevations.")]
        NotEqual,
        [Description("Check if the input level is above the reference level.")]
        Above,
        [Description("Check if the input level is above or at the same elevation of the reference level.")]
        AtOrAbove,
        [Description("Check if the input level is below the reference level.")]
        Below,
        [Description("Check if the input level is below or at the same elevation of the reference level.")]
        AtOrBelow,
    }

    /***************************************************/
}
