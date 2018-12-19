using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Internal Methods                          ****/
        /***************************************************/

        internal static CompoundStructureLayer ToRevitCompoundStructureLayer(this BH.oM.Environment.Elements.Construction constructionLayer, Document document, PushSettings pushSettings = null)
        {
            MaterialFunctionAssignment aMaterialFunctionAssignment = GetMaterialFunctionAssignment(constructionLayer);

            return new CompoundStructureLayer(UnitUtils.ConvertToInternalUnits(constructionLayer.Thickness, DisplayUnitType.DUT_METERS), aMaterialFunctionAssignment, (constructionLayer.Materials.Count > 0 ? constructionLayer.Materials[0].ToRevitMaterial(document, pushSettings).Id : null));
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static MaterialFunctionAssignment GetMaterialFunctionAssignment(BH.oM.Environment.Elements.Construction constructionLayer)
        {
            return MaterialFunctionAssignment.Structure;
        }

        /***************************************************/
    }
}