using Autodesk.Revit.DB;
using BH.oM.Environment.Elements;
using BH.oM.Adapters.Revit.Settings;
using System.Collections.Generic;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Internal Methods                          ****/
        /***************************************************/

        internal static CompoundStructureLayer ToRevitCompoundStructureLayer(this ConstructionLayer constructionLayer, Document document, PushSettings pushSettings = null)
        {
            MaterialFunctionAssignment aMaterialFunctionAssignment = GetMaterialFunctionAssignment(constructionLayer);

            return new CompoundStructureLayer(UnitUtils.ConvertToInternalUnits(constructionLayer.Thickness, DisplayUnitType.DUT_METERS), aMaterialFunctionAssignment, constructionLayer.Material.ToRevitMaterial(document, pushSettings).Id);
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static MaterialFunctionAssignment GetMaterialFunctionAssignment(ConstructionLayer constructionLayer)
        {
            return MaterialFunctionAssignment.Structure;
        }

        /***************************************************/
    }
}