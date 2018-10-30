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

        internal static CompoundStructure ToRevitCompoundStructure(IEnumerable<ConstructionLayer> constructionLayers, Document document, PushSettings pushSettings = null)
        {
            List<CompoundStructureLayer> aCompoundStructureLayerList = new List<CompoundStructureLayer>();
            foreach (ConstructionLayer aConstructionLayer in constructionLayers)
                aCompoundStructureLayerList.Add(aConstructionLayer.ToRevit(document, pushSettings));

            return CompoundStructure.CreateSimpleCompoundStructure(aCompoundStructureLayerList);
        }
    }
}