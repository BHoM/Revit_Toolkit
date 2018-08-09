using System.Collections.Generic;

using BH.oM.Environment.Elements;
using BH.oM.Environment.Interface;

using Autodesk.Revit.DB;
using BH.oM.Base;
using BH.oM.Adapters.Revit;

namespace BH.Engine.Revit
{

    /// <summary>
    /// BHoM Revit Engine Convert Methods
    /// </summary>
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        internal static CompoundStructureLayer ToRevit(this ConstructionLayer constructionLayer, Document document, PushSettings pushSettings = null)
        {
            if (pushSettings == null)
                pushSettings = PushSettings.Default;

            MaterialFunctionAssignment aMaterialFunctionAssignment = GetMaterialFunctionAssignment(constructionLayer);

            return new CompoundStructureLayer(UnitUtils.ConvertToInternalUnits(constructionLayer.Thickness, DisplayUnitType.DUT_METERS), aMaterialFunctionAssignment, constructionLayer.Material.ToRevit(document).Id);
        }

        /***************************************************/

        internal static CompoundStructure ToRevit(IEnumerable<ConstructionLayer> constructionLayers, Document document, PushSettings pushSettings = null)
        {
            if (pushSettings == null)
                pushSettings = PushSettings.Default;

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
