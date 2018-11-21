using System.ComponentModel;

using BH.oM.Reflection.Attributes;
using BH.oM.Environment.Properties;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Sets Family Type name for Builiding Element Properties")]
        [Input("buildingElementProperties", "Building Element Properties")]
        [Input("familyTypeName", "Revit Family Type Name")]
        [Output("BuildingElementProperties")]
        public static BuildingElementProperties SetFamilyTypeName(this BuildingElementProperties buildingElementProperties, string familyTypeName)
        {
            if (buildingElementProperties == null)
                return null;

            BuildingElementProperties aBuildingElementProperties = buildingElementProperties.GetShallowClone() as BuildingElementProperties;

            if (aBuildingElementProperties.CustomData.ContainsKey(Convert.FamilyTypeName))
                aBuildingElementProperties.CustomData[Convert.FamilyTypeName] = familyTypeName;
            else
                aBuildingElementProperties.CustomData.Add(Convert.FamilyTypeName, familyTypeName);

            return aBuildingElementProperties;
        }

        /***************************************************/
    }
}
