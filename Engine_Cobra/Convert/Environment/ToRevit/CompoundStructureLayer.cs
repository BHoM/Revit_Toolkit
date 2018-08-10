using Autodesk.Revit.DB;
using BH.oM.Environment.Elements;
using BH.oM.Revit;
using System.Collections.Generic;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Internal Methods                          ****/
        /***************************************************/

        internal static CompoundStructureLayer ToRevit(this ConstructionLayer constructionLayer, Document document, PushSettings pushSettings = null)
        {
            pushSettings.DefaultIfNull();

            MaterialFunctionAssignment aMaterialFunctionAssignment = GetMaterialFunctionAssignment(constructionLayer);

            return new CompoundStructureLayer(UnitUtils.ConvertToInternalUnits(constructionLayer.Thickness, DisplayUnitType.DUT_METERS), aMaterialFunctionAssignment, constructionLayer.Material.ToRevit(document, pushSettings).Id);
        }

        /***************************************************/

        internal static CompoundStructure ToRevit(IEnumerable<ConstructionLayer> constructionLayers, Document document, PushSettings pushSettings = null)
        {
            pushSettings.DefaultIfNull();

            List<CompoundStructureLayer> aCompoundStructureLayerList = new List<CompoundStructureLayer>();
            foreach (ConstructionLayer aConstructionLayer in constructionLayers)
                aCompoundStructureLayerList.Add(aConstructionLayer.ToRevit(document, pushSettings));

            return CompoundStructure.CreateSimpleCompoundStructure(aCompoundStructureLayerList);
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